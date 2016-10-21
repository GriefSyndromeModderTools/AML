using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using SysFile = System.IO.File;

namespace PluginUtils.Injection.File
{
    class FileReplacement
    {
        private static Dictionary<string, IFileProxyFactory> _FactoryList = new Dictionary<string, IFileProxyFactory>();
        private static Dictionary<int, IFileProxy> _ActiveFiles = new Dictionary<int, IFileProxy>();

        public static void RegisterFile(string fullPath, IFileProxyFactory p)
        {
            if (_FactoryList.ContainsKey(fullPath))
            {
                throw new Exception("duplicate file replacement");
            }
            _FactoryList.Add(fullPath, p);
        }

        internal static void OpenFile(string path, int mode, int handle)
        {
            var fullPath = Path.GetFullPath(path);
            IFileProxyFactory fac;
            if (_FactoryList.TryGetValue(fullPath, out fac))
            {
                _ActiveFiles.Add(handle, fac.Create());
            }
        }

        internal static bool ReadFile(int handle, IntPtr buffer, int len, IntPtr read)
        {
            IFileProxy p;
            if (_ActiveFiles.TryGetValue(handle, out p))
            {
                Marshal.WriteInt32(read, p.Read(buffer, len));
                return true;
            }
            return false;
        }

        internal static bool SetFilePointer(int handle, int dist, IntPtr dist2, int method, out int ret)
        {
            IFileProxy p;
            if (_ActiveFiles.TryGetValue(handle, out p))
            {
                if (dist2 != IntPtr.Zero)
                {
                    //this should not happen
                    ret = -1;
                    return true;
                }
                ret = p.Seek(method, dist);
                return true;
            }
            ret = 0;
            return false;
        }
    }
}
