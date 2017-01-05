using PluginUtils.Injection.Native;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace PluginUtils.Injection.Squirrel
{
    //fix griefsyndrome can not directly run native callbacks
    [Plugin(Name = "NativeClosureFixCode", RawVersion = "1.0")]
    class NativeClosureFixCode : IAMLPlugin
    {
        public void Init()
        {
            {
                //World2D.SetXXXXFunction and CreateActor
                //old:
                //  cmp dword [eax], 0x08000100
                //  jne ...
                //new:
                //  test dword [eax], 0x00000300
                //  jz ...
                CodeModification.Modify(0x560F, 0xF7, 0x00, 0x00, 0x03, 0x00, 0x00, 0x74);
                CodeModification.Modify(0x2D9D, 0xF7, 0x00, 0x00, 0x03, 0x00, 0x00, 0x74);
            }
        }

        public void Load()
        {
        }

        //hacking sq no longer cause crash so this is not used now
        private class InjectTerminateWhenCrash : NativeWrapper
        {
            private delegate void PostQuitMessageDelegate(int a);
            private PostQuitMessageDelegate _Original;

            public InjectTerminateWhenCrash()
            {
                this.AddRegisterRead(Register.EAX);
                this.AddRegisterRead(Register.EBP);
            }

            public void InjectSelf()
            {
                _Original = this.InjectFunctionPointer<PostQuitMessageDelegate>(AddressHelper.CodeOffset(0x20E284), 8);
            }

            protected override void Triggered(NativeWrapper.NativeEnvironment env)
            {
                //exit now
                Environment.Exit(0);
            }
        }
    }
}
