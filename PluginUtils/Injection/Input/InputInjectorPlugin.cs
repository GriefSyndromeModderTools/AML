using PluginUtils.Injection.Native;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace PluginUtils.Injection.Input
{
    [Plugin(DependentPlugin = typeof(PluginUtilsMainPlugin))]
    class InputInjectorPlugin : IAMLPlugin
    {
        public void Init()
        {
            new InjectCoCreateInstance().InjectSelf();

            //allow receiving input even when GetDeviceState fails (according to gso 2.02)
            CodeModification.FillNop(0xC5FF8, 5);
        }

        public void Load()
        {
        }

        private class InjectCoCreateInstance : NativeWrapper
        {
            private delegate int CoCreateInstanceDelegate(
                IntPtr rclsid, IntPtr pUnkOuter, int dwClsContext, IntPtr riid, IntPtr ppv);

            private CoCreateInstanceDelegate _Original;

            public InjectCoCreateInstance()
            {
                this.AddRegisterRead(Register.EAX);
                this.AddRegisterRead(Register.EBP);
            }

            public void InjectSelf()
            {
                _Original = this.InjectFunctionPointer<CoCreateInstanceDelegate>(AddressHelper.CodeOffset(0x20E37C), 0x14);
            }

            protected override void Triggered(NativeWrapper.NativeEnvironment env)
            {
                var p0 = env.GetParameterP(0);
                var p1 = env.GetParameterP(1);
                var p2 = env.GetParameterI(2);
                var p3 = env.GetParameterP(3);
                var p4 = env.GetParameterP(4);

                var ret = _Original(p0, p1, p2, p3, p4);
                env.SetReturnValue(ret);

                if (p2 == 0x17)
                {
                    new InjectCreateDevice().InjectSelf(Marshal.ReadIntPtr(p4));
                }
            }
        }

        private class InjectCreateDevice : NativeWrapper
        {
            private delegate int CreateDeviceDelegate(IntPtr p0, IntPtr p1, IntPtr p2, IntPtr p3);
            private CreateDeviceDelegate _Original;

            private static bool _Injected = false;

            public InjectCreateDevice()
            {
                this.AddRegisterRead(Register.EAX);
                this.AddRegisterRead(Register.EBP);
            }

            public void InjectSelf(IntPtr co)
            {
                _Original = this.InjectFunctionPointer<CreateDeviceDelegate>(AddressHelper.VirtualTable(co, 3), 16);
            }

            protected override void Triggered(NativeWrapper.NativeEnvironment env)
            {
                var p0 = env.GetParameterP(0);
                var p1 = env.GetParameterP(1);
                var p2 = env.GetParameterP(2);
                var p3 = env.GetParameterP(3);

                var ret = _Original(p0, p1, p2, p3);
                env.SetReturnValue(ret);

                if (!_Injected)
                {
                    _Injected = true;
                    new InjectGetDeviceState().InjectSelf(Marshal.ReadIntPtr(p2));
                }
            }
        }

        private class InjectGetDeviceState : NativeWrapper
        {
            private delegate int GetDeviceStateDelegate(IntPtr p0, int p1, IntPtr p2);
            private GetDeviceStateDelegate _Original;

            public static bool _Injected = false;
            private static IntPtr _InjectedInstance;

            public InjectGetDeviceState()
            {
                this.AddRegisterRead(Register.EAX);
                this.AddRegisterRead(Register.EBP);
            }

            public void InjectSelf(IntPtr co)
            {
                _Original = this.InjectFunctionPointer<GetDeviceStateDelegate>(AddressHelper.VirtualTable(co, 9), 12);
                _Injected = true;
                _InjectedInstance = co;
            }

            protected override void Triggered(NativeWrapper.NativeEnvironment env)
            {
                var p0 = env.GetParameterP(0);
                var p1 = env.GetParameterI(1);
                var p2 = env.GetParameterP(2);

                InputManager.ZeroInputData(p2, p1);
                var ret = _Original(p0, p1, p2);

                if (p0 == _InjectedInstance && InputManager.HandleAll(p2))
                {
                    env.SetReturnValue(ret);
                }
                else
                {
                    env.SetReturnValue(ret);
                }
            }
        }
    }
}
