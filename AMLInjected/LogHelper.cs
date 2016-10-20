using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace AMLInjected
{
    class LogHelper
    {
        public static void LoadedLibrary(Assembly a)
        {
            PluginUtils.Log.LoggerManager.LibraryLoaded(a);
        }

        public static void System(string msg)
        {
            PluginUtils.Log.LoggerManager.System(msg);
        }
    }
}
