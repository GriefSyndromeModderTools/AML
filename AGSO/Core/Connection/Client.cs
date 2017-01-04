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
    class Client
    {
        private interface IClientHandler
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
        private volatile IClientHandler _Handler;

        private Remote _Server;

        private NetworkClientInputHandler _InputHandler;

        public Client(string ip, int port)
        {
            _Connection = new Conn(1024);

            _Handler = new InitialClientHandler
            {
                Parent = this,
                Ip = ip,
                Port = port,
            };

            _NetworkThread = new Thread(ThreadStart);
            _NetworkThread.Start();

            _ThreadRunning = true;
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

        private void ThreadStart()
        {
            IClientHandler h = null;
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

        private class InitialClientHandler : IClientHandler
        {
            public Client Parent;
            public string Ip;
            public int Port;
            private int _TickCount = 0;
            private bool _Replied;

            public void OnStart()
            {
                Parent._Interval = 10;

                Parent._Connection.Buffer.Reset(0);
                Parent._Connection.Buffer.WriteByte((byte)PacketType.None);
                Parent._Connection.Buffer.WriteSum();
                Parent._Server = Parent._Connection.Send(Ip, Port);

                Thread.Sleep(2000);
            }

            public void OnPacket(Remote r)
            {
                if (r == Parent._Server)
                {
                    var type = (PacketType)Parent._Connection.Buffer.ReadByte();
                    switch (type)
                    {
                        case PacketType.None:
                            if (!_Replied)
                            {
                                Parent._Connection.Buffer.Reset(0);
                                Parent._Connection.Buffer.WriteByte((byte)PacketType.NewConnection);
                                Parent._Connection.Buffer.WriteSum();
                                Parent._Server = Parent._Connection.Send(Ip, Port);

                                _Replied = true;
                            }
                            return;
                        case PacketType.ServerStatus:
                            ConnectionSelectForm.Log("Server status");
                            return;
                        case PacketType.GameStart:
                            ConnectionSelectForm.Log("Game start");
                            Parent._Handler = new GameStartHandler { Parent = Parent };
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
                    if (++_TickCount >= 200)
                    {
                        Parent._Connection.Buffer.Reset(0);
                        Parent._Connection.Buffer.WriteByte((byte)PacketType.None);
                        Parent._Connection.Buffer.WriteSum();
                        Parent._Connection.Send(Parent._Server);
                    }
                }
                else
                {
                    if (++_TickCount >= 50)
                    {
                        _TickCount = 0;
                        Parent._Connection.Buffer.Reset(0);
                        Parent._Connection.Buffer.WriteByte((byte)PacketType.ClientStatus);
                        Parent._Connection.Buffer.WriteSum();
                        Parent._Connection.Send(Parent._Server);
                        if (!Parent._Connection.UpdateRemoteList(Parent._Server, 1000 * 5, 1000 * 20))
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
        }

        private class GameStartHandler : IClientHandler
        {
            public Client Parent;
            private byte[] _ByteBuffer = new byte[28];

            public void OnStart()
            {
                Parent._InputHandler = new NetworkClientInputHandler();
                InputManager.RegisterHandler(Parent._InputHandler);

                ConnectionSelectForm.CloseWindow();
            }

            public void OnPacket(Remote r)
            {
                if (r == Parent._Server)
                {
                    var type = (PacketType)Parent._Connection.Buffer.ReadByte();
                    switch (type)
                    {
                        case PacketType.ServerStatus:
                            return;
                        case PacketType.ServerInputData:
                            Parent._Connection.Buffer.ReadBytes(_ByteBuffer);
                            Parent._InputHandler.ReceiveNetworkData(_ByteBuffer);
                            return;
                    }
                }
            }

            public void OnTick()
            {
                Parent._InputHandler.SendNetworkData(Parent._Connection, Parent._Server);
            }

            public void OnAbort()
            {
            }
        }

    }
}
