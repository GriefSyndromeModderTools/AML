using PluginUtils;
using PluginUtils.Injection.Native;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EmptyDirectXDelegate
{
    [Plugin(Name = "EmptyDirectXDelegate", RawVersion = "0.1")]
    public class EmptyDirectXDelegatePlugin : IAMLPlugin
    {
        public void Init()
        {
            try
            {
                WindowsHelper.MessageBox("empty load");

                //PUInject(StateBlockInjection.Inject);
                //PUInject(EmptyCallInjection.Inject);
                new D3DXCreateTextureInjection().InjectSelf();
                //PUInject(DeviceInjection.Inject);

                //disable time check at the startup
                //this value will be write to 0x2AC51C and if later controlling is needed 
                //0x2AC51C must be modified
                CodeModification.Modify(0xD252B + 6, 0, 0, 0, 0);

                //show fps on title (very small effect on speed)
                PluginUtils.Injection.Direct3D.Direct3DHelper.InjectDevice(d =>
                {
                    CodeModification.Modify(0x2AC11E, 1);
                });
            }
            catch (Exception e)
            {
                WindowsHelper.MessageBox(e.ToString());
            }
        }

        public void Load()
        {
            try
            {
                //so it can go after the PluginUtils injection
                new InjectDirect3DCreate9().InjectSelf();
            }
            catch (Exception e)
            {
                WindowsHelper.MessageBox(e.ToString());
            }
        }

        private void PUInject(Action<IntPtr> m)
        {
            PluginUtils.Injection.Direct3D.Direct3DHelper.InjectDevice(m);
        }

        //private static StreamWriter _Log = new StreamWriter(@"E:\empty.log");
        private static volatile bool _CreatingDevice = false;

        private class InjectDirect3DCreate9 : NativeWrapper
        {
            private delegate IntPtr CreateDeviceDelegate(int version);
            private CreateDeviceDelegate _Original;

            public InjectDirect3DCreate9()
            {
                this.AddRegisterRead(Register.EAX);
                this.AddRegisterRead(Register.EBP);
            }

            public void InjectSelf()
            {
                _Original = this.InjectFunctionPointer<CreateDeviceDelegate>(AddressHelper.CodeOffset(0x20E2D0), 4);
            }

            protected override void Triggered(NativeWrapper.NativeEnvironment env)
            {
                env.SetReturnValue(D3DInjection.Instance);
                //var obj = _Original(env.GetParameterI(0));
                ////0-16
                //for (int i = 0; i < 16; ++i)
                //{
                //    new MethodIndexInjection().InjectSelf(obj, i);
                //}
                //env.SetReturnValue(obj);
            }
        }

        private class MethodIndexInjection : SimpleLogInjection
        {
            public int Index;

            public void InjectSelf(IntPtr obj, int index)
            {
                Index = index;
                InjectFunctionPointer(AddressHelper.VirtualTable(obj, index));
            }

            protected override void Triggered()
            {
                if (!_CreatingDevice)
                {
                    var addr = GetRelativeAddr(GetStackTrace(8));
                    //_Log.WriteLine("{0}, {1}, {2} {3} {4} {5} {6} {7} {8} {9}",
                    //    Index.ToString().PadLeft(4),
                    //    GetCallingPoint(),
                    //    addr[0], addr[1], addr[2], addr[3],
                    //    addr[4], addr[5], addr[6], addr[7]);
                }
            }
        }

        private class CreateDeviceInjection : NativeWrapper
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

            public void InjectSelf(IntPtr d3d)
            {
                this.AddRegisterRead(Register.EAX);
                this.AddRegisterRead(Register.EBP);
                _Original = this.InjectFunctionPointer<CreateDeviceDelegate>(AddressHelper.VirtualTable(d3d, 16), 28);
            }

            protected override void Triggered(NativeWrapper.NativeEnvironment env)
            {
                var pResult = env.GetParameterP(6);
                _CreatingDevice = true;
                var ret = _Original(
                    env.GetParameterP(0),
                    env.GetParameterI(1),
                    env.GetParameterI(2),
                    env.GetParameterP(3),
                    env.GetParameterI(4),
                    env.GetParameterP(5),
                    pResult);
                _CreatingDevice = false;
                env.SetReturnValue((IntPtr)ret);
            }
        }
    }
}
