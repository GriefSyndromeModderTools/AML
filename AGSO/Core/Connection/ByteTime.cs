using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AGSO.Core.Connection
{
    static class ByteTime
    {
        public static byte Inc(byte b)
        {
            return (byte)((b + 1) & 255);
        }

        public static int Diff(byte a, byte b)
        {
            if (a <= b)
            {
                return b - a;
            }
            return b + 256 - a;
        }
    }
}
