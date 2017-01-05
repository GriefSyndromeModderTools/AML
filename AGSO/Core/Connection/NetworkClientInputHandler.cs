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

namespace AGSO.Core.Connection
{
    class NetworkClientInputHandler : IClientSequenceExceptionHandler, IInputHandler
    {
        private volatile int _Ready;
        private readonly ClientRecorder _Recorder;
        private readonly ClientSequenceHandler _Sequence;

        public NetworkClientInputHandler()
        {
            KeyConfigInjector.Inject();
            _Recorder = new ClientRecorder(NetworkServerInputHandler.InitEmptyCount);
            _Sequence = new ClientSequenceHandler(this);
        }

        public bool HandleInput(IntPtr ptr)
        {
            if (_Ready == 0)
            {
                _Ready = 1;
            }
            _Recorder.Enqueue(ptr);
            _Sequence.Next(ptr);
            return true;
        }

        public void ReceiveNetworkData(byte[] data)
        {
            _Sequence.Receive(data);
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

        public void WaitFor(byte time)
        {
        }

        public void ResetAll(long time)
        {
        }
    }
}
