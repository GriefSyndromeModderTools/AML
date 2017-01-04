using AGSO.Core.Input;
using AGSO.Network;
using PluginUtils.Injection.Input;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Conn = AGSO.Network.Connection;
using KeyConfig = AGSO.Core.Input.KeyConfigInjector;

namespace AGSO.Core.Connection
{
    class NetworkClientInputHandler : ClientSequenceHandler, IInputHandler
    {
        private volatile int _Ready;
        private ClientRecorder _Recorder = new ClientRecorder(NetworkServerInputHandler.InitEmptyCount);

        public NetworkClientInputHandler()
        {
            KeyConfigInjector.Inject();
        }

        public bool HandleInput(IntPtr ptr)
        {
            if (_Ready == 0)
            {
                _Ready = 1;
            }
            _Recorder.Enqueue(ptr);
            SetInputData(ptr, this.Next());
            return true;
        }

        private void SetInputData(IntPtr ptr, byte[] data)
        {
            for (int i = 0; i < 27; ++i)
            {
                bool k = (data[i + 1] & 128) != 0;
                Marshal.WriteByte(ptr, KeyConfig.GetInjectedKeyIndex(i), (byte)(k ? 0x80 : 0));
            }
        }

        public void ReceiveNetworkData(byte[] data)
        {
            this.Receive(data);
        }

        public void SendNetworkData(Conn conn, Remote r)
        {
            if (_Ready == 1)
            {
                _Ready = 2;
                conn.Buffer.Reset(0);
                conn.Buffer.WriteByte((byte)PacketType.ClientReady);
                conn.Buffer.WriteSum();
                conn.Send(r);
            }
            byte[] cdata;
            while (_Recorder.TryDequeue(out cdata))
            {
                conn.Buffer.Reset(0);
                conn.Buffer.WriteByte((byte)PacketType.ClientInputData);
                conn.Buffer.WriteBytes(cdata);
                conn.Buffer.WriteSum();
                conn.Send(r);
            }
        }

        protected override void WaitFor(byte time)
        {
        }

        protected override void ResetAll(long time)
        {
        }
    }
    class NetworkClientInputHandler_ : IInputHandler
    {
        //0: not ready and signal not sent. 1: ready but signal not sent. 2: ready and signal sent
        private volatile int _Ready;
        //received raw input data from network thread to main thread, directly enqueue, not ordered.
        private ConcurrentQueue<byte[]> _InputData = new ConcurrentQueue<byte[]>();
        //analyzed input data, go before _InputData. ordered.
        private Queue<byte[]> _SortedInputData = new Queue<byte[]>();
        //temperary used in analyzing
        private List<byte[]> _UnsortedInputData = new List<byte[]>();
        //object pool for byte[27 + 1].
        private ConcurrentQueue<byte[]> _InputDataPool = new ConcurrentQueue<byte[]>();
        
        //input data from main thread to network thread.
        private ConcurrentQueue<byte[]> _ClientData = new ConcurrentQueue<byte[]>();
        //object pool for byte[9 + 1].
        private ConcurrentQueue<byte[]> _ClientDataPool = new ConcurrentQueue<byte[]>();

        //when != 0, use same value as in _CurrentInput and dec the count.
        private int _SkipCount;
        //record last input data and time when _SkipCount == 0 and next input data when _SkipCount != 0.
        private readonly byte[] _CurrentInput = new byte[27 + 1];
        //last client data. used to calculate time since last key change.
        private readonly byte[] _CurrentClient = new byte[9 + 1];

        public NetworkClientInputHandler_()
        {
            KeyConfigInjector.Inject();

            _CurrentInput[0] = 255;
            _CurrentClient[0] = 2;
            for (int i = 0; i < 9; ++i)
            {
                _CurrentClient[i + 1] = 127;
            }
        }

        public void ReceiveNetworkData(byte[] data)
        {
            byte[] c;
            if (!_InputDataPool.TryDequeue(out c))
            {
                c = new byte[27 + 1];
                for (int i = 0; i < 10; ++i)
                {
                    _InputDataPool.Enqueue(new byte[27 + 1]);
                }
            }
            Array.Copy(data, c, c.Length);
            _InputData.Enqueue(c);
        }

        public void SendNetworkData(Conn conn, Remote r)
        {
            if (_Ready == 1)
            {
                _Ready = 2;
                conn.Buffer.Reset(0);
                conn.Buffer.WriteByte((byte)PacketType.ClientReady);
                conn.Buffer.WriteSum();
                conn.Send(r);
            }
            byte[] cdata;
            while (_ClientData.TryDequeue(out cdata))
            {
                conn.Buffer.Reset(0);
                conn.Buffer.WriteByte((byte)PacketType.ClientInputData);
                conn.Buffer.WriteBytes(cdata);
                conn.Buffer.WriteSum();
                conn.Send(r);

                _ClientDataPool.Enqueue(cdata);
            }
        }

        public bool HandleInput(IntPtr ptr)
        {
            if (_Ready == 0)
            {
                _Ready = 1;
            }
            byte[] data;
            if (_SkipCount != 0)
            {
                _SkipCount -= 1;
                data = _CurrentInput;

                _ClientData.Enqueue(GetClientData(ptr));
                SetInputData(ptr, data);
            }
            else
            {
                data = ReadNetworkInputDataQueue();

                _ClientData.Enqueue(GetClientData(ptr));
                SetInputData(ptr, data);

                _InputDataPool.Enqueue(data);
            }
            return true;
        }

        private byte[] ReadNetworkInputDataQueue()
        {
            byte[] ret = WaitForNextInputData();
            byte time = Inc(_CurrentInput[0]);

            if (time != ret[0])
            {
                _UnsortedInputData.Clear();

                //not continous
                int diff = Diff(time, ret[0]);
                while (ret.Skip(1).Any(bb => (bb & 127) < diff))
                {
                    //need more
                    _UnsortedInputData.Add(ret);
                    ret = WaitForNextInputData();
                    diff = Diff(time, ret[0]);
                }
                if (_UnsortedInputData.Count > 0)
                {
                    foreach (var input in _UnsortedInputData.OrderBy(ii => Diff(time, ii[0])))
                    {
                        _SortedInputData.Enqueue(input);
                    }
                }

                _SkipCount = diff;
            }

            Array.Copy(ret, _CurrentInput, ret.Length);
            return ret;
        }

        private byte[] WaitForNextInputData()
        {
            if (_SortedInputData.Count != 0)
            {
                return _SortedInputData.Dequeue();
            }

            byte[] data;
            while (!_InputData.TryDequeue(out data))
            {
                Thread.Sleep(1);
            }
            return data;
        }

        private void SetInputData(IntPtr ptr, byte[] data)
        {
            for (int i = 0; i < 27; ++i)
            {
                bool k = (data[i + 1] & 128) != 0;
                Marshal.WriteByte(ptr, KeyConfig.GetInjectedKeyIndex(i), (byte)(k ? 0x80 : 0));
            }
        }

        private byte[] GetClientData(IntPtr ptr)
        {
            byte[] data;
            if (!_ClientDataPool.TryDequeue(out data))
            {
                data = new byte[10];
                for (int i = 0; i < 10; ++i)
                {
                    _ClientDataPool.Enqueue(new byte[10]);
                }
            }
            data[0] = Inc(_CurrentClient[0]);
            for (int i = 0; i < 9; ++i)
            {
                bool k = Marshal.ReadByte(ptr, KeyConfig.GetOriginalKeyIndex(i)) != 0;
                bool last = (_CurrentClient[i + 1] & 128) != 0;
                if (k == last)
                {
                    int c = _CurrentClient[i + 1] & 127;
                    if (c < 127)
                    {
                        c += 1;
                    }
                    data[i + 1] = (byte)((k ? 128 : 0) | c);
                }
                else
                {
                    data[i + 1] = (byte)((k ? 128 : 0) | 0);
                }
            }
            Array.Copy(data, _CurrentClient, data.Length);
            return data;
        }

        private byte Inc(byte b)
        {
            return (byte)((b + 1) & 255);
        }

        private int Diff(byte a, byte b)
        {
            if (a <= b)
            {
                return b - a;
            }
            return b + 256 - a;
        }
    }
}
