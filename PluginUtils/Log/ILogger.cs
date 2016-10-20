using PluginUtils.Injection.Native;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace PluginUtils.Log
{
    public interface ILogger
    {
        void PluginCreated(IAMLPlugin plugin);

        //note that at the time this function is called, the object is
        //only going through base class init
        void NativeInjectorCreated(NativeWrapper injector);

        void NativeInjectorInjectedDelegate(IntPtr ptr, Type delegateType);

        void LibraryLoaded(Assembly a);

        void System(string desc);
    }
}
