using PluginUtils;
using PluginUtils.Injection.File;
using PluginUtils.Injection.Squirrel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace TestMod
{
    public class TestPlugin : IAMLPlugin
    {
        public void Init()
        {
            var func = SquirrelHelper.CompileScriptFunction("return ::gameData.loopNum;", "<my_test>");
            SquirrelHelper.InjectCompileFile("data/script/hit.nut", "OnHitActor").AddAfter(vm =>
            {
                int lap;
                SquirrelFunctions.pushobject(vm, func);
                SquirrelFunctions.pushroottable(vm);
                SquirrelFunctions.call(vm, 1, 1, 0);
                SquirrelFunctions.getinteger(vm, -1, out lap);
                SquirrelFunctions.pop(vm, 2);
                //use lap
            });
        }

        public void Load()
        {
        }
    }
}
