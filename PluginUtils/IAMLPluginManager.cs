using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PluginUtils
{
    public interface IAMLPluginManager
    {
        T GetPlugin<T>() where T : IAMLPlugin;
    }
}
