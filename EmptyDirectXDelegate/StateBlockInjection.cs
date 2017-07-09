using PluginUtils.Injection.Direct3D;
using PluginUtils.Injection.Native;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace EmptyDirectXDelegate
{
    class StateBlockInjection
    {
        public static void Inject(IntPtr device)
        {
            //ComInterfaceGenerator.InjectObject(device, typeof(DeviceFunctions));
        }

        private static ComInterfaceGenerator _ComObj =
            new ComInterfaceGenerator(typeof(ComFunctions));

        public static IntPtr Instance { get { return _ComObj.Instance; } }

        private static Guid _Guid = new Guid("B07C4FE5-310D-4ba8-A23C-4F0F206F218B");

        /*
            STDMETHOD(QueryInterface)(THIS_ REFIID riid, void** ppvObj) PURE;
            STDMETHOD_(ULONG,AddRef)(THIS) PURE;
            STDMETHOD_(ULONG,Release)(THIS) PURE;

            STDMETHOD(GetDevice)(THIS_ IDirect3DDevice9** ppDevice) PURE;
            STDMETHOD(Capture)(THIS) PURE;
            STDMETHOD(Apply)(THIS) PURE;
         */
        private class ComFunctions
        {
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
                return 2;
            }

            [ComMethodAttribute(2)]
            public static int Release(IntPtr ptr)
            {
                return 1;
            }

            [ComMethodAttribute(3)]
            public static int GetDevice(IntPtr ptr, IntPtr ret)
            {
                Marshal.WriteIntPtr(ret, Direct3DHelper.Device); //const ref
                return 0;
            }

            [ComMethodAttribute(4)]
            public static int Capture(IntPtr ptr)
            {
                return 0;
            }

            [ComMethodAttribute(5)]
            public static int Apply(IntPtr ptr)
            {
                return 0;
            }
        }
    }
}
