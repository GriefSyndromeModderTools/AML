using PluginUtils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace AGSO.Core.Common
{
    class InputHandler
    {
        private static ushort[] _Rep;
        private static int _RepOffset = 3 * 0;
        private static bool _FPRunFlag = false;

        static InputHandler()
        {
            _Rep = new ushort[0];
            try
            {
                var dialog = new System.Windows.Forms.OpenFileDialog();
                if (dialog.ShowDialog() != System.Windows.Forms.DialogResult.Cancel)
                {
                    AGSO.Misc.GSO2ReplayFile rep = new Misc.GSO2ReplayFile(dialog.FileName);
                    _Rep = rep.InputData;
                }
            }
            catch
            {
                System.Windows.Forms.MessageBox.Show("Replay error");
            }

            var keyconfigData = System.IO.File.ReadAllBytes(PathHelper.GetPath("keyconfig.dat"));
            _KeyConfig = new int[9 * 3];
            Buffer.BlockCopy(keyconfigData, 0, _KeyConfig, 0, 9 * 3 * 4);
        }

        private static int[] _KeyConfig;

        private static readonly ushort[] _Mask = new ushort[] {
            0x01, 0x02, 0x04, 0x08, 0x10, 0x20, 0x40, 0x80, 0x100
        };

        public static void Aquire(IntPtr data)
        {
            if (!_FPRunFlag)
            {
                _FPRunFlag = true;
                AGSO.Core.FP.FPCode.Run();
            }
            if (_RepOffset + 2 >= _Rep.Length)
            {
                System.Windows.Forms.MessageBox.Show("Replay ends.");
                return;
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
                        Marshal.WriteByte(data, dik, 0x80);
                    }
                    else
                    {
                        //Marshal.WriteByte(data, dik, 0x00);
                    }
                }
            }

            _RepOffset += 3;
        }
    }
}
