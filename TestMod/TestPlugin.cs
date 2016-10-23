using PluginUtils;
using PluginUtils.Injection.Squirrel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace TestMod
{
    public class TestPlugin : IAMLPlugin
    {
        private static IntPtr _FuncObj;

        public void Init()
        {
            SquirrelHelper.InjectCompileFile("data/script/hit.nut", "OnHitActor").AddAfter(vm =>
            {
                if (_FuncObj == IntPtr.Zero)
                {
                    //compile
                    _FuncObj = Marshal.AllocHGlobal(8);
                    SquirrelFunctions.compilebuffer(vm, "return ::gameData.loopNum;", "<my_test>", 0);
                    SquirrelFunctions.getstackobj(vm, -1, _FuncObj);
                    SquirrelFunctions.addref(vm, _FuncObj);
                    SquirrelFunctions.pop(vm, 1);
                }
                SquirrelFunctions.pushobject(vm, _FuncObj);
                SquirrelFunctions.pushroottable(vm);
                SquirrelFunctions.call(vm, 1, 1, 0);
                int lap;
                SquirrelFunctions.getinteger(vm, -1, out lap);
                SquirrelFunctions.pop(vm, 2);
            });
        }

        public void Load()
        {
            throw new NotImplementedException();
        }
    }
}
