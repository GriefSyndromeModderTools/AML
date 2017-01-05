using PluginUtils.Injection.File;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PluginUtils.Injection.SaveData
{
    [Plugin(DependentPlugin = typeof(PluginUtilsMainPlugin))]
    class SaveDataInjector : IAMLPlugin
    {
        public void Init()
        {
            FileReplacement.RegisterFile(Path.GetFullPath("save/save0.dat"), new SaveDataHelper.SaveFile());
        }

        public void Load()
        {
        }
    }
}
