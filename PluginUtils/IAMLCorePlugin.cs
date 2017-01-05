using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PluginUtils
{
    public interface ICorePermissionToken
    {
        void AddPreventedPlugin(string plugin);

        void AddRequestedPlugin(string plugin, Version version = null);

        bool CheckPreventedPlugin(string plugin);

        bool CheckRequestedPlugin(string plugin, Version version = null);

        void RemovePreventedPlugin(string plugin);

        void RemoveRequestedPlugin(string plugin);

        IEnumerable<string> GetPreventedPlugins();

        IEnumerable<KeyValuePair<string, Version>> GetRequestedPlugins();
    }

    public interface IAMLCorePlugin : IAMLPlugin
    {
        void GrantCorePermission(ICorePermissionToken corePermissionToken);
    }
}
