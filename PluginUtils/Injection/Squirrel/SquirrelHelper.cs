using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace PluginUtils.Injection.Squirrel
{
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate int SquirrelFuncDelegate(IntPtr vm);

    public class SquirrelHelper
    {
        public static IntPtr SquirrelVM { get { return SquirrelInjectorPlugin.SquirrelVM; } }

        //prevent GC collecting
        private static List<SquirrelFuncDelegate> _DelegateRef = new List<SquirrelFuncDelegate>();

        public static void Run(Action<IntPtr> action)
        {
            var vm = SquirrelVM;
            if (vm == IntPtr.Zero)
            {
                SquirrelInjectorPlugin.OnSquirrelCreated += action;
            }
            else
            {
                action(vm);
            }
        }

        public static void RegisterGlobalFunction(string name, SquirrelFuncDelegate func)
        {
            Run(vm =>
            {
                _DelegateRef.Add(func);
                IntPtr pFunc = Marshal.GetFunctionPointerForDelegate(func);
                SquirrelFunctions.pushroottable(vm);
                SquirrelFunctions.pushstring(vm, name, -1);
                SquirrelFunctions.newclosure(vm, pFunc, 0);
                SquirrelFunctions.newslot(vm, -3, 0);
                SquirrelFunctions.pop(vm, 1);
            });
        }

        public static InjectedScriptFunction InjectCompileFile(string script, string func)
        {
            return CompileFileInjectionManager.InjectCompileFile(script, func);
        }

        public static IntPtr CompileScriptFunction(string code, string name)
        {
            var ret = Marshal.AllocHGlobal(8);

            //write null
            Marshal.WriteInt32(ret, 0x01000001);
            Marshal.WriteInt32(ret, 4, 0);

            Run(vm =>
            {
                if (SquirrelFunctions.compilebuffer(vm, code, name, 0) == 0)
                {
                    SquirrelFunctions.getstackobj(vm, -1, ret);
                    SquirrelFunctions.addref(vm, ret);
                    SquirrelFunctions.pop(vm, 1);
                }
            });

            return ret;
        }
    }
}
