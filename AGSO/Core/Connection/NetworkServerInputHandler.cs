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
    class NetworkServerInputHandler : IClientSequenceExceptionHandler, IInputHandler
    {
        public const int InitEmptyCount = 3;

        private Server.ClientInfo[] _Remote;
        private int? _PlayerIndex;

        private int _Ready;

        private readonly SequenceHandler[] _ClientData;
        private readonly ServerMerger _Merger;

        private readonly ClientRecorder _LocalRecorder;
        private readonly ClientSequenceHandler _LocalSequence;

        public NetworkServerInputHandler(Server.ClientInfo[] r, int? playerIndex)
        {
            KeyConfigInjector.Inject();
            _Remote = r;
            _PlayerIndex = playerIndex;

            _ClientData = new SequenceHandler[3];
            for (int i = 0; i < 3; ++i)
            {
                _ClientData[i] = new ServerPlayerSequenceHandler();
            }

            _Merger = new ServerMerger(_ClientData);

            for (int i = 0; i < InitEmptyCount; ++i)
            {
                _Merger.DoMerge();
            }

            _LocalRecorder = new ClientRecorder(InitEmptyCount);
            _LocalSequence = new ClientSequenceHandler(this);
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
                _LocalSequence.Receive(d);
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

            if (_PlayerIndex.HasValue)
            {
                _ClientData[_PlayerIndex.Value].Receive(_LocalRecorder.Convert(ptr));
            }

            for (int i = 0; i < 3; ++i)
            {
                if (i != _PlayerIndex && _Remote[i] == null)
                {
                    _ClientData[i].ReceiveEmpty(15);
                }
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
