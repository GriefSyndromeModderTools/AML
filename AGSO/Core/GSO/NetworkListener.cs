using PluginUtils;
using PluginUtils.Injection.Native;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace AGSO.Core.GSO
{
    class NetworkListener : IAMLPlugin
    {
        private static NetworkListenerForm _Form;

        public void Init()
        {
            if (GSOHelper.IsGSO)
            {
                System.Windows.Forms.MessageBox.Show("form");
                WindowsHelper.Run(delegate()
                {
                    _Form = new NetworkListenerForm();
                    _Form.Show();
                });
            }

            GSOHelper.GSOLoaded += delegate()
            {
                new InjectSend().InjectSelf();
                new InjectRecv().InjectSelf();
            };
        }

        public void Load()
        {
        }

        private class InjectSend : NativeWrapper
        {
            private delegate int SendToDelegate(IntPtr s, IntPtr data, int len, int flags, IntPtr addr, int addelen);
            private SendToDelegate _Original;

            public InjectSend()
            {
                this.AddRegisterRead(Register.EAX);
                this.AddRegisterRead(Register.EBP);
            }

            public void InjectSelf()
            {
                _Original = this.InjectFunctionPointer<SendToDelegate>(AddressHelper.CodeOffset("gso", 0x1C274), 24);
            }

            protected override void Triggered(NativeWrapper.NativeEnvironment env)
            {
                var p0 = env.GetParameterP(0);
                var p1 = env.GetParameterP(1);
                var p2 = env.GetParameterI(2);
                var p3 = env.GetParameterI(3);
                var p4 = env.GetParameterP(4);
                var p5 = env.GetParameterI(5);
                env.SetReturnValue(_Original(p0, p1, p2, p3, p4, p5));
                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < p2; ++i)
                {
                    sb.Append(Marshal.ReadByte(p1, i).ToString("X2"));
                    sb.Append(' ');
                }
                _Form.Append("Send", sb.ToString());
            }
        }


        private class InjectRecv : NativeWrapper
        {
            private delegate int RecvFromDelegate(IntPtr s, IntPtr data, int len, int flags, IntPtr addr, IntPtr addelen);
            private RecvFromDelegate _Original;

            public InjectRecv()
            {
                this.AddRegisterRead(Register.EAX);
                this.AddRegisterRead(Register.EBP);
            }

            public void InjectSelf()
            {
                _Original = this.InjectFunctionPointer<RecvFromDelegate>(AddressHelper.CodeOffset("gso", 0x1C288), 24);
            }

            protected override void Triggered(NativeWrapper.NativeEnvironment env)
            {
                var p0 = env.GetParameterP(0);
                var p1 = env.GetParameterP(1);
                var p2 = env.GetParameterI(2);
                var p3 = env.GetParameterI(3);
                var p4 = env.GetParameterP(4);
                var p5 = env.GetParameterP(5);
                var ret = _Original(p0, p1, p2, p3, p4, p5);
                env.SetReturnValue(ret);
                if (ret >= 0)
                {
                    StringBuilder sb = new StringBuilder();
                    for (int i = 0; i < ret; ++i)
                    {
                        sb.Append(Marshal.ReadByte(p1, i).ToString("X2"));
                        sb.Append(' ');
                    }
                    _Form.Append("Recv", sb.ToString());
                }
            }
        }
    }
}
