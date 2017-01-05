using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AGSO.Core.Connection
{
    class ServerPlayerSequenceHandler : SequenceHandler
    {
        public ServerPlayerSequenceHandler() :
            base(10)
        {
        }

        public volatile bool EnableAutoFill;
        private byte[] _AutoFillData = new byte[10];

        protected override bool CheckInterpolate(byte length, byte a, byte b)
        {
            return length < 12;
        }

        protected override byte Interpolate(byte length, byte t, byte a, byte b)
        {
            return ByteData.Interpolate(length, t, a, b);
        }

        protected override void WaitFor(byte time)
        {
            var lastData = base._LastReturned;
            if (EnableAutoFill && time == ByteTime.Inc(lastData[0]))
            {
                _AutoFillData[0] = time;
                for (int i = 1; i < 10; ++i)
                {
                    var dd = new ByteData(lastData[i]);
                    if (dd.Frame < 15)
                    {
                        dd.Frame += 1;
                    }
                    _AutoFillData[i] = dd.Data;
                }
                Receive(_AutoFillData);
            }
        }

        protected override void ResetAll(long time)
        {
        }
    }
}
