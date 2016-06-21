//using PluginUtils;
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
    public class AMLInjectedCore
    {
        [DllExport("loadcore")]
        public static uint LoadCore(IntPtr ud)
        {
            System.Windows.Forms.MessageBox.Show("core loaded");

            var codeBase = typeof(AMLInjectedCore).Assembly.CodeBase;
            var uri = new UriBuilder(codeBase);
            var path = Uri.UnescapeDataString(uri.Path);
            var dir = Path.GetDirectoryName(path);
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
                    PluginLoader.LoadPlugin(plugin.GetConstructor(Type.EmptyTypes).Invoke(new object[0]));
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
                MessageBox.Show("Loading " + file);

                try
                {
                    var libname = Path.GetFileNameWithoutExtension(lib);
                    var assembly = Assembly.LoadFile(file);
                    var types = assembly.GetTypes();

                    var plugins = types.Where(t => pluginBase.IsAssignableFrom(t));
                    foreach (var plugin in plugins)
                    {
                        PluginLoader.LoadPlugin(plugin.GetConstructor(Type.EmptyTypes).Invoke(new object[0]));
                    }

                    MessageBox.Show("Loaded " + file);
                }
                catch (Exception e)
                {
                    MessageBox.Show(e.ToString());
                }
            }

            PluginLoader.FinishLoadPlugin();

            MessageBox.Show("core loaded");
            return 0;
        }
    }
}
