using AGSO.Network;
using PluginUtils.Injection.Input;
using System;
using System.Collections.Generic;
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

            public void OnStart()
            {
                Parent.Connection.Buffer.Write(PacketType.None);
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
                            if (!_Replied)
                            {
                                Parent.Connection.Buffer.Write(PacketType.NewConnection);
                                Parent.Connection.Send(Parent._Server);

                                _Replied = true;
                            }
                            return;
                        case PacketType.ServerStatus:
                            ConnectionSelectForm.Log("Server status");
                            return;
                        case PacketType.GameStart:
                            ConnectionSelectForm.Log("Game start");
                            Parent.ChangeStage(new GameStartHandler { Parent = Parent });
                            return;
                    }
                }
                ConnectionSelectForm.Log("Error");
                //error
            }

            public void OnTick()
            {
                if (!_Replied)
                {
                    if (++_TickCount >= 100)
                    {
                        _TickCount = 0;
                        Parent.Connection.Buffer.Write(PacketType.None);
                        Parent.Connection.Send(Parent._Server);
                    }
                }
                else
                {
                    if (++_TickCount >= 50)
                    {
                        _TickCount = 0;
                        Parent.Connection.Buffer.Write(PacketType.ClientStatus);
                        Parent.Connection.Send(Parent._Server);
                        if (!Parent.Connection.UpdateRemoteList(Parent._Server, 1000 * 5, 1000 * 20))
                        {
                            ConnectionSelectForm.Log("Close");
                            Parent.Stop();
                        }
                    }
                }
            }

            public void OnAbort()
            {
            }

            public int Interval
            {
                get { return 10; }
            }
        }

        private class GameStartHandler : IConnectionStage
        {
            public Client Parent;
            private byte[] _ByteBuffer = new byte[28];

            public void OnStart()
            {
                Parent._InputHandler = new ClientInputHandler();
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
                            return;
                        case PacketType.ServerInputData:
                            Parent.Connection.Buffer.ReadBytes(_ByteBuffer);
                            Parent._InputHandler.ReceiveNetworkData(_ByteBuffer);
                            return;
                    }
                }
            }

            public void OnTick()
            {
                Parent._InputHandler.SendNetworkData(Parent.Connection, Parent._Server);
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
