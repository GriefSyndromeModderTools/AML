using PluginUtils.Injection.Native;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PluginUtils.Injection.Squirrel
{
    class SquirrelInjectorPlugin : IAMLPlugin
    {
        public static IntPtr SquirrelVM { get; private set; }
        public static readonly Dictionary<string, SquirrelFuncDelegate> UnregisteredFunction =
            new Dictionary<string, SquirrelFuncDelegate>();

        public void Init()
        {
            new InjectSquirrelVM().InjectSelf();
        }

        public void Load()
        {
        }

        private class InjectSquirrelVM : NativeWrapper
        {
            public InjectSquirrelVM()
            {
                this.AddRegisterRead(Register.EAX);
            }

            public void InjectSelf()
            {
                this.Inject(AddressHelper.CodeOffset(0xB69AA), 6);
            }

            protected override void Triggered(NativeWrapper.NativeEnvironment env)
            {
                var pVM = env.GetRegister(Register.EAX);
                SquirrelVM = pVM;

                SquirrelFunctions.pushroottable(pVM);
                SquirrelFunctions.pushstring(pVM, "MY_TEST_NUMBER", -1);
                SquirrelFunctions.pushinteger(pVM, 123);
                SquirrelFunctions.newslot(pVM, -3, 0);
                SquirrelFunctions.pop(pVM, 1);

                var list = new Dictionary<string, SquirrelFuncDelegate>(UnregisteredFunction);
                UnregisteredFunction.Clear();
                foreach (var entry in list)
                {
                    SquirrelHelper.RegisterGlobalFunction(entry.Key, entry.Value);
                }
            }
        }
    }
}
