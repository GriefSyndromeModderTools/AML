using PluginUtils.Injection.Native;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace PluginUtils.Injection.File
{
    class FileAPIInjectorPlugin : IAMLPlugin
    {
        public void Init()
        {
            new InjectCreateFile().InjectSelf();
            new InjectReadFile().InjectSelf();
            new InjectSetFilePointer().InjectSelf();
            //TODO handle close handle
        }

        public void Load()
        {
        }

        private class InjectCreateFile : NativeWrapper
        {
            private delegate int CreateFileDelegate(IntPtr filename, int access, int share,
                IntPtr sec, int cd, int flags, int template);
            private CreateFileDelegate _Original;

            public InjectCreateFile()
            {
                this.AddRegisterRead(Register.EAX);
                this.AddRegisterRead(Register.EBP);
            }

            public void InjectSelf()
            {
                _Original = this.InjectFunctionPointer<CreateFileDelegate>(AddressHelper.CodeOffset(0x20E0B0), 28);
            }

            protected override void Triggered(NativeWrapper.NativeEnvironment env)
            {
                var p0 = env.GetParameterP(0);
                var p1 = env.GetParameterI(1);
                var p2 = env.GetParameterI(2);
                var p3 = env.GetParameterP(3);
                var p4 = env.GetParameterI(4);
                var p5 = env.GetParameterI(5);
                var p6 = env.GetParameterI(6);
                var ret = _Original(p0, p1, p2, p3, p4, p5, p6);
                env.SetReturnValue(ret);

                FileReplacement.OpenFile(Marshal.PtrToStringAnsi(p0), p1, ret);
            }
        }

        private class InjectReadFile : NativeWrapper
        {
            private delegate int ReadFileDelegate(int file, IntPtr buffer, int len, IntPtr read, IntPtr overlap);
            private ReadFileDelegate _Original;

            public InjectReadFile()
            {
                this.AddRegisterRead(Register.EAX);
                this.AddRegisterRead(Register.EBP);
            }

            public void InjectSelf()
            {
                _Original = this.InjectFunctionPointer<ReadFileDelegate>(AddressHelper.CodeOffset(0x20E09C), 20);
            }

            protected override void Triggered(NativeWrapper.NativeEnvironment env)
            {
                var p0 = env.GetParameterI(0);
                var p1 = env.GetParameterP(1);
                var p2 = env.GetParameterI(2);
                var p3 = env.GetParameterP(3);
                var p4 = env.GetParameterP(4);
                if (FileReplacement.ReadFile(p0, p1, p2, p3))
                {
                    env.SetReturnValue(1);
                }
                else
                {
                    env.SetReturnValue(_Original(p0, p1, p2, p3, p4));
                }
            }
        }

        private class InjectSetFilePointer : NativeWrapper
        {
            private delegate int SetFilePointerDelegate(int file, int dist, IntPtr dist2, int method);
            private SetFilePointerDelegate _Original;

            public InjectSetFilePointer()
            {
                this.AddRegisterRead(Register.EAX);
                this.AddRegisterRead(Register.EBP);
            }

            public void InjectSelf()
            {
                _Original = this.InjectFunctionPointer<SetFilePointerDelegate>(AddressHelper.CodeOffset(0x20E0A4), 16);
            }

            protected override void Triggered(NativeWrapper.NativeEnvironment env)
            {
                var p0 = env.GetParameterI(0);
                var p1 = env.GetParameterI(1);
                var p2 = env.GetParameterP(2);
                var p3 = env.GetParameterI(3);
                int ret;
                if (FileReplacement.SetFilePointer(p0, p1, p2, p3, out ret))
                {
                    env.SetReturnValue(ret);
                }
                else
                {
                    env.SetReturnValue(_Original(p0, p1, p2, p3));
                }
            }
        }
    }
}
