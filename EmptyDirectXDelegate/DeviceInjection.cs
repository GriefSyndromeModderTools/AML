using PluginUtils;
using PluginUtils.Injection.Native;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace EmptyDirectXDelegate
{
    class DeviceInjection
    {
        public static void Inject(IntPtr device)
        {
            WindowsHelper.MessageBox("device injection");
            //new QueryInterfaceInjection().InjectSelf(device);
        }

        class QueryInterfaceInjection : NativeWrapper
        {
            private delegate int QueryInterfaceDelegate(IntPtr d, IntPtr guid, IntPtr ret);
            private QueryInterfaceDelegate _Original;

            public void InjectSelf(IntPtr d)
            {
                this.AddRegisterRead(Register.EAX);
                this.AddRegisterRead(Register.EBP);
                _Original = InjectFunctionPointer<QueryInterfaceDelegate>(AddressHelper.VirtualTable(d, 0), 12);
            }

            private static Guid _Device = new Guid("d0223b96-bf7a-43fd-92bd-a43b0d82b9eb");
            private byte[] _Data = new byte[16];
            protected override void Triggered(NativeWrapper.NativeEnvironment env)
            {
                var p0 = env.GetParameterP(0);
                var p1 = env.GetParameterP(1);
                var p2 = env.GetParameterP(2);
                Marshal.Copy(p1, _Data, 0, 16);
                if (new Guid(_Data) != _Device)
                {
                    Marshal.WriteInt32(p2, 0);
                    env.SetReturnValue(unchecked((int)0x80004002));
                    return;
                }
                Marshal.AddRef(p0);
                Marshal.WriteIntPtr(p2, p0);
                env.SetReturnValue(0);
                //env.SetReturnValue(_Original(p0, p1, p2));
            }
        }
    }
}
