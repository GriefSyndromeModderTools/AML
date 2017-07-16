using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace EmptyDirectXDelegate
{
    class SurfaceInjection
    {
        private static ComInterfaceGenerator _Com =
            new ComInterfaceGenerator(typeof(ComFunctions));

        public static IntPtr Create(int[] desc, int reference = 1)
        {
            var data = Marshal.AllocHGlobal(desc.Length * 4);
            Marshal.Copy(desc, 0, data, desc.Length);
            return Create(data, reference);
        }
        public static IntPtr Create(IntPtr desc, int reference = 1)
        {
            var ret = Marshal.AllocHGlobal(12);
            Marshal.WriteIntPtr(ret, _Com.VTab);
            Marshal.WriteInt32(ret, 4, reference);
            Marshal.WriteIntPtr(ret, 8, desc);
            return ret;
        }

        [ComClass(17)]
        private class ComFunctions
        {
            private static Guid _Guid1 = new Guid("0CFBAF3A-9FF6-429a-99B3-A2796AF8B89B");
            private static Guid _Guid2 = new Guid("05EEC05D-8F7D-4362-B999-D1BAF357C704");

            [ComMethodAttribute(0)]
            public static int QueryInterface(IntPtr ptr, IntPtr pGuid, IntPtr ret)
            {
                byte[] d = new byte[16];
                Marshal.Copy(pGuid, d, 0, 16);
                var g = new Guid(d);
                if (g == _Guid1 || g == _Guid2)
                {
                    Marshal.WriteIntPtr(ret, ptr);
                    return 0;
                }
                return unchecked((int)0x80004002);
            }

            [ComMethodAttribute(1)]
            public static int AddRef(IntPtr ptr)
            {
                var x = Marshal.ReadInt32(ptr, 4) + 1;
                Marshal.WriteInt32(ptr, 4, x);
                return x;
            }

            [ComMethodAttribute(2)]
            public static int Release(IntPtr ptr)
            {
                var x = Marshal.ReadInt32(ptr, 4) - 1;
                Marshal.WriteInt32(ptr, 4, x);
                if (x == 0)
                {
                    Marshal.FreeHGlobal(ptr);
                }
                return x;
            }

            [ComMethodAttribute(12)]
            public static int GetDesc(IntPtr ptr, IntPtr ret)
            {
                byte[] d = new byte[4 * 8];
                var pData = Marshal.ReadIntPtr(ptr, 8);
                Marshal.Copy(pData, d, 0, d.Length);
                Marshal.Copy(d, 0, ret, d.Length);
                return 0;
            }

            [ComMethodAttribute(13)]
            public static int LockRect(IntPtr ptr, IntPtr ret, IntPtr r, int flag)
            {
                Marshal.WriteInt32(ret, 0, 100); //pitch
                Marshal.WriteIntPtr(ret, 4, LockedMemoryRegion.Ptr);
                return 0;
            }

            [ComMethodAttribute(14)]
            public static int UnlockRect(IntPtr ptr)
            {
                return 0;
            }
        }
    }
}
