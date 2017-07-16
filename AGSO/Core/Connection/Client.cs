using AGSO.Network;
using PluginUtils.Injection.Input;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Conn = AGSO.Network.Connection;

namespace AGSO.Core.Connection
{
    class Client : ConnectionHandler
    {
        private Remote _Server;

        private ClientInputHandler _InputHandler;

        public Client(string ip, int port)
        {
            Connect();
            Start(new InitialClientHandler
            {
                Parent = this,
                Ip = ip,
                Port = port,
            });
        }

        private class InitialClientHandler : IConnectionStage
        {
            public Client Parent;
            public string Ip;
            public int Port;
            private int _TickCount = 0;
            private bool _Replied;
            private Stopwatch _Clock = new Stopwatch();

            public void OnStart()
            {
                Parent.Connection.Buffer.Write(PacketType.None, 0);
                Parent._Server = Parent.Connection.Send(Ip, Port);

                Thread.Sleep(500);
            }

            public void OnPacket(Remote r)
            {
                if (r == Parent._Server)
                {
                    var type = (PacketType)Parent.Connection.Buffer.ReadByte();
                    switch (type)
                    {
                        case PacketType.None:
                            NetworkLogHelper.Write("Empty packet.");
                            if (!_Replied)
                            {
                                NetworkLogHelper.Write("Empty packet from server. Send connection.");
                                Parent.Connection.Buffer.Write(PacketType.NewConnection, 0);
                                Parent.Connection.Send(Parent._Server);

                                _Replied = true;
                            }
                            return;
                        case PacketType.ServerStatus:
                            NetworkLogHelper.Write("Server status.");
                            return;
                        case PacketType.ConnectionAccept:
                            NetworkLogHelper.Write("Accepted.");
                            ConnectionSelectForm.Log("Accepted");
                            return;
                        case PacketType.GameStart:
                            NetworkLogHelper.Write("Game start.");
                            ConnectionSelectForm.Log("Game start");
                            Parent.ChangeStage(new GameStartHandler
                            {
                                Parent = Parent,
                                SessionID = Parent.Connection.Buffer.ReadByte()
                            });
                            return;
                        case PacketType.PingRequest:
                            Parent.Connection.Buffer.Write(PacketType.PingReply, 0);
                            Parent.Connection.Send(r);
                            return;
                        case PacketType.PingReply:
                            ConnectionSelectForm.Ping((int)_Clock.ElapsedMilliseconds);
                            return;
                    }
                }
                NetworkLogHelper.Write("Error", Parent.Connection.Buffer);
                ConnectionSelectForm.Log("Error");
                //error
            }

            public void OnTick()
            {
                if (!_Replied)
                {
                    if (++_TickCount >= 1000)
                    {
                        NetworkLogHelper.Write("Send empty packet.");
                        _TickCount = 0;
                        Parent.Connection.Buffer.Write(PacketType.None, 0);
                        Parent.Connection.Send(Parent._Server);
                    }
                }
                else
                {
                    if (++_TickCount >= 500)
                    {
                        NetworkLogHelper.Write("Send client status.");
                        _TickCount = 0;
                        Parent.Connection.Buffer.Write(PacketType.ClientStatus, 0);
                        Parent.Connection.Send(Parent._Server);
                        if (!Parent.Connection.UpdateRemoteList(Parent._Server, 1000 * 5, 1000 * 20))
                        {
                            ConnectionSelectForm.Log("Close");
                            Parent.Stop();
                        }

                        _Clock.Restart();
                        Parent.Connection.Buffer.Write(PacketType.PingRequest, 0);
                        Parent.Connection.Send(Parent._Server);
                    }
                }
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
            public Client Parent;
            public byte SessionID;
            private int _CountForPing;

            private byte[] _ByteBuffer = new byte[28];

            public void OnStart()
            {
                Parent._InputHandler = new ClientInputHandler(SessionID);
                InputManager.RegisterHandler(Parent._InputHandler);

                ConnectionSelectForm.CloseWindow();
            }

            public void OnPacket(Remote r)
            {
                if (r == Parent._Server)
                {
                    var type = (PacketType)Parent.Connection.Buffer.ReadByte();
                    switch (type)
                    {
                        case PacketType.ServerStatus:
                            NetworkLogHelper.Write("Server status (ignore).");
                            return;
                        case PacketType.ServerInputData:
                            NetworkLogHelper.Write("Input data", Parent.Connection.Buffer);
                            if (!CheckSessionID())
                            {
                                break;
                            }
                            Parent.Connection.Buffer.ReadBytes(_ByteBuffer);
                            Parent._InputHandler.ReceiveNetworkData(_ByteBuffer);
                            return;
                        case PacketType.PingRequest:
                            Parent.Connection.Buffer.Write(PacketType.PingReply, 0);
                            Parent.Connection.Send(r);
                            return;
                        case PacketType.PingReply:
                            return;
                    }
                }
            }

            private bool CheckSessionID()
            {
                return Parent.Connection.Buffer.ReadByte() == SessionID;
            }

            public void OnTick()
            {
                Parent._InputHandler.SendNetworkData(Parent.Connection, Parent._Server);
                if (++_CountForPing == 120)
                {
                    _CountForPing = 0;
                    Parent.Connection.Buffer.Write(PacketType.PingRequest, 0);
                    Parent.Connection.Send(Parent._Server);
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
