using AGSO.Network;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Conn = AGSO.Network.Connection;

namespace AGSO.Core.Connection
{
    class ConnectionHandler
    {
        private Conn _Connection;
        private Thread _NetworkThread;
        private object _Mutex = new object();
        private volatile bool _ThreadRunning;
        private volatile int _Interval = 1;
        private volatile IConnectionStage _Stage;

        public void Connect()
        {
            _Connection = new Conn(1024);
        }

        public Conn Connection
        {
            get { return _Connection; }
        }

        public void Connect(int port)
        {
            _Connection = new Conn(1024);
            _Connection.Bind(port);
        }

        public void Start(IConnectionStage stage)
        {
            _Stage = stage;

            _ThreadRunning = true;

            _NetworkThread = new Thread(ThreadEntry);
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

        public void ChangeStage(IConnectionStage stage)
        {
            _Stage = stage;
        }

        private void ThreadEntry()
        {
            IConnectionStage h = null;
            while (_ThreadRunning)
            {
                if (h != _Stage)
                {
                    h = _Stage;
                    h.OnStart();
                    _Interval = h.Interval;
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
    }
}
