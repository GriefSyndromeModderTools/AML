using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AGSO.Core.Connection
{
    struct ByteData
    {
        public const byte InitialValue = 15;

        public bool State;
        public byte Change;
        public byte Frame;

        public ByteData(byte b)
        {
            State = b > 128;
            Change = (byte)((b >> 4) & 7);
            Frame = (byte)(b & 15);
        }

        public byte Data
        {
            get
            {
                return (byte)((State ? 128 : 0) | Change << 4 | Frame);
            }
        }

        public static bool CheckInterpolate(byte length, byte a, byte b)
        {
            var aa = new ByteData(a);
            var bb = new ByteData(b);
            if (aa.Change == bb.Change)
            {
                return true;
            }
            if (((aa.Change + 1) & 7) == bb.Change && bb.Frame != 15)
            {
                return true;
            }
            return false;
        }

        public static byte Interpolate(byte length, byte t, byte a, byte b)
        {
            var aa = new ByteData(a);
            var bb = new ByteData(b);
            if (aa.Change == bb.Change)
            {
                //a + t
                if (aa.Frame + t >= 15)
                {
                    aa.Frame = 15;
                }
                else
                {
                    aa.Frame = (byte)(aa.Frame + t);
                }
                return aa.Data;
            }
            if (((aa.Change + 1) & 7) == bb.Change && bb.Frame != 15)
            {
                if (t + bb.Frame >= length)
                {
                    //b - (length - t)
                    bb.Frame -= (byte)(length - t);
                    return bb.Data;
                }
                else
                {
                    //a + t
                    if (aa.Frame + t >= 15)
                    {
                        aa.Frame = 15;
                    }
                    else
                    {
                        aa.Frame = (byte)(aa.Frame + t);
                    }
                    return aa.Data;
                }
            }
            //a + t
            if (aa.Frame + t >= 15)
            {
                aa.Frame = 15;
            }
            else
            {
                aa.Frame = (byte)(aa.Frame + t);
            }
            return aa.Data;
        }

        public static byte Append(byte l, bool v)
        {
            var byteLast = new ByteData(l);
            if (v == byteLast.State)
            {
                if (byteLast.Frame < 15)
                {
                    byteLast.Frame += 1;
                }
                return byteLast.Data;
            }
            else
            {
                byteLast.State = v;
                byteLast.Change = (byte)((byteLast.Change + 1) & 7);
                byteLast.Frame = 0;
                return byteLast.Data;
            }
        }

        public static byte Append(byte l)
        {
            var byteLast = new ByteData(l);
            if (byteLast.Frame < 15)
            {
                byteLast.Frame += 1;
            }
            return byteLast.Data;
        }

        public static bool Status(byte v)
        {
            return (v & 128) != 0;
        }
    }
}
