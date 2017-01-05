using PluginUtils;
using PluginUtils.Injection.Direct3D;
using PluginUtils.Injection.Native;
using PluginUtils.Injection.Squirrel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace RenderOffset
{
    [Plugin]
    class RenderOffsetPlugin : IAMLPlugin
    {
        private static IntPtr _Device;

        public void Init()
        {
            SquirrelHelper.RegisterGlobalFunction("SetDXOffsetKuma", SetDXOffsetKuma);
            Direct3DHelper.InjectDevice(device => new InjectDrawPrimitiveUP().InjectSelf(device));
        }

        private static float _OffsetX, _OffsetY;
        public static int SetDXOffsetKuma(IntPtr p)
        {
            int x, y;
            SquirrelFunctions.getinteger(p, 2, out x);
            SquirrelFunctions.getinteger(p, 3, out y);
            _OffsetX = (float)x;
            _OffsetY = (float)y;
            return 0;
        }

        public void Load()
        {
        }

        private class InjectDrawPrimitiveUP : NativeWrapper
        {
            private delegate int DrawPrimitiveUPDelegate(IntPtr p0, int p1, int p2, IntPtr p3, int p4);
            private DrawPrimitiveUPDelegate _Original;

            public InjectDrawPrimitiveUP()
            {
                this.AddRegisterRead(Register.EAX);
                this.AddRegisterRead(Register.EBP);
            }

            public void InjectSelf(IntPtr d)
            {
                _Original = this.InjectFunctionPointer<DrawPrimitiveUPDelegate>(AddressHelper.VirtualTable(d, 83), 20);
            }

            private float[] _Buffer = new float[7 * 4];

            protected override void Triggered(NativeWrapper.NativeEnvironment env)
            {
                System.Windows.Forms.MessageBox.Show("draw");
                var p0 = env.GetParameterP(0);
                var p1 = env.GetParameterI(1);
                var p2 = env.GetParameterI(2);
                var p3 = env.GetParameterP(3);
                var p4 = env.GetParameterI(4);

                Marshal.Copy(p3, _Buffer, 0, _Buffer.Length);
                for (int i = 0; i < 4; ++i)
                {
                    _Buffer[i * 7] += _OffsetX;
                    _Buffer[i * 7 + 1] += _OffsetY;
                }
                Marshal.Copy(_Buffer, 0, p3, _Buffer.Length);

                env.SetReturnValue(_Original(p0, p1, p2, p3, p4));
            }
        }

    }
}
