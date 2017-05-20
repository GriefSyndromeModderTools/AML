using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace AMLInjected
{
    internal static class Helper
    {
        public static void LoadPlugins(IEnumerable<string> dllFiles)
        {
            PluginUtils.PluginLoader.LoadPlugins(dllFiles);
        }

        public static void LogSystem(string msg)
        {
            PluginUtils.Log.LoggerManager.System(msg);
        }

        public static void SetupArgs(IntPtr ptr)
        {
            PluginUtils.ArgHelper.Set(ptr);
        }
    }
}
