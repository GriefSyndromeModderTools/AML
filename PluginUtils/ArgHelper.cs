using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace PluginUtils
{
    public class ArgHelper
    {
        private static object _Mutex = new object();
        private static string[] _Args;

        public static void Set(IntPtr ptr)
        {
            lock (_Mutex)
            {
                if (_Args != null)
                {
                    return;
                }

                int len = Marshal.ReadInt32(ptr);
                byte[] data = new byte[len];
                Marshal.Copy(ptr + 4, data, 0, len);

                List<string> list = new List<string>();
                using (var ms = new MemoryStream(data))
                {
                    using (var br = new BinaryReader(ms))
                    {
                        while (br.BaseStream.Length > br.BaseStream.Position)
                        {
                            list.Add(br.ReadString());
                        }
                    }
                }

                _Args = list.ToArray();
            }
        }

        public static int Count { get { return _Args.Length; } }

        public static string Get(int index)
        {
            return _Args[index];
        }

        public static string GetFollowed(string arg)
        {
            var index = Array.FindIndex(_Args, x => x == arg);
            if (index == -1 || index == _Args.Length - 1)
            {
                return null;
            }
            return _Args[index + 1];
        }
    }
}
