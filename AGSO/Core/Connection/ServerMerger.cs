using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AGSO.Core.Connection
{
    class ServerMerger
    {
        private ServerPlayerSequenceHandler[] _Input;
        private ConcurrentQueue<byte[]> _Queue = new ConcurrentQueue<byte[]>();
        private ConcurrentQueue<byte[]> _Pool = new ConcurrentQueue<byte[]>();
        private byte[] _Last;

        public ServerMerger()
        {
            _Input = new ServerPlayerSequenceHandler[3];
            for (int i = 0; i < 3; ++i)
            {
                _Input[i] = new ServerPlayerSequenceHandler();
            }

            for (int i = 0; i < 10; ++i)
            {
                _Pool.Enqueue(new byte[28]);
            }
            _Last = new byte[28];
        }

        public ServerPlayerSequenceHandler this[int index]
        {
            get
            {
                return _Input[index];
            }
        }

        public void AddInitialEmpty(int count)
        {
            for (int i = 0; i < count; ++i)
            {
                this[0].ReceiveEmpty(15);
                this[1].ReceiveEmpty(15);
                this[2].ReceiveEmpty(15);
                this.DoMerge();
            }
        }

        private byte[] GetFromPool()
        {
            byte[] ret;
            if (!_Pool.TryDequeue(out ret))
            {
                ret = new byte[28];
                for (int i = 0; i < 10; ++i)
                {
                    _Pool.Enqueue(new byte[28]);
                }
            }
            return ret;
        }

        public void DoMerge()
        {
            var ret = GetFromPool();
            var p1 = _Input[0].Next();
            var p2 = _Input[1].Next();
            var p3 = _Input[2].Next();
            Array.Copy(p1, 0, ret, 0, 10);
            Array.Copy(p2, 1, ret, 10, 9);
            Array.Copy(p2, 1, ret, 19, 9);
            _Queue.Enqueue(ret);
        }

        public bool TryDequeue(out byte[] ret)
        {
            if (_Queue.TryDequeue(out ret))
            {
                _Pool.Enqueue(_Last);
                _Last = ret;
                return true;
            }
            return false;
        }
    }
}
