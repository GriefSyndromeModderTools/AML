using PluginUtils.Injection.Direct3D;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace EmptyDirectXDelegate
{
    class BufferInjection
    {
        public static IntPtr Instance { get { return _Com.Instance; } }

        private static ComInterfaceGenerator _Com = new ComInterfaceGenerator(typeof(ComFunctions));

        [ComClass(14)]
        private class ComFunctions
        {
            private static Guid _Guid1 = new Guid("B64BB1B5-FD70-4df6-BF91-19D0A12455E3"); //vb
            private static Guid _Guid2 = new Guid("7C9DD65E-D3F7-4529-ACEE-785830ACDE35"); //ib

            [ComMethodAttribute(0)]
            public static int QueryInterface(IntPtr ptr, IntPtr pGuid, IntPtr ret)
            {
                byte[] d = new byte[16];
                Marshal.Copy(pGuid, d, 0, 16);
                var g = new Guid(d);
                if (g == _Guid1 || g == _Guid2)
                {
                    Marshal.AddRef(ptr);
                    Marshal.WriteIntPtr(ret, ptr);
                    return 0;
                }
                return unchecked((int)0x80004002);
            }

            [ComMethodAttribute(1)]
            public static int AddRef(IntPtr ptr)
            {
                return 4;
            }

            [ComMethodAttribute(2)]
            public static int Release(IntPtr ptr)
            {
                return 3;
            }

            [ComMethodAttribute(3)]
            public static int GetDevice(IntPtr ptr, IntPtr ret)
            {
                Marshal.WriteIntPtr(ret, Direct3DHelper.Device); //const ref
                return 0;
            }

            [ComMethodAttribute(11)]
            public static int Lock(IntPtr ptr, int a, int b, IntPtr ret, int c)
            {
                Marshal.WriteIntPtr(ret, LockedMemoryRegion.Ptr );
                return 0;
            }

            [ComMethodAttribute(12)]
            public static int Unlock(IntPtr ptr)
            {
                return 0;
            }
        }
    }
}
