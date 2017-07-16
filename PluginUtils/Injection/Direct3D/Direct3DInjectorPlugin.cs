using PluginUtils.Injection.Native;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace PluginUtils.Injection.Direct3D
{
    [Plugin(DependentPlugin = typeof(PluginUtilsMainPlugin))]
    class Direct3DInjectorPlugin : IAMLPlugin
    {
        //private static IntPtr _Device;

        public void Init()
        {
            new InjectDirect3DCreate9().InjectSelf();
        }

        public void Load()
        {
        }

        private class InjectDirect3DCreate9 : NativeWrapper
        {
            private delegate IntPtr CreateDeviceDelegate(int version);
            private CreateDeviceDelegate _Original;

            public InjectDirect3DCreate9()
            {
                this.AddRegisterRead(Register.EBP);
            }

            public void InjectSelf()
            {
                _Original = this.InjectFunctionPointer<CreateDeviceDelegate>(AddressHelper.CodeOffset(0x20E2D0), 4);
            }

            protected override void Triggered(NativeWrapper.NativeEnvironment env)
            {
                var obj = _Original(env.GetParameterI(0));
                new InjectD3DCreateDevice().InjectSelf(obj);
                env.SetReturnValue(obj);
            }
        }

        internal class InjectD3DCreateDevice : NativeWrapper
        {
            private delegate int CreateDeviceDelegate(
                IntPtr pD3d,
                int adapter,
                int deviceType,
                IntPtr hWnd,
                int behaviorFlags,
                IntPtr pPresentParameters,
                IntPtr pResult);
            private CreateDeviceDelegate _Original;

            public InjectD3DCreateDevice()
            {
                this.AddRegisterRead(Register.EBP);
            }

            public void InjectSelf(IntPtr obj)
            {
                _Original = this.InjectFunctionPointer<CreateDeviceDelegate>(AddressHelper.VirtualTable(obj, 16), 28);
            }

            protected override void Triggered(NativeWrapper.NativeEnvironment env)
            {
                var pResult = env.GetParameterP(6);
                var ret = _Original(
                    env.GetParameterP(0),
                    env.GetParameterI(1),
                    env.GetParameterI(2),
                    env.GetParameterP(3),
                    env.GetParameterI(4),
                    env.GetParameterP(5),
                    pResult);
                env.SetReturnValue((IntPtr)ret);
                Direct3DHelper.OnDeviceCreated(Marshal.ReadIntPtr(pResult));
            }
        }
    }
}
