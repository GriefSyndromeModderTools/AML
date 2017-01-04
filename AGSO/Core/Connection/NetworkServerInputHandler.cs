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
    class NetworkServerInputHandler : IInputHandler
    {
        public const int InitEmptyCount = 3;

        private class ServerPlayerSequenceHandler : SequenceHandler
        {
            public ServerPlayerSequenceHandler() :
                base(10)
            {
                for (int i = 0; i < InitEmptyCount; ++i)
                {
                    this.ReceiveEmpty(15);
                }
            }

            protected override bool CheckInterpolate(byte length, byte a, byte b)
            {
                return length < 10;
            }

            protected override byte Interpolate(byte length, byte t, byte a, byte b)
            {
                var aa = new ByteData(a);
                var bb = new ByteData(b);
                if (aa.Change == bb.Change)
                {
                    //a + t
                    if (aa.Frame + t >= 15)
                    {
                        aa.Frame = 15;
                    }
                    else
                    {
                        aa.Frame = (byte)(aa.Frame + t);
                    }
                    return aa.Data;
                }
                if (((aa.Change + 1) & 7) == bb.Change && bb.Frame != 15)
                {
                    if (t + bb.Frame >= length)
                    {
                        //b - (length - t)
                        bb.Frame -= (byte)(length - t);
                        return bb.Data;
                    }
                    else
                    {
                        //a + t
                        if (aa.Frame + t >= 15)
                        {
                            aa.Frame = 15;
                        }
                        else
                        {
                            aa.Frame = (byte)(aa.Frame + t);
                        }
                        return aa.Data;
                    }
                }
                //a + t
                if (aa.Frame + t >= 15)
                {
                    aa.Frame = 15;
                }
                else
                {
                    aa.Frame = (byte)(aa.Frame + t);
                }
                return aa.Data;
            }

            protected override void WaitFor(byte time)
            {
            }

            protected override void ResetAll(long time)
            {
            }
        }

        private Server.ClientInfo[] _Remote;
        private int _PlayerIndex; //Server
        private int _Ready;
        private SequenceHandler[] _ClientData = new SequenceHandler[3];
        private ServerMerger _Merger;
        private ClientRecorder _Client = new ClientRecorder(InitEmptyCount);
        private ClientSequenceHandler _ClientSequence = new ClientSequenceHandler();

        public NetworkServerInputHandler(Server.ClientInfo[] r, int playerIndex)
        {
            KeyConfigInjector.Inject();
            _Remote = r;
            _PlayerIndex = playerIndex;

            for (int i = 0; i < 3; ++i)
            {
                _ClientData[i] = new ServerPlayerSequenceHandler();
            }

            _Merger = new ServerMerger(_ClientData);

            for (int i = 0; i < InitEmptyCount; ++i)
            {
                _Merger.DoMerge();
            }
        }

        public void AllReady()
        {
            _Ready = 1;
        }

        public void ReceiveNetworkData(int id, byte[] data)
        {
            _ClientData[id].Receive(data);
        }

        public void SendNetworkData(Conn conn)
        {
            byte[] d;
            while (_Merger.TryDequeue(out d))
            {
                foreach (var c in _Remote)
                {
                    if (c != null)
                    {
                        conn.Buffer.Reset(0);
                        conn.Buffer.WriteByte((byte)PacketType.ServerInputData);
                        conn.Buffer.WriteBytes(d);
                        conn.Buffer.WriteSum();
                        conn.Send(c.Remote);
                    }
                }
                _ClientSequence.Receive(d);
            }
        }

        public bool HandleInput(IntPtr ptr)
        {
            if (_Ready != 2)
            {
                while (_Ready == 0)
                {
                    Thread.Sleep(1);
                }
                _Ready = 2;
            }
            _ClientData[_PlayerIndex].Receive(_Client.Convert(ptr));
            for (int i = 0; i < 3; ++i)
            {
                if (i != _PlayerIndex && _Remote[i] == null)
                {
                    _ClientData[i].ReceiveEmpty(15);
                }
            }
            _Merger.DoMerge();

            SetInputData(ptr, _ClientSequence.Next());

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
    }
    class NetworkServerInputHandler_ : IInputHandler
    {
        private class ClientInputQueue
        {
            private ConcurrentQueue<byte[]> _Queue = new ConcurrentQueue<byte[]>();
            private NetworkServerInputHandler_ _Parent;
            private byte _LastTime;

            private int _Skip;
            private byte[] _SkipData;

            public ClientInputQueue(NetworkServerInputHandler_ parent)
            {
                _Parent = parent;
                _LastTime = 255;
            }

            public void Receive(byte[] data)
            {
                byte[] d = _Parent.GetBytes10();
                Array.Copy(data, d, data.Length);
                _Queue.Enqueue(d);
            }

            public byte[] Dequeue(byte time)
            {
                if (_Skip != 0)
                {
                    _Skip -= 1;
                    
                    var d = _Parent.GetBytes10();
                    Array.Copy(_SkipData, d, d.Length);
                    d[0] = _LastTime;

                    return d;
                }

                _LastTime = Inc(_LastTime);
                byte[] ret;
                do
                {
                    while (!_Queue.TryDequeue(out ret))
                    {
                        Thread.Sleep(1);
                    }
                    _Skip = Diff(_LastTime, ret[0]);
                } while (ret[0] != _LastTime && _Skip > 10); //TODO
                if (_Skip != 0)
                {
                    var d = _Parent.GetBytes10();
                    Array.Copy(ret, d, d.Length);
                    d[0] = _LastTime;

                    _SkipData = ret;
                    return d;
                }
                return ret;
            }
        }

        private static byte Inc(byte b)
        {
            return (byte)((b + 1) & 255);
        }

        private static int Diff(byte a, byte b)
        {
            if (a <= b)
            {
                return b - a;
            }
            return b + 256 - a;
        }

        private Server.ClientInfo[] _Remote;
        private int _PlayerIndex; //Server
        private int _Ready;

        private ConcurrentQueue<byte[]> _BytePool10 = new ConcurrentQueue<byte[]>();
        private ConcurrentQueue<byte[]> _BytePool28 = new ConcurrentQueue<byte[]>();

        private readonly byte[] _CurrentClient = new byte[9 + 1];

        private ClientInputQueue[] _ClientInput;
        private byte _CurrentMergeTime;

        private ConcurrentQueue<byte[]> _SendQueue = new ConcurrentQueue<byte[]>();

        public NetworkServerInputHandler_(Server.ClientInfo[] r, int playerIndex)
        {
            KeyConfigInjector.Inject();

            _Remote = r;
            _PlayerIndex = playerIndex;

            _ClientInput = new[] 
            {
                new ClientInputQueue(this),
                new ClientInputQueue(this),
                new ClientInputQueue(this),
            };

            for (byte i = 0; i < 3; ++i)
            {
                _ClientInput[0].Receive(GetEmptyInput(i));
                _ClientInput[1].Receive(GetEmptyInput(i));
                _ClientInput[2].Receive(GetEmptyInput(i));
            }

            _CurrentMergeTime = 255;
            _CurrentClient[0] = 2;
            for (int i = 0; i < 9; ++i)
            {
                _CurrentClient[i + 1] = 127;
            }
        }

        private byte[] GetBytes10()
        {
            byte[] ret;
            if (!_BytePool10.TryDequeue(out ret))
            {
                ret = new byte[10];
                for (int i = 0; i < 10; ++i)
                {
                    _BytePool10.Enqueue(new byte[10]);
                }
            }
            return ret;
        }

        private byte[] GetBytes28()
        {
            byte[] ret;
            if (!_BytePool28.TryDequeue(out ret))
            {
                ret = new byte[28];
                for (int i = 0; i < 10; ++i)
                {
                    _BytePool28.Enqueue(new byte[28]);
                }
            }
            return ret;
        }

        public void ReceiveNetworkData(int id, byte[] data)
        {
            _ClientInput[id].Receive(data);
        }

        public void SendNetworkData(Conn conn)
        {
            byte[] d;
            while (_SendQueue.TryDequeue(out d))
            {
                foreach (var c in _Remote)
                {
                    if (c != null)
                    {
                        conn.Buffer.Reset(0);
                        conn.Buffer.WriteByte((byte)PacketType.ServerInputData);
                        conn.Buffer.WriteBytes(d);
                        conn.Buffer.WriteSum();
                        conn.Send(c.Remote);
                    }
                }
                _BytePool28.Enqueue(d);
            }
        }

        public bool HandleInput(IntPtr ptr)
        {
            if (_Ready != 2)
            {
                while (_Ready == 0)
                {
                    Thread.Sleep(1);
                }
                _Ready = 2;
            }

            var client = GetClientData(ptr);
            _ClientInput[0].Receive(client);
            if (_Remote[1] == null)
            {
                _ClientInput[1].Receive(GetEmptyInput(client[0]));
            }
            if (_Remote[2] == null)
            {
                _ClientInput[2].Receive(GetEmptyInput(client[0]));
            }

            byte[] d = GetBytes28();

            byte[] d1 = _ClientInput[0].Dequeue(_CurrentMergeTime);
            byte[] d2 = _ClientInput[1].Dequeue(_CurrentMergeTime);
            byte[] d3 = _ClientInput[2].Dequeue(_CurrentMergeTime);
            Array.Copy(d1, d, 10);
            Array.Copy(d2, 1, d, 10, 9);
            Array.Copy(d3, 1, d, 19, 9);
            _CurrentMergeTime = Inc(_CurrentMergeTime);

            _BytePool10.Enqueue(d1);
            _BytePool10.Enqueue(d2);
            _BytePool10.Enqueue(d3);

            SetInputData(ptr, d);
            _SendQueue.Enqueue(d);

            return true;
        }

        public void AllReady()
        {
            _Ready = 1;
        }

        private byte[] GetEmptyInput(byte time, byte k = 127)
        {
            var ret = GetBytes10();
            ret[0] = time;
            for (int i = 1; i < 10; ++i)
            {
                ret[i] = k;
            }
            return ret;
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
            byte[] data = GetBytes10();
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
    }
}
