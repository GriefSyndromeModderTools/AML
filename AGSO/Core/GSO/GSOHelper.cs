using PluginUtils;
using PluginUtils.Injection.Native;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace AGSO.Core.GSO
{
    class GSOHelper : IAMLPlugin
    {
        public static bool IsGSO { get; private set; }
        public static bool IsGSOLoaded { get; private set; }

        public static event Action GSOLoaded;

        public void Init()
        {
            //check if we are in gso environment
            var check = AddressHelper.CodeOffset(0x1AB4A3);
            if (Marshal.ReadByte(check) == 0xE9)
            {
                IsGSO = true;

                //inject to get a callback when gso library is loaded
                new InjectGSOLoaded().InjectSelf();
            }
        }

        public void Load()
        {
        }

        private class InjectGSOLoaded : NativeWrapper
        {
            public void InjectSelf()
            {
                this.Inject(AddressHelper.CodeOffset(0x1AB336), 7);
            }

            protected override void Triggered(NativeWrapper.NativeEnvironment env)
            {
                IsGSOLoaded = true;
                if (GSOLoaded != null)
                {
                    GSOLoaded();
                }
            }
        }

    }
}
