using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace PluginUtils.Injection.Native
{
    public class AssemblyCodeStorage
    {
        public static IntPtr WriteCode(byte[] code)
        {
            var ret = AllocateCode(code.Length);
            WriteCode(ret, code);
            return ret;
        }

        public static IntPtr AllocateCode(int length)
        {
            //TODO better implementation
            return VirtualAlloc(IntPtr.Zero, (IntPtr)length,
                AllocationType.COMMIT, MemoryProtection.EXECUTE_READWRITE);
        }

        public static void WriteCode(IntPtr addr, byte[] code)
        {
            Marshal.Copy(code, 0, addr, code.Length);
            Flush();
        }

        public static IntPtr AllocateIndirect()
        {
            if (_IndirectPointer.ToInt32() >= _IndirectEnd.ToInt32())
            {
                _IndirectPointer = Marshal.AllocHGlobal(128);
                _IndirectEnd = _IndirectPointer + 128;
            }
            var ret = _IndirectPointer;
            _IndirectPointer += 4;
            return ret;
        }

        public static void WriteIndirect(IntPtr addr, IntPtr val)
        {
            Marshal.WriteInt32(addr, val.ToInt32());
        }

        private static void Flush()
        {
        }

        private static IntPtr _IndirectPointer, _IndirectEnd;

        #region natives

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern IntPtr VirtualAlloc(IntPtr lpAddress, IntPtr dwSize,
           AllocationType flAllocationType, MemoryProtection flProtect);

        [Flags]
        private enum AllocationType : uint
        {
            COMMIT = 0x1000,
            RESERVE = 0x2000,
            RESET = 0x80000,
            LARGE_PAGES = 0x20000000,
            PHYSICAL = 0x400000,
            TOP_DOWN = 0x100000,
            WRITE_WATCH = 0x200000
        }

        [Flags]
        private enum MemoryProtection : uint
        {
            EXECUTE = 0x10,
            EXECUTE_READ = 0x20,
            EXECUTE_READWRITE = 0x40,
            EXECUTE_WRITECOPY = 0x80,
            NOACCESS = 0x01,
            READONLY = 0x02,
            READWRITE = 0x04,
            WRITECOPY = 0x08,
            GUARD_Modifierflag = 0x100,
            NOCACHE_Modifierflag = 0x200,
            WRITECOMBINE_Modifierflag = 0x400
        }

        #endregion
    }
}
