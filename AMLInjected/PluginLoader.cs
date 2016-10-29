using PluginUtils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AMLInjected
{
    class PluginLoader
    {
        private static List<IAMLPlugin> _Plugins = new List<IAMLPlugin>();

        public static void LoadPlugin(object plugin)
        {
            IAMLPlugin cp = (IAMLPlugin)plugin;
            _Plugins.Add(cp);
            PluginUtils.Log.LoggerManager.PluginCreated(cp);
        }

        public static void FinishLoadPlugin()
        {
            var m = new PluginManager();
            Plugins.Manager = m;
            m.Run();
        }

        private class PluginManager : IAMLPluginManager
        {
            private Dictionary<Type, IAMLPlugin> _Plugins;

            public PluginManager()
            {
                _Plugins = PluginLoader._Plugins.ToDictionary(p => p.GetType());
            }

            public T GetPlugin<T>() where T : IAMLPlugin
            {
                IAMLPlugin p;
                if (_Plugins.TryGetValue(typeof(T), out p))
                {
                    return (T)p;
                }
                return default(T);
            }

            public void Run()
            {
                //TODO use plugin container
                List<Type> removedPlugin = new List<Type>();
                foreach (var p in _Plugins.Values)
                {
                    try
                    {
                        p.Init();
                    }
                    catch (Exception e)
                    {
                        System.Windows.Forms.MessageBox.Show(e.ToString());
                        removedPlugin.Add(p.GetType());
                    }
                }
                foreach (var p in _Plugins)
                {
                    if (removedPlugin.Contains(p.Key))
                    {
                        continue;
                    }
                    try
                    {
                        p.Value.Load();
                    }
                    catch (Exception e)
                    {
                        System.Windows.Forms.MessageBox.Show(e.ToString());
                    }
                }
            }
        }
    }
}
