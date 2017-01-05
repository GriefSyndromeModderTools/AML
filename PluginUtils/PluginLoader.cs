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
                    throw new ArgumentNullException("plugin");
                }

                _preventedPlugins.Add(plugin);
            }

            public void AddRequestedPlugin(string plugin, Version version = null)
            {
                if (plugin == null)
                {
                    throw new ArgumentNullException("plugin");
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
                    throw new ArgumentNullException("plugin");
                }

                _preventedPlugins.Remove(plugin);
            }

            public void RemoveRequestedPlugin(string plugin)
            {
                if (plugin == null)
                {
                    throw new ArgumentNullException("plugin");
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
                    currentReadyToLoadPlugins.ToDictionary(p => GetPluginAttribute(p).Name,
                        p => GetPluginAttribute(p).Version);

            while (true)
            {
                var loadPluginInfo =
                    new HashSet<string>(
                        loadPluginTmpDic.Values.Select(
                                pl => pl.Select(p => GetPluginAttribute(p).Name))
                            .Aggregate(Enumerable.Empty<string>(), (c, cc) => c.Concat(cc)));

                var nextReadyToLoadPlugins = new HashSet<Type>();
                foreach (var plugin in currentReadyToLoadPlugins)
                {
                    var attr = GetPluginAttribute(plugin);
                    var dependencies = attr.Dependencies;
                    if (firstTest)
                    {
                        var missingDependencies = new HashSet<KeyValuePair<string, Version>>();

                        foreach (var dep in dependencies)
                        {
                            Version ver;
                            if (!readyToLoadPluginsInfo.TryGetValue(dep.Key, out ver) || ver < dep.Value)
                            {
                                missingDependencies.Add(dep);
                            }
                        }
                        foreach (var dep in attr.WeakDependencies)
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
                    
                    if (!dependencies.Select(pair => pair.Key).Except(loadPluginInfo).Any())
                    {
                        var priority = GetPluginAttribute(plugin).Priority;
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
                Log.LoggerManager.System(String.Format(
                    "Warning: Cycle dependency detected, these plugins will not be loaded:{0}",
                    currentReadyToLoadPlugins.Aggregate("", (str, plugin) => String.Concat(str, "\n", GetPluginAttribute(plugin).Name))));
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
                    Log.LoggerManager.System(String.Format("Unhandled Exception caught while loading plugin {0}: {1}",
                        GetPluginAttribute(pluginType).Name, e.Message));
                }
            }

            FinishLoadPlugin();
        }

        public static void LoadPlugin(Type pluginType)
        {
            if (_corePermissionToken.CheckPreventedPlugin(GetPluginAttribute(pluginType).Name))
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
            if (plugin is IAMLCorePlugin)
            {
                ((IAMLCorePlugin)plugin).GrantCorePermission(_corePermissionToken);
            }

            _Plugins.Add(plugin);
            Log.LoggerManager.PluginCreated(plugin);
        }

        public static void FinishLoadPlugin()
        {
            var loadedPluginAttributes =
                _Plugins.Select(p => GetPluginAttribute(p.GetType()))
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

        private static Dictionary<Type, PluginAttribute> _PluginAttrCache = new Dictionary<Type, PluginAttribute>();

        private static PluginAttribute GetPluginAttribute(Type obj)
        {
            PluginAttribute ret;
            if (!_PluginAttrCache.TryGetValue(obj, out ret))
            {
                var raw = obj.GetCustomAttribute<PluginAttribute>();
                ret = new PluginAttribute();

                ret.Dependencies = raw.Dependencies;
                if (ret.Dependencies == null)
                {
                    ret.Dependencies = new Dictionary<string, Version>();
                }

                ret.DependentPlugin = raw.DependentPlugin;

                ret.Name = raw.Name;
                if (ret.Name == null)
                {
                    ret.Name = obj.Name;
                }

                ret.Priority = raw.Priority;

                ret.RawVersion = raw.RawVersion;

                ret.WeakDependencies = raw.WeakDependencies;
                if (ret.WeakDependencies == null)
                {
                    ret.WeakDependencies = new Dictionary<string, Version>();
                }

                _PluginAttrCache.Add(obj, ret);
            }
            return ret;
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

                throw new KeyNotFoundException(String.Format("Plugin {} does not loaded.", typeof(T).Name));
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
