using PluginUtils.Injection.Native;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace PluginUtils.Log
{
    public class LoggerManager
    {
        private static List<ILogger> _Loggers = new List<ILogger>();

        static LoggerManager()
        {
            AddLogger(new DefaultLogger());
        }

        public static void AddLogger(ILogger logger)
        {
            _Loggers.Add(logger);
        }

        public static void PluginCreated(IAMLPlugin plugin)
        {
            _Loggers.ForEach(l => l.PluginCreated(plugin));
        }

        public static void PluginMissDependency(Type plugin, string missedDenpendency, Version version)
        {
            _Loggers.ForEach(l => l.PluginMissDependency(plugin, missedDenpendency, version));
        }

        public static void NativeInjectorCreated(NativeWrapper injector)
        {
            _Loggers.ForEach(l => l.NativeInjectorCreated(injector));
        }

        public static void NativeInjectorInjectedDelegate(IntPtr ptr, Type delegateType)
        {
            _Loggers.ForEach(l => l.NativeInjectorInjectedDelegate(ptr, delegateType));
        }

        public static void LibraryLoaded(Assembly a)
        {
            _Loggers.ForEach(l => l.LibraryLoaded(a));
        }

        public static void System(string desc)
        {
            _Loggers.ForEach(l => l.System(desc));
        }
    }
}
