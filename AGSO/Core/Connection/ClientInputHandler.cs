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
    class ClientInputHandler : IClientSequenceExceptionHandler, IInputHandler
    {
        private volatile int _Ready;
        private readonly ClientRecorder _Recorder;
        private readonly ClientSequenceHandler _Sequence;
        private byte _SessionID;

        public ClientInputHandler(byte sid)
        {
            KeyConfigInjector.Inject();
            _SessionID = sid;
            _Recorder = new ClientRecorder(ServerInputHandler.InitEmptyCount);
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
                conn.Buffer.Write(PacketType.ClientReady, _SessionID);
                conn.Send(r);
            }
            byte[] cdata;
            while (_Recorder.TryDequeue(out cdata))
            {
                conn.Buffer.Write(PacketType.ClientInputData, _SessionID, cdata);
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
