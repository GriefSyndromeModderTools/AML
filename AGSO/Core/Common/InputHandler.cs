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
        private static bool _Use2P;

        static InputHandler()
        {
            AGSO.Misc.GSO2ReplayFile rep = new Misc.GSO2ReplayFile(
                @"E:\Games\[game]GRIEFSYNDROME\griefsyndrome\replay\20161006_234348.rep");
            _Rep = rep.InputData;

            var keyconfigData = System.IO.File.ReadAllBytes(
                @"E:\Games\[game]GRIEFSYNDROME\griefsyndrome\keyconfig.dat");
            _KeyConfig = new int[9 * 3];
            Buffer.BlockCopy(keyconfigData, 0, _KeyConfig, 0, 9 * 3 * 4);
        }

        private static int[] _KeyConfig;

        private static readonly ushort[] _Mask = new ushort[] {
            0x01, 0x02, 0x04, 0x08, 0x10, 0x20, 0x40, 0x80, 0x100
        };

        public static void Aquire(IntPtr data)
        {
            if (_RepOffset == 0)
            {
                AGSO.Core.FP.FPCode.Run();
            }
            if (_RepOffset >= 0)
            {
                for (int p = 0; p < 3; p++)
                {
                    int playerOffset = _RepOffset + p;
                    var pp = p;
                    if (_Use2P && pp != 2) pp = 1 - pp;
                    if (pp == 1)
                    {
                        playerOffset -= 0;
                    }
                    for (int k = 0; k < 9; ++k)
                    {
                        var dik = _KeyConfig[pp * 9 + k];
                        if ((_Rep[playerOffset] & _Mask[k]) != 0)
                        {
                            if (Marshal.ReadByte(data, dik) != 0)
                            {

                            }
                            Marshal.WriteByte(data, dik, 0x80);
                        }
                        else
                        {
                            //Marshal.WriteByte(data, dik, 0x00);
                        }
                    }
                }
            }

            _RepOffset += 3;
        }
    }
}
