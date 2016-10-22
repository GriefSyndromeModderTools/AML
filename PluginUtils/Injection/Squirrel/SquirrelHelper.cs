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
        private static List<SquirrelFuncDelegate> _DelegateRef = new List<SquirrelFuncDelegate>();

        public static void RegisterGlobalFunction(string name, SquirrelFuncDelegate func)
        {
            var pVM = SquirrelInjectorPlugin.SquirrelVM;
            if (pVM == IntPtr.Zero)
            {
                SquirrelInjectorPlugin.UnregisteredFunction.Add(name, func);
            }
            else
            {
                _DelegateRef.Add(func);
                IntPtr pFunc = Marshal.GetFunctionPointerForDelegate(func);
                SquirrelFunctions.pushroottable(pVM);
                SquirrelFunctions.pushstring(pVM, name, -1);
                SquirrelFunctions.newclosure(pVM, pFunc, 0);
                SquirrelFunctions.newslot(pVM, -3, 0);
                SquirrelFunctions.pop(pVM, 1);
            }
        }
    }
}
