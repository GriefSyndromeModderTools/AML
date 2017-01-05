using AGSO.Network;
using PluginUtils.Injection.Input;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Conn = AGSO.Network.Connection;

namespace AGSO.Core.Connection
{
    class Server
    {
        public class ClientInfo
        {
            public Remote Remote;
            public int PlayerIndex;
        }

        private interface IServerHandler
        {
            void OnStart();
            void OnPacket(Remote r);
            void OnTick();
            void OnAbort();
        }

        private Conn _Connection;
        private Thread _NetworkThread;
        private object _Mutex = new object();
        private volatile bool _ThreadRunning;
        private volatile int _Interval = 1;
        private volatile IServerHandler _Handler;

        private List<ClientInfo> _Clients = new List<ClientInfo>();

        private ServerInputHandler _InputHandler;

        public Server(int port)
        {
            _Connection = new Conn(1024);
            _Connection.Bind(port);

            _Handler = new WaitForStartHandler { _Parent = this };

            _ThreadRunning = true;

            _NetworkThread = new Thread(ThreadStart);
            _NetworkThread.Start();
        }

        public void Stop()
        {
            lock (_Mutex)
            {
                if (_NetworkThread == null)
                {
                    return;
                }
                _ThreadRunning = false;
                _NetworkThread = null;
            }
        }

        public void StartGame()
        {
            _Handler = new GameStartHandler { Parent = this };
            ConnectionSelectForm.Log("Game start");
            ConnectionSelectForm.CloseWindow();
        }

        private void ThreadStart()
        {
            IServerHandler h = null;
            while (_ThreadRunning)
            {
                if (h != _Handler)
                {
                    h = _Handler;
                    h.OnStart();
                }

                Remote r;
                while (_Connection.Receive(out r))
                {
                    if (_Connection.Buffer.CheckSum())
                    {
                        h.OnPacket(r);
                    }
                }
                h.OnTick();
                Thread.Sleep(_Interval);
            }
            h.OnAbort();
        }

        private void RemoveClient(Remote r)
        {
            if (r.Data == null)
            {
                return;
            }
            _Clients.Remove((ClientInfo)r.Data);
            ConnectionSelectForm.Log("[D] Client " + _Clients.Count);
            r.ReceiveCount = 0;
        }

        private class WaitForStartHandler : IServerHandler
        {
            public Server _Parent;
            private int _CountForUpdateRemote;

            public void OnStart()
            {
                _Parent._Interval = 50;
            }

            public void OnPacket(Remote r)
            {
                var type = (PacketType)_Parent._Connection.Buffer.ReadByte();
                switch (type)
                {
                    case PacketType.None:
                        //empty packet
                        r.ReceiveCount -= 1;
                        {
                            var buffer = _Parent._Connection.Buffer;
                            buffer.Reset(0);
                            buffer.WriteByte((byte)PacketType.None);
                            buffer.WriteSum();
                            _Parent._Connection.Send(r);
                        }
                        return;
                    case PacketType.NewConnection:
                        if (r.ReceiveCount == 1)
                        {
                            ClientInfo ci = new ClientInfo();
                            ci.Remote = r;
                            r.Data = ci;
                            _Parent._Clients.Add(ci);

                            //just reply
                            SendStatus(r);

                            ConnectionSelectForm.Log("New client " + r.ToString());
                            ConnectionSelectForm.Log("[A] Client " + _Parent._Clients.Count);
                            return;
                        }
                        break;
                    case PacketType.ClientStatus:
                        if (r.ReceiveCount != 1)
                        {
                            //TODO check if active client
                            //we don't need any information, just reply
                            SendStatus(r);

                            ConnectionSelectForm.Log("Client status " + r.ToString());
                            return;
                        }
                        break;
                    default:
                        break;
                }
                ConnectionSelectForm.Log("Error");
                _Parent.RemoveClient(r);
            }

            public void OnTick()
            {
                //check every 1s
                if (++_CountForUpdateRemote > 20)
                {
                    _CountForUpdateRemote = 0;

                    //we don't ask for reply, so check reply for long time (100s)
                    //client should send tick frequently, check if we received in the last 1s

                    for (int i = 0; i < _Parent._Clients.Count; ++i)
                    {
                        if (!_Parent._Connection.UpdateRemoteList(_Parent._Clients[i].Remote, 1000 * 100, 1000))
                        {
                            _Parent._Clients.RemoveAt(i);
                            i -= 1;
                            ConnectionSelectForm.Log("[C] Client " + _Parent._Clients.Count);
                        }
                    }
                }
                //
            }

            private void SendStatus(Remote r)
            {
                var buffer = _Parent._Connection.Buffer;
                buffer.Reset(0);
                buffer.WriteByte((byte)PacketType.ServerStatus);
                buffer.WriteSum();
                _Parent._Connection.Send(r);
            }

            public void OnAbort()
            {
            }
        }

        private class GameStartHandler : IServerHandler
        {
            public Server Parent;
            private bool[] _Ready;

            public void OnStart()
            {
                Parent._Interval = 1;
                ConnectionSelectForm.Log("[E] Client " + Parent._Clients.Count);
                var client = new ClientInfo[3];

                for (int i = 0; i < Parent._Clients.Count; ++i)
                {
                    Parent._Clients[i].PlayerIndex = i + 1;
                    client[i + 1] = Parent._Clients[i];

                    Parent._Connection.Buffer.Reset(0);
                    Parent._Connection.Buffer.WriteByte((byte)PacketType.GameStart);
                    Parent._Connection.Buffer.WriteSum();
                    Parent._Connection.Send(Parent._Clients[i].Remote);

                    ConnectionSelectForm.Log("Inform " + Parent._Clients[i].Remote.ToString() + "...");
                }

                _Ready = client.Select(c => c == null).ToArray();

                Parent._InputHandler = new ServerInputHandler(client, 0);
                InputManager.RegisterHandler(Parent._InputHandler);
            }

            private byte[] _ByteBuffer = new byte[10];

            public void OnPacket(Remote r)
            {
                var type = (PacketType)Parent._Connection.Buffer.ReadByte();
                switch (type)
                {
                    case PacketType.NewConnection:
                        //TODO
                        return;
                    case PacketType.ClientReady:
                        ConnectionSelectForm.Log("Ready " + r.ToString());
                        _Ready[RemoteIndex(r)] = true;
                        if (_Ready.All(rr => rr))
                        {
                            Parent._InputHandler.AllReady();
                        }
                        return;
                    case PacketType.ClientStatus:
                        //ignore
                        return;
                    case PacketType.ClientInputData:
                        //
                        Parent._Connection.Buffer.ReadBytes(_ByteBuffer);
                        Parent._InputHandler.ReceiveNetworkData(RemoteIndex(r), _ByteBuffer);
                        return;
                    default:
                        break;
                }
            }

            private int RemoteIndex(Remote r)
            {
                return ((ClientInfo)r.Data).PlayerIndex;
            }

            public void OnTick()
            {
                Parent._InputHandler.SendNetworkData(Parent._Connection);
            }

            public void OnAbort()
            {
            }
        }

    }
}
