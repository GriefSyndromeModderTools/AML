using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PluginUtils
{
    public static class PluginLoader
    {
        private sealed class CorePermissionToken : ICorePermissionToken
        {
            private readonly ISet<string> _preventedPlugins = new HashSet<string>();
            private readonly IDictionary<string, Version> _requestedPlugins = new Dictionary<string, Version>();

            public void AddPreventedPlugin(string plugin)
            {
                if (plugin == null)
                {
                    throw new ArgumentNullException(nameof(plugin));
                }

                _preventedPlugins.Add(plugin);
            }

            public void AddRequestedPlugin(string plugin, Version version = null)
            {
                if (plugin == null)
                {
                    throw new ArgumentNullException(nameof(plugin));
                }

                _requestedPlugins.Add(plugin, version);
            }

            public bool CheckPreventedPlugin(string plugin)
            {
                return plugin != null && _preventedPlugins.Contains(plugin);
            }

            public bool CheckRequestedPlugin(string plugin, Version version = null)
            {
                return plugin != null && _requestedPlugins.ContainsKey(plugin) &&
                       (version == null || _requestedPlugins[plugin] == version);
            }

            public void RemovePreventedPlugin(string plugin)
            {
                if (plugin == null)
                {
                    throw new ArgumentNullException(nameof(plugin));
                }

                _preventedPlugins.Remove(plugin);
            }

            public void RemoveRequestedPlugin(string plugin)
            {
                if (plugin == null)
                {
                    throw new ArgumentNullException(nameof(plugin));
                }

                _requestedPlugins.Remove(plugin);
            }

            public IEnumerable<string> GetPreventedPlugins()
            {
                return _preventedPlugins;
            }

            public IEnumerable<KeyValuePair<string, Version>> GetRequestedPlugins()
            {
                return _requestedPlugins;
            }
        }

        private static readonly List<IAMLPlugin> _Plugins = new List<IAMLPlugin>();
        private static readonly CorePermissionToken _corePermissionToken = new CorePermissionToken();

        public static void LoadPlugins(IEnumerable<string> dllFiles)
        {
            var readyToLoadPlugins = new HashSet<Type>();

            foreach (var plugin in typeof(PluginLoader).Assembly.GetTypes()
                .Where(
                    t =>
                        t != null && t.IsClass && !t.IsAbstract && typeof(IAMLPlugin).IsAssignableFrom(t) &&
                        t.GetCustomAttribute<PluginAttribute>() != null))
            {
                readyToLoadPlugins.Add(plugin);
            }

            foreach (var file in dllFiles)
            {
                var lib = Path.GetFileName(file);
                if (lib == "AMLInjected.dll" || lib == "PluginUtils.dll")
                {
                    continue;
                }

                var assembly = Assembly.LoadFile(file);
                var types = assembly.GetTypes();

                var plugins =
                    types.Where(
                        t =>
                            t != null && t.IsClass && !t.IsAbstract && typeof(IAMLPlugin).IsAssignableFrom(t) &&
                            t.GetCustomAttribute<PluginAttribute>() != null);
                foreach (var plugin in plugins)
                {
                    readyToLoadPlugins.Add(plugin);
                }

                Log.LoggerManager.LibraryLoaded(assembly);
            }

            LoadPlugins(readyToLoadPlugins);
        }

        public static void LoadPlugins(IEnumerable<Type> plugins)
        {
            var readyToLoadPlugins = new HashSet<Type>(plugins);
            var loadPluginList = new List<Type>();

            var loadPluginTmpDic = new Dictionary<PluginLoadPriority, List<Type>>();

            var firstTest = true;
            var currentReadyToLoadPlugins = readyToLoadPlugins;

            var readyToLoadPluginsInfo =
                    currentReadyToLoadPlugins.ToDictionary(p => p.GetCustomAttribute<PluginAttribute>().Name,
                        p => p.GetCustomAttribute<PluginAttribute>().Version);

            while (true)
            {
                var loadPluginInfo =
                    new HashSet<string>(
                        loadPluginTmpDic.Values.Select(
                                pl => pl.Select(p => p.GetCustomAttribute<PluginAttribute>().Name))
                            .Aggregate(Enumerable.Empty<string>(), (c, cc) => c.Concat(cc)));

                var nextReadyToLoadPlugins = new HashSet<Type>();
                foreach (var plugin in currentReadyToLoadPlugins)
                {
                    var attr = plugin.GetCustomAttribute<PluginAttribute>();
                    var dependencies = attr.Dependencies;
                    if (firstTest)
                    {
                        var missingDependencies = new HashSet<KeyValuePair<string, Version>>();
                        foreach (
                            var dep in
                            dependencies?.Concat(attr.WeakDependencies) ??
                            attr.WeakDependencies ?? Enumerable.Empty<KeyValuePair<string, Version>>())
                        {
                            Version ver;
                            if (!readyToLoadPluginsInfo.TryGetValue(dep.Key, out ver) || ver < dep.Value)
                            {
                                missingDependencies.Add(dep);
                            }
                        }
                        if (missingDependencies.Count > 0)
                        {
                            foreach (var missingPlugin in missingDependencies)
                            {
                                Log.LoggerManager.PluginMissDependency(plugin, missingPlugin.Key, missingPlugin.Value);
                            }
                            firstTest = false;
                            continue;
                        }
                        firstTest = false;
                    }
                    
                    if (dependencies == null || !dependencies.Select(pair => pair.Key)
                        .Except(loadPluginInfo)
                        .Any())
                    {
                        var priority = plugin.GetCustomAttribute<PluginAttribute>().Priority;
                        List<Type> loadList;
                        if (loadPluginTmpDic.TryGetValue(priority, out loadList))
                        {
                            loadList.Add(plugin);
                        }
                        else
                        {
                            loadPluginTmpDic.Add(priority, new List<Type> { plugin });
                        }
                    }
                    else
                    {
                        nextReadyToLoadPlugins.Add(plugin);
                    }
                }

                if (!currentReadyToLoadPlugins.Except(nextReadyToLoadPlugins).Any())
                {
                    break;
                }

                currentReadyToLoadPlugins = nextReadyToLoadPlugins;
            }

            if (currentReadyToLoadPlugins.Count > 0)
            {
                Log.LoggerManager.System(
                    $"Warning: Cycle dependency detected, these plugins will not be loaded:{currentReadyToLoadPlugins.Aggregate("", (str, plugin) => string.Concat(str, "\n", plugin.GetCustomAttribute<PluginAttribute>().Name))}");
            }

            var keys = loadPluginTmpDic.Keys.ToList();
            keys.Sort();

            foreach (var key in keys)
            {
                loadPluginList.AddRange(loadPluginTmpDic[key]);
            }

            foreach (var pluginType in loadPluginList)
            {
                try
                {
                    LoadPlugin(pluginType);
                }
                catch (Exception e)
                {
                    Log.LoggerManager.System($"Unhandled Exception caught while loading plugin {pluginType.GetCustomAttribute<PluginAttribute>().Name}: {e.Message}");
                }
            }

            FinishLoadPlugin();
        }

        public static void LoadPlugin(Type pluginType)
        {
            if (_corePermissionToken.CheckPreventedPlugin(pluginType.GetCustomAttribute<PluginAttribute>().Name))
            {
                return;
            }

            var constructor = pluginType.GetConstructor(Type.EmptyTypes);
            if (constructor != null)
            {
                LoadPlugin((IAMLPlugin) constructor.Invoke(new object[0]));
            }
        }

        public static void LoadPlugin(IAMLPlugin plugin)
        {
            (plugin as IAMLCorePlugin)?.GrantCorePermission(_corePermissionToken);

            _Plugins.Add(plugin);
            Log.LoggerManager.PluginCreated(plugin);
        }

        public static void FinishLoadPlugin()
        {
            var loadedPluginAttributes =
                _Plugins.Select(p => p.GetType().GetCustomAttribute<PluginAttribute>())
                    .ToDictionary(attr => attr.Name, attr => attr.Version);
            foreach (var requestedPlugin in _corePermissionToken.GetRequestedPlugins())
            {
                Version ver;
                if (!loadedPluginAttributes.TryGetValue(requestedPlugin.Key, out ver) || ver < requestedPlugin.Value)
                {
                    throw new PluginException("Not all requested plugins are loaded.");
                }
            }

            var m = new PluginManager();
            Plugins.Manager = m;
            m.Run();
        }

        private class PluginManager : IAMLPluginManager
        {
            private readonly Dictionary<Type, IAMLPlugin> _Plugins;

            public PluginManager()
            {
                _Plugins = PluginLoader._Plugins.ToDictionary(p => p.GetType());
            }

            public T GetPlugin<T>() where T : IAMLPlugin
            {
                IAMLPlugin p;
                if (_Plugins.TryGetValue(typeof(T), out p))
                {
                    return (T) p;
                }

                throw new KeyNotFoundException($"Plugin {typeof(T).Name} does not loaded.");
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
                        MessageBox.Show(e.ToString());
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
                        MessageBox.Show(e.ToString());
                    }
                }
            }
        }
    }
}
