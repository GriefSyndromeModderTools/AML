using PluginUtils.Injection.Native;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace EmptyDirectXDelegate
{
    //If this d3dx function is not executed, we can reduce the number
    //of functions in Texture/Surface interface
    //this may cause png texture to be loaded as dds
    class D3DXCreateTextureInjection : NativeWrapper
    {
        private delegate int DDelegate(IntPtr d, IntPtr p, int l, IntPtr ret);

        public void InjectSelf()
        {
            this.AddRegisterRead(Register.EAX);
            this.AddRegisterRead(Register.EBP);
            InjectFunctionPointer<DDelegate>(AddressHelper.CodeOffset(0x20E338), 16);
        }

        protected override void Triggered(NativeWrapper.NativeEnvironment env)
        {
            var desc = new int[]
            {
                21, 1, 0, 0, 0, 0, 800, 600
            };
            var ret = TextureInjection.Create(desc);
            Marshal.WriteIntPtr(env.GetParameterP(3), ret);
            env.SetReturnValue(0);
        }
    }
}
