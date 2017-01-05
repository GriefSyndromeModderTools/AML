using PluginUtils;
using PluginUtils.Injection.Native;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AGSO.Misc
{
    [Plugin(DependentPlugin = typeof(AGSOMainPlugin))]
    class MultipleProcessFix : IAMLPlugin
    {
        public void Init()
        {
            CodeModification.Modify(0x9F633, 0xEB);
        }

        public void Load()
        {
        }
    }
}
