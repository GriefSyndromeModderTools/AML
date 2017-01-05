using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace PluginUtils.Log
{
    class DefaultLogger : ILogger
    {
        private readonly StreamWriter _Writer;

        public DefaultLogger()
        {
            try
            {
                _Writer = new StreamWriter("default.log", true);
            }
            catch
            {
                _Writer = new StreamWriter(new MemoryStream());
            }
            _Writer.WriteLine("Start");
        }

        public void PluginCreated(IAMLPlugin plugin)
        {
            _Writer.WriteLine("Plugin created: {0}", plugin.GetType().FullName);
            _Writer.Flush();
        }

        public void PluginMissDependency(Type plugin, string missedDenpendency, Version version)
        {
            _Writer.WriteLine(
                $"Plugin {plugin.GetCustomAttribute<PluginAttribute>().Name} cannot be loaded: {missedDenpendency}({version}) is missing.");
            _Writer.Flush();
        }

        public void NativeInjectorCreated(Injection.Native.NativeWrapper injector)
        {
            _Writer.WriteLine("Injector created: {0}", injector.GetType().FullName);
            _Writer.Flush();
        }

        public void NativeInjectorInjectedDelegate(IntPtr ptr, Type delegateType)
        {
            _Writer.WriteLine("Injector injected: 0x{0}: {1}", ptr.ToString("X8"), delegateType.FullName);
            _Writer.Flush();
        }

        public void LibraryLoaded(System.Reflection.Assembly a)
        {
            _Writer.WriteLine("Library loaded: {0}", Path.GetFileName(a.CodeBase));
            _Writer.Flush();
        }

        public void System(string desc)
        {
            _Writer.WriteLine("System: {0}", desc);
            _Writer.Flush();
        }
    }
}
