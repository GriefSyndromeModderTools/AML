using PluginUtils;
using PluginUtils.Injection.Native;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace AGSO.Misc
{
    [Plugin(DependentPlugin = typeof(AGSOMainPlugin))]
    class GameWindowTitleFix : IAMLPlugin
    {
        public static int Handle { get; private set; }

        public void Init()
        {
            new FixTitle().InjectSelf();
        }

        public void Load()
        {
        }

        public class GSOForgroundCheck : NativeWrapper
        {
            private delegate int GetForegroundWindowDelegate();
            private GetForegroundWindowDelegate _Original;

            public GSOForgroundCheck()
            {
                this.AddRegisterRead(Register.EAX);
                this.AddRegisterRead(Register.EBP);
                System.Windows.Forms.MessageBox.Show("gso");
            }

            public void InjectSelf()
            {
                var addr = AddressHelper.CodeOffset("gso", 0x1C204);
                if (addr == IntPtr.Zero)
                {
                    return;
                }
                _Original = this.InjectFunctionPointer<GetForegroundWindowDelegate>(addr, 0);
            }

            protected override void Triggered(NativeWrapper.NativeEnvironment env)
            {
                if (Handle != 0)
                {
                    env.SetReturnValue(Handle);
                }
                else
                {
                    env.SetReturnValue(_Original());
                }
            }
        }


        public class FixTitle : NativeWrapper
        {
            private delegate int CreateWindowExADelegate(
                int dwExStyle,
                IntPtr lpClassName,
                IntPtr lpWindowName,
                int dwStyle,
                int x,
                int y,
                int nWidth,
                int nHeight,
                IntPtr hWndParent,
                IntPtr hMenu,
                IntPtr hInstance,
                IntPtr lpParam);

            private CreateWindowExADelegate _Original;
            private static readonly string _NewTitle = "Griefsyndrome Online";
            private IntPtr _NewTitlePtr;

            public FixTitle()
            {
                this.AddRegisterRead(Register.EAX);
                this.AddRegisterRead(Register.EBP);

                var data = Encoding.Default.GetBytes(_NewTitle.ToCharArray());
                _NewTitlePtr = Marshal.AllocHGlobal(data.Length + 2);
                Marshal.Copy(data, 0, _NewTitlePtr, data.Length);
                Marshal.WriteInt16(_NewTitlePtr, data.Length, 0);
            }

            public void InjectSelf()
            {
                _Original = this.InjectFunctionPointer<CreateWindowExADelegate>(AddressHelper.CodeOffset(0x20E258), 0x30);
            }

            protected override void Triggered(NativeWrapper.NativeEnvironment env)
            {
                var p0 = env.GetParameterI(0);
                var p1 = env.GetParameterP(1);
                var p2 = env.GetParameterP(2);
                var p3 = env.GetParameterI(3);
                var p4 = env.GetParameterI(4);
                var p5 = env.GetParameterI(5);
                var p6 = env.GetParameterI(6);
                var p7 = env.GetParameterI(7);
                var p8 = env.GetParameterP(8);
                var p9 = env.GetParameterP(9);
                var p10 = env.GetParameterP(10);
                var p11 = env.GetParameterP(11);
                p2 = _NewTitlePtr;
                var ret = _Original(p0, p1, p2, p3, p4, p5, p6, p7, p8, p9, p10, p11);
                env.SetReturnValue(ret);
                Handle = ret;

                new GSOForgroundCheck().InjectSelf();
            }
        }
    }
}
