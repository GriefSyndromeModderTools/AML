using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AGSO.Core.Connection
{
    class SimpleByteData
    {
        public const byte InitialValue = 0;

        public static bool CheckInterpolate(byte length, byte a, byte b)
        {
            return length <= 8;
        }

        public static byte Interpolate(byte length, byte t, byte a, byte b)
        {
            return (byte)((a << length | b) >> t);
        }

        public static byte Append(byte l, bool v)
        {
            return (byte)(l << 1 | (v ? 1 : 0));
        }

        public static byte Append(byte l)
        {
            return (byte)(l << 1 | (l & 1));
        }

        public static bool Status(byte v)
        {
            return (v & 1) != 0;
        }
    }
}
