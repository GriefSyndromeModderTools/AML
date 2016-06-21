using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace PluginUtils.Injection.Native
{
    public delegate void NativeCallback(IntPtr data);

    public class NativeEntrance
    {
        private delegate void EntranceDelegate(int index, IntPtr data);
        private static EntranceDelegate _Delegate;
        private static IntPtr _DelegatePtr;

        static NativeEntrance()
        {
            _Delegate = Entrance;
            _DelegatePtr = Marshal.GetFunctionPointerForDelegate(_Delegate);
        }

        public static IntPtr EntrancePtr
        {
            get
            {
                return _DelegatePtr;
            }
        }

        private static Dictionary<int, NativeCallback> _CallbackList = new Dictionary<int, NativeCallback>();

        private static int _NextIndex;

        public static void Register(int index, NativeCallback cb)
        {
            _CallbackList[index] = cb;
        }

        public static void Unregister(int index)
        {
            _CallbackList.Remove(index);
        }

        public static int NextIndex()
        {
            return _NextIndex++;
        }

        private static void Entrance(int index, IntPtr data)
        {
            NativeCallback cb;
            if (!_CallbackList.TryGetValue(index, out cb))
            {
                return;
            }
            cb(data);
        }
    }
}
