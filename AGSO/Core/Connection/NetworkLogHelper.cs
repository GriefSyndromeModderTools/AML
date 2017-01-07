using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace AGSO.Core.Connection
{
    class NetworkLogHelper
    {
        private static int _ProcIDi = Process.GetCurrentProcess().Id;
        private static byte[] _ProcID = BitConverter.GetBytes(Process.GetCurrentProcess().Id);
        private static object _Mutex = new object();
        private static byte[] _Zero = new byte[4];

        public static void Write(string type)
        {
            return;
            lock (_Mutex)
            {
                using (var f = File.Open("agso_network." + _ProcIDi + ".log", FileMode.Append))
                {
                    var b = Encoding.ASCII.GetBytes(type);
                    f.Write(BitConverter.GetBytes(b.Length), 0, 4);
                    f.Write(b, 0, b.Length);
                    f.Write(_Zero, 0, 4);
                }
            }
        }
        public static void Write(string type, byte[] data)
        {
            return;
            if (data == null)
            {
                Write(type);
                return;
            }
            lock (_Mutex)
            {
                using (var f = File.Open("agso_network." + _ProcIDi + ".log", FileMode.Append))
                {
                    var b = Encoding.ASCII.GetBytes(type);
                    f.Write(BitConverter.GetBytes(b.Length), 0, 4);
                    f.Write(b, 0, b.Length);
                    f.Write(BitConverter.GetBytes(data.Length), 0, 4);
                    f.Write(data, 0, data.Length);
                }
            }
        }
        private static byte[] _Buffer = new byte[1024];
        public static void Write(string type, AGSO.Network.Buffer data)
        {
            return;
            if (data == null)
            {
                Write(type);
                return;
            }
            Marshal.Copy(data.Pointer, _Buffer, 0, data.Length);
            lock (_Mutex)
            {
                using (var f = File.Open("agso_network." + _ProcIDi + ".log", FileMode.Append))
                {
                    var b = Encoding.ASCII.GetBytes(type);
                    f.Write(BitConverter.GetBytes(b.Length + 4 + data.Length), 0, 4);
                    f.Write(_ProcID, 0, 4);
                    f.Write(b, 0, b.Length);
                    f.Write(_Buffer, 0, data.Length);
                }
            }
        }
    }
}
