using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace PluginUtils.Injection.Squirrel
{
    class CompileFileInjectionManager
    {
        private static Dictionary<string, Dictionary<string, int>> _FunctionDict =
            new Dictionary<string, Dictionary<string, int>>();
        private static List<InjectedScriptFunction> _FunctionList = new List<InjectedScriptFunction>();

        public static void AfterCompileFile(string file, ref SquirrelFunctions.SQObject table)
        {
            var vm = SquirrelInjectorPlugin.SquirrelVM;
            Dictionary<string, int> script;
            if (_FunctionDict.TryGetValue(file, out script))
            {
                SquirrelFunctions.pushobject_(vm, table.Type, table.Pointer);
                ProcessTable(vm, script);
                SquirrelFunctions.pop(vm, 1);
            }
        }

        private static void ProcessTable(IntPtr vm, Dictionary<string, int> functions)
        {
            foreach (var entry in functions)
            {
                if (CheckKey(vm, entry.Key))
                {
                    ProcessFunction(vm, entry.Key, entry.Value);
                }
            }
        }

        private static bool CheckKey(IntPtr vm, string key)
        {
            SquirrelFunctions.pushstring(vm, key, -1);
            return SquirrelFunctions.rawget(vm, -2) == 0;
            //TODO should check if the value is a function, or ProcessFunction may fail
        }

        private static void ProcessFunction(IntPtr vm, string key, int index)
        {
            //table func

            SquirrelFunctions.pushinteger(vm, index); //table func index
            SquirrelFunctions.newclosure(vm, Marshal.GetFunctionPointerForDelegate(_InjectedEntrance), 2);
            //table func_new

            //save the function into table
            SquirrelFunctions.pushstring(vm, key, -1);//table func_new key
            SquirrelFunctions.push(vm, -2);//table func_new key func_new
            SquirrelFunctions.newslot(vm, -4, 0);//table func_new
            SquirrelFunctions.pop(vm, 1);//table
        }

        //save the entrance
        private static SquirrelFuncDelegate _InjectedEntrance = InjectedEntrance;

        public static InjectedScriptFunction InjectCompileFile(string script, string func)
        {
            Dictionary<string, int> s;
            if (!_FunctionDict.TryGetValue(script, out s))
            {
                s = new Dictionary<string, int>();
                _FunctionDict.Add(script, s);
            }
            int index;
            if (!s.TryGetValue(func, out index))
            {
                index = _FunctionList.Count;
                s.Add(func, index);
                var ret = new InjectedScriptFunction();
                _FunctionList.Add(ret);
                return ret;
            }
            return _FunctionList[index];
        }

        //2 free variables: original, index
        private static int InjectedEntrance(IntPtr vm)
        {
            int index;
            if (SquirrelFunctions.getinteger(vm, -2, out index) != 0)
            {
                return -1;
            }
            if (index < 0 || index >= _FunctionList.Count)
            {
                return -1;
            }

            SquirrelFunctions.SQObject obj;
            SquirrelFunctions.getstackobj(vm, -1, out obj);

            //pop the two free vars
            SquirrelFunctions.pop(vm, 2);

            var f = _FunctionList[index];
            return f.Invoke(vm, SquirrelFunctions.gettop(vm), obj.Type, obj.Pointer);
        }
    }
}
