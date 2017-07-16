using AGSO.Network;
using PluginUtils.Injection.Input;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
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
            public long PingTime;
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
            NetworkLogHelper.Write("User game start");
            ConnectionSelectForm.CloseWindow();
        }

        private void RemoveClient(Remote r)
        {
            NetworkLogHelper.Write("Remove client " + r.ToString());
            r.ReceiveCount = 100;
            Connection.Block(r);
            if (r.Data == null)
            {
                return;
            }
            _Clients.Remove((ClientInfo)r.Data);
            ConnectionSelectForm.Log("Remove client " + r.Address.ToString());
        }

        private class WaitForStartHandler : IConnectionStage
        {
            public Server _Parent;
            private int _CountForUpdateRemote;
            private Stopwatch _Clock = new Stopwatch();

            public void OnStart()
            {
                ConnectionSelectForm.Log("Server start");
            }

            public void OnPacket(Remote r)
            {
                var type = (PacketType)_Parent.Connection.Buffer.ReadByte();
                switch (type)
                {
                    case PacketType.None:
                        NetworkLogHelper.Write("Empty packet");
                        //empty packet
                        r.ReceiveCount -= 1;
                        _Parent.Connection.Buffer.Write(PacketType.None, 0);
                        _Parent.Connection.Send(r);
                        return;
                    case PacketType.NewConnection:
                        NetworkLogHelper.Write("New connection");
                        if (r.ReceiveCount == 1)
                        {
                            NetworkLogHelper.Write("New connection accepted");
                            ClientInfo ci = new ClientInfo();
                            ci.Remote = r;
                            r.Data = ci;
                            _Parent._Clients.Add(ci);

                            _Parent.Connection.Buffer.Write(PacketType.ConnectionAccept, 0);
                            _Parent.Connection.Send(r);

                            ConnectionSelectForm.Log("New client " + r.ToString());
                            return;
                        }
                        break;
                    case PacketType.ClientStatus:
                        NetworkLogHelper.Write("Client status");
                        if (r.ReceiveCount != 1)
                        {
                            NetworkLogHelper.Write("Client status replied");
                            //TODO check if active client
                            //we don't need any information, just reply
                            SendStatus(r);
                            return;
                        }
                        break;
                    case PacketType.PingRequest:
                        _Parent.Connection.Buffer.Write(PacketType.PingReply, 0);
                        _Parent.Connection.Send(r);
                        return;
                    case PacketType.PingReply:
                        {
                            var client = (ClientInfo)r.Data;
                            if (client != null)
                            {
                                client.PingTime = _Clock.ElapsedMilliseconds;
                            }
                        }
                        return;
                    default:
                        break;
                }
                ConnectionSelectForm.Log("Error");
                NetworkLogHelper.Write("Error", _Parent.Connection.Buffer);
                _Parent.RemoveClient(r);
            }

            public void OnTick()
            {
                //check every 1s
                if (++_CountForUpdateRemote > 200)
                {
                    _CountForUpdateRemote = 0;

                    //we don't ask for reply, so check reply for long time (100s)
                    //client should send tick frequently, check if we received in the last 1s

                    for (int i = 0; i < _Parent._Clients.Count; ++i)
                    {
                        var r = _Parent._Clients[i].Remote;
                        if (!_Parent.Connection.UpdateRemoteList(r, 1000 * 100, 1000))
                        {
                            _Parent._Clients.RemoveAt(i);
                            i -= 1;
                            ConnectionSelectForm.Log("Remove client " + r.Address.ToString());
                        }
                    }

                    if (_Parent._Clients.All(c => c.PingTime != -1))
                    {
                        ConnectionSelectForm.Ping(_Parent._Clients.Select(c => (int)c.PingTime).ToArray());
                        _Clock.Restart();
                        for (int i = 0; i < _Parent._Clients.Count; ++i)
                        {
                            _Parent.Connection.Buffer.Write(PacketType.PingRequest, 0);
                            _Parent.Connection.Send(_Parent._Clients[i].Remote);
                        }
                    }
                }
                //
            }

            private void SendStatus(Remote r)
            {
                _Parent.Connection.Buffer.Write(PacketType.ServerStatus, 0);
                _Parent.Connection.Send(r);
            }

            public void OnAbort()
            {
            }

            public int Interval
            {
                get { return 5; }
            }
        }

        private class GameStartHandler : IConnectionStage
        {
            public Server Parent;
            private bool[] _Ready;
            private static byte _NextSessionID = 1;
            private byte _CurrentSession;
            private int _CountForPing;

            public void OnStart()
            {
                NetworkLogHelper.Write("Start stage");
                var client = new ClientInfo[3];

                var sid = _NextSessionID++;
                _CurrentSession = sid;

                Parent.Connection.Buffer.Write(PacketType.GameStart, sid);
                for (int i = 0; i < Parent._Clients.Count; ++i)
                {
                    Parent._Clients[i].PlayerIndex = i + 1;
                    client[i + 1] = Parent._Clients[i];

                    ConnectionSelectForm.Log("Inform " + Parent._Clients[i].Remote.ToString() + "...");
                    Parent.Connection.Send(Parent._Clients[i].Remote);
                }

                _Ready = client.Select(c => c == null).ToArray();

                Parent._InputHandler = new ServerInputHandler(client, 0, sid);
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
                        NetworkLogHelper.Write("New connection in game");
                        //TODO
                        return;
                    case PacketType.ClientReady:
                        ConnectionSelectForm.Log("Ready " + r.ToString());
                        NetworkLogHelper.Write("Client ready");
                        _Ready[RemoteIndex(r)] = true;
                        CheckReady();
                        return;
                    case PacketType.ClientStatus:
                        NetworkLogHelper.Write("Client status in game");
                        //ignore
                        return;
                    case PacketType.ClientInputData:
                        //
                        NetworkLogHelper.Write("Client input", Parent.Connection.Buffer);
                        if (!CheckSessionID())
                        {
                            break;
                        }
                        Parent.Connection.Buffer.ReadBytes(_ByteBuffer);
                        Parent._InputHandler.ReceiveNetworkData(RemoteIndex(r), _ByteBuffer);
                        return;
                    case PacketType.PingRequest:
                        Parent.Connection.Buffer.Write(PacketType.PingReply, 0);
                        Parent.Connection.Send(r);
                        return;
                    case PacketType.PingReply:
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

            private bool CheckSessionID()
            {
                return Parent.Connection.Buffer.ReadByte() == _CurrentSession;
            }

            public void OnTick()
            {
                Parent._InputHandler.SendNetworkData(Parent.Connection);
                if (++_CountForPing == 120)
                {
                    _CountForPing = 0;
                    for (int i = 0; i < Parent._Clients.Count; ++i)
                    {
                        Parent.Connection.Buffer.Write(PacketType.PingRequest, 0);
                        Parent.Connection.Send(Parent._Clients[i].Remote);
                    }
                }
            }

            public void OnAbort()
            {
            }

            public int Interval
            {
                get { return 1; }
            }
        }
    }
}
