using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace EmptyDirectXDelegate
{
    class TextureInjection
    {
        private static ComInterfaceGenerator _Com =
            new ComInterfaceGenerator(typeof(ComFunctions));

        public static IntPtr Create(int[] desc, int reference = 1)
        {
            var ret = Marshal.AllocHGlobal(12 + desc.Length * 4);
            Marshal.WriteIntPtr(ret, _Com.VTab);
            Marshal.WriteInt32(ret, 4, reference);
            Marshal.WriteIntPtr(ret, 8, SurfaceInjection.Create(desc, 1));
            Marshal.Copy(desc, 0, ret + 12, desc.Length);
            return ret;
        }

        [ComClass(22)]
        private class ComFunctions
        {
            private static Guid _Guid1 = new Guid("85C31227-3DE5-4f00-9B3A-F11AC38C18B5"); //tex
            private static Guid _Guid2 = new Guid("580CA87E-1D3C-4d54-991D-B7D3E3C298CE"); //base

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
                    Marshal.Release(Marshal.ReadIntPtr(ptr, 8));
                    Marshal.FreeHGlobal(ptr);
                }
                return x;
            }

            [ComMethodAttribute(9)]
            public static int Preload(IntPtr ptr)
            {
                return 0;
            }

            [ComMethodAttribute(17)]
            public static int GetLevelDesc(IntPtr ptr, int i, IntPtr ret)
            {
                byte[] d = new byte[4 * 8];
                var pData = ptr + 12;
                Marshal.Copy(pData, d, 0, d.Length);
                Marshal.Copy(d, 0, ret, d.Length);
                return 0;
            }

            [ComMethodAttribute(18)]
            public static int GetSurfaceLevel(IntPtr ptr, int i, IntPtr ret)
            {
                IntPtr s = Marshal.ReadIntPtr(ptr, 8);
                Marshal.AddRef(s);
                Marshal.WriteIntPtr(ret, s);
                return 0;
            }

            [ComMethodAttribute(19)]
            public static int LockRect(IntPtr ptr, int i, IntPtr ret, IntPtr r, int flag)
            {
                Marshal.WriteInt32(ret, 0, 100); //pitch
                Marshal.WriteIntPtr(ret, 4, LockedMemoryRegion.Ptr);
                return 0;
            }

            [ComMethodAttribute(20)]
            public static int UnlockRect(IntPtr ptr, int i)
            {
                return 0;
            }
        }
    }
}
