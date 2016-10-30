using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AGSO.Network
{
    class Connection
    {
        public static Connection CreateServer(int port)
        {
            return null;
        }

        public static Connection CreateClient(int port)
        {
            return null;
        }

        private IntPtr _Socket;
        private Thread _Thread;
        private volatile bool _StopThreadFlag;

        private Connection()
        {
        }

        private void StartThread()
        {
            while (!_StopThreadFlag)
            {
            }
        }

        public void Release()
        {
            if (_Thread.ThreadState == ThreadState.Running)
            {
                _StopThreadFlag = true;
                _Thread.Join();
            }
            //close
        }
    }
}
