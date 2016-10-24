using AGSO.Core.Common;
using PluginUtils;
using PluginUtils.Injection.File;
using PluginUtils.Injection.Native;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace AGSO.Core.Input
{
    class InputInjectorPlugin : IAMLPlugin
    {
        public void Init()
        {
            WindowsHelper.MessageBox("input");
            new InjectCoCreateInstance().InjectSelf();
            InputHandler.InitInputHandler();
            if (InputHandler.ReplayLoaded)
            {
                InputHandler.KeyConfig = GetKeyCodeList();
                FileReplacement.RegisterFile(Path.GetFullPath("keyconfig.dat"), new KeyConfigFile());
            }
            else
            {
                var keyconfigData = File.ReadAllBytes(PathHelper.GetPath("keyconfig.dat"));
                InputHandler.KeyConfig = new int[9 * 3];
                Buffer.BlockCopy(keyconfigData, 0, InputHandler.KeyConfig, 0, 9 * 3 * 4);
            }
        }

        public void Load()
        {
        }

        private static int[] GetKeyCodeList()
        {
            var ret = new int[9 * 3];
            for (int i = 0; i < ret.Length; ++i)
            {
                ret[i] = i + 10;
            }
            return ret;
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

                Marshal.Copy(_Zero, 0, p2, p1);

                if (p0 == _InjectedInstance)
                {
                    Common.InputHandler.Aquire(p2);
                    env.SetReturnValue(0);
                }
                else
                {
                    var ret = _Original(p0, p1, p2);
                    env.SetReturnValue(ret);
                }
            }

            private static readonly byte[] _Zero = new byte[0x100];
        }

        private class KeyConfigFile : CachedModificationFileProxyFactory
        {
            public override byte[] Modify(byte[] data)
            {
                Buffer.BlockCopy(InputHandler.KeyConfig, 0, data, 0, 9 * 3 * 4);
                return data;
            }
        }

    }
}
