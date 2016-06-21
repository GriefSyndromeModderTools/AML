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
        public void Init()
        {
            new InjectSquirrelVM().InjectSelf();
            MessageBox.Show("squirrel injected");
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
                SquirrelFunctions.pushroottable(pVM);
                SquirrelFunctions.pushstring(pVM, "MY_TEST_NUMBER", -1);
                SquirrelFunctions.pushinteger(pVM, 123);
                SquirrelFunctions.newslot(pVM, -3, 0);
                SquirrelFunctions.pop(pVM, 1);
            }
        }
    }
}
