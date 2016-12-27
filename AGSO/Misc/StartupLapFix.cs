using PluginUtils;
using PluginUtils.Injection.File;
using PluginUtils.Injection.SaveData;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AGSO.Misc
{
    class StartupLapFix : IAMLPlugin
    {
        public void Init()
        {
            SaveDataHelper.ModifySaveData += delegate(GSDataFile.CompoundType obj)
            {
                obj["lastPlayLap"] = 0;
                obj["loopNum"] = 677;
            };
        }

        public void Load()
        {
        }
    }
}
