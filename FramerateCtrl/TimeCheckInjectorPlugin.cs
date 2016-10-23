using PluginUtils;
using PluginUtils.Injection.Native;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FramerateCtrl
{
    class TimeCheckInjectorPlugin : IAMLPlugin
    {
        public void Init()
        {
            //disable time check at the startup
            //this value will be write to 0x2AC51C and if later controlling is needed 
            //0x2AC51C must be modified
            CodeModification.Modify(0xD252B + 6, 0, 0, 0, 0);

            //show fps on title (very small effect on speed)
            PluginUtils.Injection.Direct3D.Direct3DHelper.InjectDevice(d =>
            {
                CodeModification.Modify(0x2AC11E, 1);
            });

            CodeModification.FillNop(0xC613A, 0xC61BD - 0xC613A); //remove render
            CodeModification.Modify(0x58230, 0x33, 0xC0, 0xC3); //PlaySE
            CodeModification.Modify(0xF1CC0, 0x33, 0xC0, 0xC3); //BitBlt
            CodeModification.Modify(0xF1F00, 0x33, 0xC0, 0xC3); //StretchBlt
        }

        public void Load()
        {
        }
    }
}
