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
        [StructLayout(LayoutKind.Explicit)]
        public struct SQObjectValue
        {
            [FieldOffset(0)] public IntPtr Pointer;
            [FieldOffset(0)] public int Integer;
            [FieldOffset(0)] public float Float;
        }

        public struct SQObject
        {
            public SquirrelHelper.SQObjectType Type;
            public SQObjectValue Value;
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate IntPtr Delegate_I_P(int arg);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate IntPtr Delegate_PI_P(IntPtr arg1, int arg2);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void Delegate_P_V(IntPtr arg);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate int Delegate_P_I(IntPtr arg);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate IntPtr Delegate_P_P(IntPtr arg);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void Delegate_PP_V(IntPtr arg1, IntPtr arg2);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate int Delegate_PP_I(IntPtr arg1, IntPtr arg2);

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
        public delegate void Delegate_PIP_V(IntPtr arg1, int arg2, IntPtr arg3);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate int Delegate_PIpO_I(IntPtr arg1, int arg2, out SQObject arg3);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void Delegate_PpO_V(IntPtr arg1, ref SQObject arg2);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate int Delegate_PpO_I(IntPtr arg1, ref SQObject arg2);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void Delegate_PII_V(IntPtr arg1, int arg2, int arg3);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void Delegate_PushObject(IntPtr arg1, SquirrelHelper.SQObjectType arg2, SQObjectValue arg3);

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
        public delegate int Delegate_PIoP_I(IntPtr arg1, int arg2, out IntPtr arg3);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate int Delegate_PPISI_I(IntPtr arg1, IntPtr arg2, int arg3,
            [MarshalAs(UnmanagedType.LPStr)]string arg4, int arg5);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate int Delegate_PPP_I(IntPtr arg1, IntPtr arg2, IntPtr arg3);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.LPStr)]
        public delegate string Delegate_PI_S(IntPtr arg1, int arg2);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.LPStr)]
        public delegate string Delegate_PII_S(IntPtr arg1, int arg2, int arg3);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate int Delegate_PS_I(IntPtr arg1, [MarshalAs(UnmanagedType.LPStr)]string arg2);

        public static Delegate_P_V setdebughook = GetFunction<Delegate_P_V>(0x12B630);

        public static Delegate_PI_V enabledebuginfo = GetFunction<Delegate_PI_V>(0x12B6A0);

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
        public static Delegate_PIoP_I getstring_ = GetFunction<Delegate_PIoP_I>(0x12BB90);
        public static Delegate_PIoP_I getuserpointer = GetFunction<Delegate_PIoP_I>(0x12BC40);

        public static Delegate_P_I gettop = GetFunction<Delegate_P_I>(0x12BC80);
        public static Delegate_PI_V push = GetFunction<Delegate_PI_V>(0x12B970);
        public static Delegate_PI_V pop = GetFunction<Delegate_PI_V>(0x12BC90);
        public static Delegate_PI_V remove = GetFunction<Delegate_PI_V>(0x12BCC0);
        public static Delegate_PI_I gettype = GetFunction<Delegate_PI_I>(0x12B9B0);

        public static Delegate_PII_I newslot = GetFunction<Delegate_PII_I>(0x12DBC0);
        public static Delegate_PII_I deleteslot = GetFunction<Delegate_PII_I>(0x12DC80);
        public static Delegate_PI_I next = GetFunction<Delegate_PI_I>(0x12C840);
        public static Delegate_PII_I rawdeleteslot = GetFunction<Delegate_PII_I>(0x12BCE0);
        public static Delegate_PI_I rawget = GetFunction<Delegate_PI_I>(0x12E030);
        public static Delegate_PI_I rawset = GetFunction<Delegate_PI_I>(0x12DD80);

        public static Delegate_PIP_I getstackobj_ = GetFunction<Delegate_PIP_I>(0x12BDA0);
        public static Delegate_PIpO_I getstackobj = GetFunction<Delegate_PIpO_I>(0x12BDA0);
        public static Delegate_PushObject pushobject_ = GetFunction<Delegate_PushObject>(0x12BDF0);
        public static Delegate_PP_V addref = GetFunction<Delegate_PP_V>(0x12B6C0);
        public static Delegate_PpO_V addref_ = GetFunction<Delegate_PpO_V>(0x12B6C0);
        public static Delegate_PP_I release = GetFunction<Delegate_PP_I>(0x12B6F0);
        public static Delegate_PpO_I release_ = GetFunction<Delegate_PpO_I>(0x12B6F0);

        public static Delegate_PPI_V newclosure = GetFunction<Delegate_PPI_V>(0x12EA10);
        public static Delegate_P_V newtable = GetFunction<Delegate_P_V>(0x12B8C0);
        public static Delegate_PI_V newarray = GetFunction<Delegate_PI_V>(0x12E930);

        public static Delegate_PIII_I call = GetFunction<Delegate_PIII_I>(0x12BEF0);
        public static Delegate_PPISI_I compilebuffer_ = GetFunction<Delegate_PPISI_I>(0x12E270);

        public static Delegate_PIP_I setnativeclosurename = GetFunction<Delegate_PIP_I>(0x12D7D0);
        public static Delegate_PIP_I setparamscheck = GetFunction<Delegate_PIP_I>(0x12F020);
        public static Delegate_PI_P newthread = GetFunction<Delegate_PI_P>(0x12B4F0);
        public static Delegate_P_V seterrorhandler = GetFunction<Delegate_P_V>(0x12B5C0);
        public static Delegate_PI_V tostring = GetFunction<Delegate_PI_V>(0x12B9E0);
        public static Delegate_PIP_I getthread = GetFunction<Delegate_PIP_I>(0x12BBE0);
        public static Delegate_P_V poptop = GetFunction<Delegate_P_V>(0x12BCB0);
        public static Delegate_P_V resetobject = GetFunction<Delegate_P_V>(0x12BE40);
        public static Delegate_PS_I throwerror = GetFunction<Delegate_PS_I>(0x12BE60);
        public static Delegate_P_I suspendvm = GetFunction<Delegate_P_I>(0x12BFB0);
        public static Delegate_PIII_I wakeupvm = GetFunction<Delegate_PIII_I>(0x12BFC0);
        public static Delegate_PIP_V setreleasehook = GetFunction<Delegate_PIP_V>(0x12C140);
        public static Delegate_PP_V setcompilererrorhandler = GetFunction<Delegate_PP_V>(0x12C1B0);
        public static Delegate_PPP_I writeclosure = GetFunction<Delegate_PPP_I>(0x12C1D0);
        public static Delegate_PPP_I readclosure = GetFunction<Delegate_PPP_I>(0x12C260);
        public static Delegate_PI_S getscratchpad = GetFunction<Delegate_PI_S>(0x12C370);
        public static Delegate_P_I collectgarbage = GetFunction<Delegate_P_I>(0x12C390);
        public static Delegate_PI_I setattributes = GetFunction<Delegate_PI_I>(0x12C3B0);
        public static Delegate_PI_I getattributes = GetFunction<Delegate_PI_I>(0x12C530);
        public static Delegate_PI_I getclass = GetFunction<Delegate_PI_I>(0x12C620);
        public static Delegate_PI_I createinstance = GetFunction<Delegate_PI_I>(0x12C6A0);
        public static Delegate_PI_V weakref = GetFunction<Delegate_PI_V>(0x12C720);
        public static Delegate_PI_I getweakrefval = GetFunction<Delegate_PI_I>(0x12C7B0);
        public static Delegate_PPI_V move = GetFunction<Delegate_PPI_V>(0x12CA80);
        public static Delegate_PP_V setprintfunc = GetFunction<Delegate_PP_V>(0x12CAC0);
        public static Delegate_P_P getprintfunc = GetFunction<Delegate_P_P>(0x12CAE0);
        public static Delegate_PI_I getsize = GetFunction<Delegate_PI_I>(0x12D9C0);
        public static Delegate_PI_I setdelegate = GetFunction<Delegate_PI_I>(0x12DEE0);
        public static Delegate_PI_I get = GetFunction<Delegate_PI_I>(0x12DFC0);
        public static Delegate_PII_S getlocal = GetFunction<Delegate_PII_S>(0x12E160);
        public static Delegate_PIP_I stackinfos = GetFunction<Delegate_PIP_I>(0x13ABE0);

        private static T GetFunction<T>(uint offset)
        {
            return (T)(object)Marshal.GetDelegateForFunctionPointer(AddressHelper.CodeOffset(offset), typeof(T));
        }

        public static int getstring(IntPtr vm, int stack, out string str)
        {
            IntPtr s;
            var ret = getstring_(vm, stack, out s);
            str = Marshal.PtrToStringAnsi(s);
            return ret;
        }

        //original function require a 8 byte object
        //this function use the pointer of the object and send it to original function
        public static void pushobject(IntPtr vm, IntPtr pObj)
        {
            var obj = (SQObject) Marshal.PtrToStructure(pObj, typeof(SQObject));
            pushobject_(vm, obj.Type, obj.Value);
        }

        [DllImport("kernel32.dll", CharSet = CharSet.Ansi, ExactSpelling = true)]
        internal static extern int lstrlenA(IntPtr ptr);

        public static int compilebuffer(IntPtr vm, string str, string buffername, int raiseerror)
        {
            var buffer = Marshal.StringToHGlobalAnsi(str);
            var ret = compilebuffer_(vm, buffer, lstrlenA(buffer) + 1, buffername, raiseerror);
            Marshal.FreeHGlobal(buffer);
            return ret;
        }
    }
}
