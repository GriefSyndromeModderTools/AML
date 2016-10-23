using PluginUtils.Injection.Native;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace PluginUtils.Injection.Squirrel
{
    //griefsyndrome can not directly run callbacks
    class NativeClosureFixCode : IAMLPlugin
    {
        //TODO move this to public api
        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool VirtualProtect(IntPtr lpAddress, uint dwSize,
           Protection flNewProtect, out Protection lpflOldProtect);

        public enum Protection
        {
            PAGE_NOACCESS = 0x01,
            PAGE_READONLY = 0x02,
            PAGE_READWRITE = 0x04,
            PAGE_WRITECOPY = 0x08,
            PAGE_EXECUTE = 0x10,
            PAGE_EXECUTE_READ = 0x20,
            PAGE_EXECUTE_READWRITE = 0x40,
            PAGE_EXECUTE_WRITECOPY = 0x80,
            PAGE_GUARD = 0x100,
            PAGE_NOCACHE = 0x200,
            PAGE_WRITECOMBINE = 0x400
        }

        public void Init()
        {
            Protection oldP;
            {
                //World2D.SetXXXXFunction
                //+5615 -> nop nop
                var code = AddressHelper.CodeOffset(0x5615);
                VirtualProtect(code, 2, Protection.PAGE_EXECUTE_READWRITE, out oldP);
                Marshal.WriteByte(code, 0x90);
                Marshal.WriteByte(code, 1, 0x90);
            }
            {
                //World2D.SetXXXXFunction
                //+5615 -> nop nop
                var code = AddressHelper.CodeOffset(0x2DA3);
                VirtualProtect(code, 2, Protection.PAGE_EXECUTE_READWRITE, out oldP);
                Marshal.WriteByte(code, 0x90);
                Marshal.WriteByte(code, 1, 0x90);
            }
            {
                //game may crashe while exiting due to sq hack, just terminate the process
                new InjectTerminateWhenCrash().InjectSelf();
            }
        }

        public void Load()
        {
        }

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
