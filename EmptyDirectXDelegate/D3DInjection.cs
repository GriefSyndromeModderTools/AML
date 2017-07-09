using PluginUtils.Injection.Direct3D;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace EmptyDirectXDelegate
{
    class D3DInjection
    {
        private static ComInterfaceGenerator _Com = new ComInterfaceGenerator(typeof(ComFunctions));

        public static IntPtr Instance { get { return _Com.Instance; } }

        [ComClass(17)]
        private class ComFunctions
        {
            private static Guid _Device = new Guid("81BDCBCA-64D4-426d-AE8D-AD0147F4275C");
            private static byte[] _Data = new byte[16];
            [ComMethod(0)]
            public static int QueryInterface(IntPtr ptr, IntPtr guid, IntPtr ret)
            {
                Marshal.Copy(guid, _Data, 0, 16);
                if (new Guid(_Data) != _Device)
                {
                    Marshal.WriteInt32(ret, 0);
                    return unchecked((int)0x80004002);
                }
                Marshal.WriteIntPtr(ret, ptr);
                return 0;
            }
            [ComMethod(1)]
            public static int AddRef(IntPtr ptr)
            {
                return 10;
            }
            [ComMethod(2)]
            public static int Release(IntPtr ptr)
            {
                return 9;
            }
            [ComMethod(8)]
            public static int GetAdapterDisplayMode(IntPtr ptr, int adapter, IntPtr ret)
            {
                Marshal.WriteInt32(ret, 0, 1024);
                Marshal.WriteInt32(ret, 4, 768);
                Marshal.WriteInt32(ret, 8, 0);
                Marshal.WriteInt32(ret, 12, 22);
                return 0;
            }
            [ComMethod(10)]
            public static int CheckDeviceFormat(IntPtr ptr, int adapter, int type, int format, int usage, int res, int check)
            {
                return 0;
            }
            [ComMethod(16)]
            public static int CreateDevice(IntPtr pD3d, int adapter, int deviceType,
                IntPtr hWnd, int behaviorFlags, IntPtr pPresentParameters, IntPtr pResult)
            {
                Marshal.WriteIntPtr(pResult, EmptyCallInjection.Instance);
                Direct3DHelper.ReinjectDeviceObject(EmptyCallInjection.Instance);
                return 0;
            }
        }
    }
}
