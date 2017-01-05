using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PluginUtils
{
    public interface IPluginMetaData
    {
        IEnumerable<string> GetKeys();

        string GetValue(string key);
    }

    public interface IAMLPlugin
    {
        IPluginMetaData GetMetaData();

        //setup yourself, can check other plugins (may not inited)
        void Init();

        //visit other plugins if necessary
        void Load();
    }
}
