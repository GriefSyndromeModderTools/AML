using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AGSO.Core.Connection
{
    class ClientSequenceHandler : SequenceHandler
    {
        public ClientSequenceHandler() :
            base(28)
        {
        }

        protected override bool CheckInterpolate(byte length, byte a, byte b)
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

        protected override byte Interpolate(byte length, byte t, byte a, byte b)
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
            //never goes here
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

        protected override void WaitFor(byte time)
        {
        }

        protected override void ResetAll(long time)
        {
        }
    }
}
