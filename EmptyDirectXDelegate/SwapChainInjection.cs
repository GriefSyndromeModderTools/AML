using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace EmptyDirectXDelegate
{
    class SwapChainInjection
    {
        public static IntPtr Instance { get { return _Com.Instance; } }

        private static ComInterfaceGenerator _Com = new ComInterfaceGenerator(typeof(ComFunctions));
        
        [ComClass(10)]
        private class ComFunctions
        {
            private static Guid _Guid = new Guid("794950F2-ADFC-458a-905E-10A10B0B503B");

            [ComMethodAttribute(0)]
            public static int QueryInterface(IntPtr ptr, IntPtr pGuid, IntPtr ret)
            {
                byte[] d = new byte[16];
                Marshal.Copy(pGuid, d, 0, 16);
                if (new Guid(d) == _Guid)
                {
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
            public static int Present(IntPtr ptr, IntPtr a, IntPtr b, int c, IntPtr d, int e)
            {
                return 0;
            }
        }
    }
}
