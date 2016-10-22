using PluginUtils.Injection.Native;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace PluginUtils.Injection.Squirrel
{
    public class SquirrelFunctions
    {
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate IntPtr Delegate_I_P(int arg);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void Delegate_P_V(IntPtr arg);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate int Delegate_P_I(IntPtr arg);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void Delegate_PP_V(IntPtr arg1, IntPtr arg2);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void Delegate_PI_V(IntPtr arg1, int arg2);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate int Delegate_PI_I(IntPtr arg1, int arg2);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void Delegate_PSI_V(IntPtr arg1, [MarshalAs(UnmanagedType.LPStr)]string arg2, int arg3);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate int Delegate_PII_I(IntPtr arg1, int arg2, int arg3);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate int Delegate_PIP_I(IntPtr arg1, int arg2, IntPtr arg3);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void Delegate_PPI_V(IntPtr arg1, IntPtr arg2, int arg3);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void Delegate_PF_V(IntPtr arg1, float arg2);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate int Delegate_PIII_I(IntPtr arg1, int arg2, int arg3, int arg4);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate int Delegate_PIoI_I(IntPtr arg1, int arg2, out int arg3);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate int Delegate_PIoF_I(IntPtr arg1, int arg2, out float arg3);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate int Delegate_PIoS_I(IntPtr arg1, int arg2, [MarshalAs(UnmanagedType.LPStr)]out string arg3);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate int Delegate_PIoP_I(IntPtr arg1, int arg2, out IntPtr arg3);

        public static Delegate_P_V pushroottable = GetFunction<Delegate_P_V>(0x12B930);
        public static Delegate_PSI_V pushstring = GetFunction<Delegate_PSI_V>(0x12B740);
        public static Delegate_PI_V pushinteger = GetFunction<Delegate_PI_V>(0x12B7B0);
        public static Delegate_PF_V pushfloat = GetFunction<Delegate_PF_V>(0x12B840);
        public static Delegate_P_V pushnull = GetFunction<Delegate_P_V>(0x12B720);
        public static Delegate_PI_V pushbool = GetFunction<Delegate_PI_V>(0x12B7F0);
        public static Delegate_PP_V pushuserpointer = GetFunction<Delegate_PP_V>(0x12B880);

        public static Delegate_PIoI_I getinteger = GetFunction<Delegate_PIoI_I>(0x12BA90);
        public static Delegate_PIoF_I getfloat = GetFunction<Delegate_PIoF_I>(0x12BAF0);
        public static Delegate_PIoI_I getbool = GetFunction<Delegate_PIoI_I>(0x12BB50);
        public static Delegate_PIoS_I getstring = GetFunction<Delegate_PIoS_I>(0x12BB90);
        public static Delegate_PIoP_I getuserpointer = GetFunction<Delegate_PIoP_I>(0x12BC40);

        public static Delegate_P_I gettop = GetFunction<Delegate_P_I>(0x12BC80);
        public static Delegate_PI_V push = GetFunction<Delegate_PI_V>(0x12B970);
        public static Delegate_PI_V pop = GetFunction<Delegate_PI_V>(0x12BC90);
        public static Delegate_PI_V remove = GetFunction<Delegate_PI_V>(0x12BCC0);
        public static Delegate_PI_I gettype = GetFunction<Delegate_PI_I>(0x12B9B0);

        public static Delegate_PII_I newslot = GetFunction<Delegate_PII_I>(0x12DBC0);
        public static Delegate_PII_I deleteslot = GetFunction<Delegate_PII_I>(0x12DC80);

        public static Delegate_PPI_V newclosure = GetFunction<Delegate_PPI_V>(0x12EA10);
        public static Delegate_P_V newtable = GetFunction<Delegate_P_V>(0x12B8C0);
        public static Delegate_PI_V newarray = GetFunction<Delegate_PI_V>(0x12E930);

        public static Delegate_PIII_I call = GetFunction<Delegate_PIII_I>(0x12BEF0);

        private static T GetFunction<T>(uint offset)
        {
            return (T)(object)Marshal.GetDelegateForFunctionPointer(AddressHelper.CodeOffset(offset), typeof(T));
        }
    }
}
