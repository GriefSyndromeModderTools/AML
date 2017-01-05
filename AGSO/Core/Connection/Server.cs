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
    class Server : ConnectionHandler
    {
        public class ClientInfo
        {
            public Remote Remote;
            public int PlayerIndex;
        }

        private List<ClientInfo> _Clients = new List<ClientInfo>();

        private ServerInputHandler _InputHandler;

        public Server(int port)
        {
            Connect(port);
            Start(new WaitForStartHandler { _Parent = this });
        }

        public void StartGame()
        {
            ChangeStage(new GameStartHandler { Parent = this });
            ConnectionSelectForm.Log("Game start");
            ConnectionSelectForm.CloseWindow();
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

        private class WaitForStartHandler : IConnectionStage
        {
            public Server _Parent;
            private int _CountForUpdateRemote;

            public void OnStart()
            {
            }

            public void OnPacket(Remote r)
            {
                var type = (PacketType)_Parent.Connection.Buffer.ReadByte();
                switch (type)
                {
                    case PacketType.None:
                        //empty packet
                        r.ReceiveCount -= 1;
                        _Parent.Connection.Buffer.Write(PacketType.None);
                        _Parent.Connection.Send(r);
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
                        if (!_Parent.Connection.UpdateRemoteList(_Parent._Clients[i].Remote, 1000 * 100, 1000))
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
                _Parent.Connection.Buffer.Write(PacketType.ServerStatus);
                _Parent.Connection.Send(r);
            }

            public void OnAbort()
            {
            }

            public int Interval
            {
                get { return 50; }
            }
        }

        private class GameStartHandler : IConnectionStage
        {
            public Server Parent;
            private bool[] _Ready;

            public void OnStart()
            {
                ConnectionSelectForm.Log("[E] Client " + Parent._Clients.Count);
                var client = new ClientInfo[3];

                Parent.Connection.Buffer.Write(PacketType.GameStart);
                for (int i = 0; i < Parent._Clients.Count; ++i)
                {
                    Parent._Clients[i].PlayerIndex = i + 1;
                    client[i + 1] = Parent._Clients[i];

                    ConnectionSelectForm.Log("Inform " + Parent._Clients[i].Remote.ToString() + "...");
                    Parent.Connection.Send(Parent._Clients[i].Remote);
                }

                _Ready = client.Select(c => c == null).ToArray();

                Parent._InputHandler = new ServerInputHandler(client, 0);
                InputManager.RegisterHandler(Parent._InputHandler);

                CheckReady();
            }

            private byte[] _ByteBuffer = new byte[10];

            public void OnPacket(Remote r)
            {
                var type = (PacketType)Parent.Connection.Buffer.ReadByte();
                switch (type)
                {
                    case PacketType.NewConnection:
                        //TODO
                        return;
                    case PacketType.ClientReady:
                        ConnectionSelectForm.Log("Ready " + r.ToString());
                        _Ready[RemoteIndex(r)] = true;
                        CheckReady();
                        return;
                    case PacketType.ClientStatus:
                        //ignore
                        return;
                    case PacketType.ClientInputData:
                        //
                        Parent.Connection.Buffer.ReadBytes(_ByteBuffer);
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

            private void CheckReady()
            {
                if (_Ready.All(rr => rr))
                {
                    Parent._InputHandler.AllReady();
                }
            }

            public void OnTick()
            {
                Parent._InputHandler.SendNetworkData(Parent.Connection);
            }

            public void OnAbort()
            {
            }

            public int Interval
            {
                get { return 50; }
            }
        }
    }
}
