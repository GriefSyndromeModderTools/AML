using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace PluginUtils.Injection.File
{
    class SimpleFileProxy : IFileProxy
    {
        private byte[] _Data;
        private int _Position;

        public SimpleFileProxy(byte[] data)
        {
            _Data = data;
        }

        private int Seek(int pos)
        {
            _Position = pos;
            return pos;
        }

        public int Seek(int method, int num)
        {
            switch (method)
            {
            case 0:
                return Seek(num);
            case 1:
                return Seek(_Position + num);
            case 2:
                return Seek(_Data.Length + num);
            }
            return 0;
        }

        public int Read(IntPtr buffer, int max)
        {
            int len = max;
            if (_Position + len > _Data.Length)
            {
                len = _Data.Length - _Position;
            }
            Marshal.Copy(_Data, _Position, buffer, len);
            Seek(_Position + len);
            return len;
        }
    }
}
