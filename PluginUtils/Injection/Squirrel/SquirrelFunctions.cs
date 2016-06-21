using PluginUtils.Injection.Native;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace PluginUtils.Injection.Squirrel
{
    internal class SquirrelFunctions
    {
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate IntPtr Delegate_I_P(int arg);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void Delegate_P_V(IntPtr arg);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void Delegate_PI_V(IntPtr arg1, int arg2);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void Delegate_PSI_V(IntPtr arg1, [MarshalAs(UnmanagedType.LPStr)]string arg2, int arg3);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate int Delegate_PII_I(IntPtr arg1, int arg2, int arg3);

        public static Delegate_P_V pushroottable;
        public static Delegate_PSI_V pushstring;
        public static Delegate_PI_V pushinteger;
        public static Delegate_PII_I newslot;
        public static Delegate_PI_V pop;

        static SquirrelFunctions()
        {
            pushroottable = GetFunction<Delegate_P_V>(0x12B930);
            pushstring = GetFunction<Delegate_PSI_V>(0x12B740);
            pushinteger = GetFunction<Delegate_PI_V>(0x12B7B0);
            newslot = GetFunction<Delegate_PII_I>(0x12DBC0);
            pop = GetFunction<Delegate_PI_V>(0x12BC90);
        }

        private static T GetFunction<T>(uint offset)
        {
            return (T)(object)Marshal.GetDelegateForFunctionPointer(AddressHelper.CodeOffset(offset), typeof(T));
        }
    }
}
