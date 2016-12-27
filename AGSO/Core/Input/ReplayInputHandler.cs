using PluginUtils;
using PluginUtils.Injection.File;
using PluginUtils.Injection.Input;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace AGSO.Core.Input
{
    class ReplayInputHandler : IAMLPlugin, IInputHandler
    {
        private static ushort[] _Rep;
        private int[] _KeyConfig;
        private static int _RepOffset;

        private static readonly ushort[] _Mask = new ushort[] {
            0x01, 0x02, 0x04, 0x08, 0x10, 0x20, 0x40, 0x80, 0x100
        };

        public void Init()
        {
            WindowsHelper.RunAndWait(delegate()
            {
                try
                {
                    var dialog = new System.Windows.Forms.OpenFileDialog();
                    if (dialog.ShowDialog() != System.Windows.Forms.DialogResult.Cancel)
                    {
                        var rep = new Misc.GSO2ReplayFile(dialog.FileName);
                        _Rep = rep.InputData;

                        _KeyConfig = GetKeyCodeList();
                        FileReplacement.RegisterFile(Path.GetFullPath("keyconfig.dat"),
                            new KeyConfigFile { KeyConfig = _KeyConfig });

                        InputManager.RegisterHandler(this);
                    }
                }
                catch
                {
                    System.Windows.Forms.MessageBox.Show("Replay error");
                }
            });
        }

        public void Load()
        {
        }

        public bool HandleInput(IntPtr ptr)
        {
            if (_Rep == null)
            {
                return false;
            }
            if (_RepOffset + 2 >= _Rep.Length)
            {
                _Rep = null;
                return false;
            }
            for (int p = 0; p < 3; p++)
            {
                int playerOffset = _RepOffset + p;
                var pp = p;
                for (int k = 0; k < 9; ++k)
                {
                    var dik = _KeyConfig[pp * 9 + k];
                    if ((_Rep[playerOffset] & _Mask[k]) != 0)
                    {
                        Marshal.WriteByte(ptr, dik, 0x80);
                    }
                }
            }

            _RepOffset += 3;
            return true;
        }

        private static int[] GetKeyCodeList()
        {
            var ret = new int[9 * 3];
            for (int i = 0; i < ret.Length; ++i)
            {
                ret[i] = i + 30;
            }
            return ret;
        }

        private class KeyConfigFile : CachedModificationFileProxyFactory
        {
            public int[] KeyConfig;

            public override byte[] Modify(byte[] data)
            {
                Buffer.BlockCopy(KeyConfig, 0, data, 0, 9 * 3 * 4);
                return data;
            }
        }
    }
}
