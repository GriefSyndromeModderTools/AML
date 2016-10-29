using PluginUtils.Injection.Native;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace PluginUtils.Injection.Squirrel
{
    class CompileFileInjectorPlugin : IAMLPlugin
    {
        private struct CompileFileCall
        {
            public SquirrelFunctions.SQObject Table;
            public string FileName;
        }
        private static Stack<CompileFileCall> _CallStack = new Stack<CompileFileCall>();

        public void Init()
        {
            new InjectBeforeArgs().InjectSelf();
            new InjectBeforeRun().InjectSelf();
            new InjectAfterRun().InjectSelf();
        }

        public void Load()
        {
        }

        private class InjectBeforeArgs : NativeWrapper
        {
            public InjectBeforeArgs()
            {
                this.AddRegisterRead(Register.EAX);
                this.AddRegisterRead(Register.EBP);
            }

            public void InjectSelf()
            {
                this.Inject(AddressHelper.CodeOffset(0xB71BF), 9); //binary
                this.Inject(AddressHelper.CodeOffset(0xB72DC), 9); //text
            }

            protected override void Triggered(NativeWrapper.NativeEnvironment env)
            {
                var filename = Marshal.PtrToStringAnsi(env.GetParameterP(0));
                CompileFileInjectionManager.BeforeCompileFile(filename);
            }
        }

        private class InjectBeforeRun : NativeWrapper
        {
            public InjectBeforeRun()
            {
                this.AddRegisterRead(Register.EAX);
                this.AddRegisterRead(Register.EBP);
            }

            public void InjectSelf()
            {
                this.Inject(AddressHelper.CodeOffset(0xB71E3), 6); //binary
                this.Inject(AddressHelper.CodeOffset(0xB7300), 6); //text
            }

            protected override void Triggered(NativeWrapper.NativeEnvironment env)
            {
                CompileFileCall c = new CompileFileCall();
                if (SquirrelFunctions.getstackobj(SquirrelInjectorPlugin.SquirrelVM, -1, out c.Table) == 0)
                {
                    c.FileName = Marshal.PtrToStringAnsi(env.GetParameterP(0));
                    _CallStack.Push(c);
                }
            }
        }

        private class InjectAfterRun : NativeWrapper
        {
            public InjectAfterRun()
            {
                this.AddRegisterRead(Register.EAX);
                this.AddRegisterRead(Register.EBP);
            }

            public void InjectSelf()
            {
                this.Inject(AddressHelper.CodeOffset(0xB720B), 11); //binary
                this.Inject(AddressHelper.CodeOffset(0xB7328), 11); //text
            }

            protected override void Triggered(NativeWrapper.NativeEnvironment env)
            {
                var c = _CallStack.Pop();
                CompileFileInjectionManager.AfterCompileFile(c.FileName, ref c.Table);
            }
        }
    }
}
