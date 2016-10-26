using RGiesecke.DllExport;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AMLInjected
{
    //Be careful when modifying this class.
    //It CANNOT directly reference class in PluginUtils project, or loading will fail.
    public class AMLInjectedCore
    {
        [DllExport("loadcore")]
        public static uint LoadCore(IntPtr ud)
        {
            var uri = new UriBuilder(typeof(AMLInjectedCore).Assembly.CodeBase).Path;
            var dir = Path.GetDirectoryName(Uri.UnescapeDataString(uri));
            var dllFiles = Directory.EnumerateFiles(Path.Combine(dir, "../mods"), "*.dll",
                SearchOption.TopDirectoryOnly);
            
            AppDomain.CurrentDomain.AppendPrivatePath("aml/core");
            AppDomain.CurrentDomain.AppendPrivatePath("aml/mods");

            var utilsAssembly = Assembly.LoadFile(Path.Combine(dir, "PluginUtils.dll"));
            var pluginBase = utilsAssembly.GetType("PluginUtils.IAMLPlugin");

            foreach (var plugin in utilsAssembly.GetTypes()
                .Where(t => pluginBase.IsAssignableFrom(t)))
            {
                if (!plugin.IsClass || plugin.IsAbstract)
                {
                    continue;
                }
                try
                {
                    var constructorInfo = plugin.GetConstructor(Type.EmptyTypes);
                    if (constructorInfo != null)
                        PluginLoader.LoadPlugin(constructorInfo.Invoke(new object[0]));
                }
                catch
                {
                }
            }

            foreach (var file in dllFiles)
            {
                var lib = Path.GetFileName(file);
                if (lib == "AMLInjected.dll" || lib == "PluginUtils.dll")
                {
                    continue;
                }

                try
                {
                    var assembly = Assembly.LoadFile(file);
                    var types = assembly.GetTypes();

                    var plugins = types.Where(t => pluginBase.IsAssignableFrom(t));
                    foreach (var plugin in plugins)
                    {
                        var constructorInfo = plugin.GetConstructor(Type.EmptyTypes);
                        if (constructorInfo != null)
                            PluginLoader.LoadPlugin(constructorInfo.Invoke(new object[0]));
                    }

                    LogHelper.LoadedLibrary(assembly);
                }
                catch (Exception e)
                {
                    MessageBox.Show(e.ToString());
                }
            }

            PluginLoader.FinishLoadPlugin();

            LogHelper.System("Core loaded");
            return 0;
        }
    }
}
