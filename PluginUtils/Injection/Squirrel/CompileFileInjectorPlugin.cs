using PluginUtils.Injection.Native;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace PluginUtils.Injection.Squirrel
{
    //TODO 1 sq text code will not trigger
    //TODO 2 handle compile stack correctly
    class CompileFileInjectorPlugin : IAMLPlugin
    {
        private static string _LastFile;
        private static IntPtr _SavedTable = Marshal.AllocHGlobal(8);

        public void Init()
        {
            new InjectBeforeRun().InjectSelf();
            new InjectAfterRun().InjectSelf();
        }

        public void Load()
        {
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
                this.Inject(AddressHelper.CodeOffset(0xB71E3), 6);
            }

            protected override void Triggered(NativeWrapper.NativeEnvironment env)
            {
                if (SquirrelFunctions.getstackobj(SquirrelInjectorPlugin.SquirrelVM, -1, _SavedTable) == 0)
                {
                    //only save filename when get table succeeded
                    _LastFile = Marshal.PtrToStringAnsi(env.GetParameterP(0));
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
                this.Inject(AddressHelper.CodeOffset(0xB720B), 11);
            }

            protected override void Triggered(NativeWrapper.NativeEnvironment env)
            {
                if (_LastFile != null)
                {
                    CompileFileInjectionManager.AfterCompileFile(_LastFile, _SavedTable);
                    _LastFile = null;
                }
            }
        }
    }
}
