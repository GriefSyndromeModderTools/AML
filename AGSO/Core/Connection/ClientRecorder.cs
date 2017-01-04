﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using KeyConfig = AGSO.Core.Input.KeyConfigInjector;

namespace AGSO.Core.Connection
{
    class ClientRecorder
    {
        private ConcurrentQueue<byte[]> _Pool = new ConcurrentQueue<byte[]>();
        private ConcurrentQueue<byte[]> _Queue = new ConcurrentQueue<byte[]>();
        private byte[] _Last;
        private byte[] _LastDequeue;

        public ClientRecorder(byte firstTime)
        {
            for (int i = 0; i < 10; ++i)
            {
                _Pool.Enqueue(new byte[10]);
            }
            _Last = new byte[] {
                (byte)(firstTime - 1),
                15, 15, 15,
                15, 15, 15,
                15, 15, 15,
            };
            _LastDequeue = new byte[10];
        }
        
        //queue mode (enqueue)
        public void Enqueue(IntPtr ptr)
        {
            _Queue.Enqueue(GetClientData(ptr));
        }

        //queue mode (dequeue)
        public bool TryDequeue(out byte[] ret)
        {
            if (_Queue.TryDequeue(out ret))
            {
                _Pool.Enqueue(_LastDequeue);
                _LastDequeue = ret;
                return true;
            }
            ret = null;
            return false;
        }

        //direct mode
        public byte[] Convert(IntPtr ptr)
        {
            var ret = GetClientData(ptr);
            _Pool.Enqueue(_LastDequeue);
            _LastDequeue = ret;
            return ret;
        }

        private byte[] GetFromPool()
        {
            byte[] ret;
            if (!_Pool.TryDequeue(out ret))
            {
                ret = new byte[10];
                for (int i = 0; i < 10; ++i)
                {
                    _Pool.Enqueue(new byte[10]);
                }
            }
            return ret;
        }

        private byte[] GetClientData(IntPtr ptr)
        {
            var data = GetFromPool();
            data[0] = Inc(_Last[0]);

            for (int i = 0; i < 9; ++i)
            {
                bool k = Marshal.ReadByte(ptr, KeyConfig.GetOriginalKeyIndex(i)) != 0;
                var byteLast = new ByteData(_Last[i + 1]);
                if (k == byteLast.State)
                {
                    if (byteLast.Frame < 15)
                    {
                        byteLast.Frame += 1;
                    }
                    data[i + 1] = byteLast.Data;
                }
                else
                {
                    byteLast.State = k;
                    byteLast.Change = (byte)((byteLast.Change + 1) & 7);
                    byteLast.Frame = 0;
                    data[i + 1] = byteLast.Data;
                }
            }

            _Pool.Enqueue(_Last);
            _Last = data;
            return data;
        }

        private byte Inc(byte b)
        {
            return (byte)((b + 1) & 255);
        }

        private int Diff(byte a, byte b)
        {
            if (a <= b)
            {
                return b - a;
            }
            return b + 256 - a;
        }
    }
}
