using PluginUtils.Injection.Native;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace EmptyDirectXDelegate
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    class ComMethodAttribute : Attribute
    {
        public int Index { get; private set; }

        public ComMethodAttribute(int index)
        {
            Index = index;
        }
    }

    [AttributeUsage(AttributeTargets.Class)]
    class ComClassAttribute : Attribute
    {
        public int Count { get; private set; }

        public ComClassAttribute(int count)
        {
            Count = count;
        }
    }

    class ComInterfaceGenerator
    {
        public ComInterfaceGenerator(Type type)
        {
            _TypeName = type.FullName;
            _VTab = CreateVTab(type);
            _Inst = Marshal.AllocHGlobal(4);
            Marshal.WriteIntPtr(_Inst, _VTab);
        }

        private readonly string _TypeName;
        private readonly List<Delegate> _Delegates = new List<Delegate>();
        private readonly IntPtr _VTab;
        private readonly IntPtr _Inst;

        public IntPtr VTab { get { return _VTab; } }
        public IntPtr Instance { get { return _Inst; } }

        //TODO merge this function to instance
        private static ConcurrentBag<List<Delegate>> _InjectedDelegates =
            new ConcurrentBag<List<Delegate>>();

        //do a lot of expensive work each call
        //should only use on device/other objects that are only created once
        public static void InjectObject(IntPtr obj, Type type)
        {
            List<Delegate> delegates = new List<Delegate>();
            var vtab = Marshal.ReadIntPtr(obj);
            foreach (var m in type.GetMethods())
            {
                var attrs = m.GetCustomAttributes<ComMethodAttribute>();
                if (attrs.Count() == 0)
                {
                    continue;
                }
                var d = GetDelegateFromMethod(m);
                delegates.Add(d);
                var ptr = AssemblyCodeStorage.WrapManagedDelegate(d);
                foreach (var index in attrs)
                {
                    //Marshal.WriteIntPtr(vtab, 4 * index.Index, ptr);
                    CodeModification.WritePointer(
                        AddressHelper.VirtualTable(obj, index.Index), ptr);
                }
            }
            _InjectedDelegates.Add(delegates);
        }

        private IntPtr CreateVTab(Type type)
        {
            Dictionary<int, IntPtr> table = new Dictionary<int, IntPtr>();
            int count = 0;
            var typeAttr = type.GetCustomAttribute<ComClassAttribute>();
            if (typeAttr != null)
            {
                count = typeAttr.Count;
            }

            foreach (var m in type.GetMethods())
            {
                var attrs = m.GetCustomAttributes<ComMethodAttribute>();
                if (attrs.Count() == 0)
                {
                    continue;
                }
                var d = GetDelegateFromMethod(m);
                _Delegates.Add(d);
                var ptr = AssemblyCodeStorage.WrapManagedDelegate(d);
                foreach (var index in attrs)
                {
                    table[index.Index] = ptr;
                }
            }
            count = Math.Max(table.Max(x => x.Key) + 1, count);
            IntPtr mem = Marshal.AllocHGlobal(count * 4);

            _RaiseErrorDelegate = RaiseError;
            //var f0 = Marshal.GetFunctionPointerForDelegate(_RaiseErrorDelegate);
            for (int i = 0; i < count; ++i)
            {
                IntPtr f;
                if (table.TryGetValue(i, out f))
                {
                    Marshal.WriteIntPtr(mem, i * 4, f);
                }
                else
                {
                    //Marshal.WriteIntPtr(mem, i * 4, f0);
                    Marshal.WriteIntPtr(mem, i * 4, new IntPtr(i));
                }
            }
            return mem;
        }

        private Action _RaiseErrorDelegate;

        //TODO this is not working
        private void RaiseError()
        {
            throw new Exception("COM method not found in " + _TypeName);
        }

        private static Delegate GetDelegateFromMethod(MethodInfo m)
        {
            return Delegate.CreateDelegate(_Factory.CreateDelegateType(m), m);
        }

        private static DelegateTypeFactory _Factory = new DelegateTypeFactory();
    }
}
