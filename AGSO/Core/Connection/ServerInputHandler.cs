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
    class ServerInputHandler : IClientSequenceExceptionHandler, IInputHandler
    {
        public const int InitEmptyCount = 6;

        private Server.ClientInfo[] _Remote;
        private int _PlayerIndex;

        private ManualResetEvent _Ready = new ManualResetEvent(false);

        private readonly ServerMerger _Merger;

        private readonly ClientRecorder _LocalRecorder;
        private readonly ClientSequenceHandler _LocalSequence;

        public ServerInputHandler(Server.ClientInfo[] r, int playerIndex)
        {
            KeyConfigInjector.Inject();
            _Remote = r;
            _PlayerIndex = playerIndex;

            _Merger = new ServerMerger();
            _Merger.AddInitialEmpty(InitEmptyCount);
            for (int i = 0; i < 3; ++i)
            {
                if (i != playerIndex && r[i] == null)
                {
                    _Merger[i].EnableAutoFill = true;
                }
            }

            _LocalRecorder = new ClientRecorder(InitEmptyCount);
            _LocalSequence = new ClientSequenceHandler(this);
        }

        public void AllReady()
        {
            _Ready.Set();
        }

        public void ReceiveNetworkData(int id, byte[] data)
        {
            _Merger[id].Receive(data);
        }

        public void SendNetworkData(Conn conn)
        {
            byte[] d;
            while (_Merger.TryDequeue(out d))
            {
                conn.Buffer.Write(PacketType.ServerInputData, d);
                foreach (var c in _Remote)
                {
                    if (c != null)
                    {
                        conn.Send(c.Remote);
                    }
                }
                _LocalSequence.Receive(d);
            }
        }

        public bool HandleInput(IntPtr ptr)
        {
            _Ready.WaitOne();

            if (_PlayerIndex != -1)
            {
                _Merger[_PlayerIndex].Receive(_LocalRecorder.Convert(ptr));
            }
            _Merger.DoMerge();

            _LocalSequence.Next(ptr);

            return true;
        }

        public void WaitFor(byte time)
        {
        }

        public void ResetAll(long time)
        {
        }
    }
}
