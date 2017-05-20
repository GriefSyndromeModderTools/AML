using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace PluginUtils.Injection.Native
{
    public class CodeModification
    {
        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool VirtualProtect(IntPtr lpAddress, uint dwSize,
           Protection flNewProtect, out Protection lpflOldProtect);

        private enum Protection
        {
            PAGE_NOACCESS = 0x01,
            PAGE_READONLY = 0x02,
            PAGE_READWRITE = 0x04,
            PAGE_WRITECOPY = 0x08,
            PAGE_EXECUTE = 0x10,
            PAGE_EXECUTE_READ = 0x20,
            PAGE_EXECUTE_READWRITE = 0x40,
            PAGE_EXECUTE_WRITECOPY = 0x80,
            PAGE_GUARD = 0x100,
            PAGE_NOCACHE = 0x200,
            PAGE_WRITECOMBINE = 0x400
        }

        static CodeModification()
        {
            _NopArray = new byte[256];
            for (int i = 0; i < _NopArray.Length; ++i)
            {
                _NopArray[i] = 0x90;
            }
        }

        public static void Modify(uint offset, params byte[] code)
        {
            Protection oldP;
            var addr = AddressHelper.CodeOffset(offset);
            VirtualProtect(addr, (uint)code.Length, Protection.PAGE_EXECUTE_READWRITE, out oldP);
            Marshal.Copy(code, 0, addr, code.Length);
        }

        public static void Modify(string module, uint offset, params byte[] code)
        {
            Protection oldP;
            var addr = AddressHelper.CodeOffset(module, offset);
            VirtualProtect(addr, (uint)code.Length, Protection.PAGE_EXECUTE_READWRITE, out oldP);
            Marshal.Copy(code, 0, addr, code.Length);
        }

        private static byte[] _NopArray;

        public static void FillNop(uint offset, int len)
        {
            IntPtr addr = AddressHelper.CodeOffset(offset);
            Protection oldP;
            VirtualProtect(addr, (uint)len, Protection.PAGE_EXECUTE_READWRITE, out oldP);
            int i;
            for (i = 0; i + 256 <= len; i += 256, addr += 256)
            {
                Marshal.Copy(_NopArray, 0, addr, 256);
            }
            Marshal.Copy(_NopArray, 0, addr, len - i);
        }

        public static void WritePointer(IntPtr addr, IntPtr val)
        {
            Protection p = Protection.PAGE_EXECUTE_READWRITE;
            Protection oldP;
            VirtualProtect(addr, 4, p, out oldP);

            Marshal.WriteIntPtr(addr, val);

            VirtualProtect(addr, 4, oldP, out oldP);
        }
    }
}
