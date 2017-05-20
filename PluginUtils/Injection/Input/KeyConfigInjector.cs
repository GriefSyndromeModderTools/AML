using PluginUtils.Injection.File;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PluginUtils.Injection.Input
{
    public class KeyConfigInjector
    {
        private static bool _Injected;
        private static int[] _KeyConfig = new int[9 * 3];
        private static int[] _KeyConfigOriginal = new int[9 * 3];

        public static void Inject()
        {
            if (_Injected)
            {
                return;
            }
            _Injected = true;

            CalcKeyCodeList();
            FileReplacement.RegisterFile(Path.GetFullPath("keyconfig.dat"),
                new KeyConfigFile { KeyConfig = _KeyConfig, KeyConfigOriginal = _KeyConfigOriginal });
        }

        private static void CalcKeyCodeList()
        {
            for (int i = 0; i < _KeyConfig.Length; ++i)
            {
                _KeyConfig[i] = i + 30;
            }
        }

        public static int GetInjectedKeyIndex(int key)
        {
            return _KeyConfig[key];
        }

        public static int GetOriginalKeyIndex(int key)
        {
            return _KeyConfigOriginal[key];
        }

        private class KeyConfigFile : CachedModificationFileProxyFactory
        {
            public int[] KeyConfig;
            public int[] KeyConfigOriginal;

            public override byte[] Modify(byte[] data)
            {
                Buffer.BlockCopy(data, 0, KeyConfigOriginal, 0, 9 * 3 * 4);
                Buffer.BlockCopy(KeyConfig, 0, data, 0, 9 * 3 * 4);
                return data;
            }
        }
    }
}
