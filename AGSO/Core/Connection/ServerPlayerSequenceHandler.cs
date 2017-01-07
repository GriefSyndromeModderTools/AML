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
            return SimpleByteData.CheckInterpolate(length, a, b);
        }

        protected override byte Interpolate(byte length, byte t, byte a, byte b)
        {
            return SimpleByteData.Interpolate(length, t, a, b);
        }

        protected override void WaitFor(byte time)
        {
            var lastData = base._LastReturned;
            if (EnableAutoFill && time == ByteTime.Inc(lastData[0]))
            {
                _AutoFillData[0] = time;
                for (int i = 1; i < 10; ++i)
                {
                    _AutoFillData[i] = SimpleByteData.Append(lastData[i]);
                }
                Receive(_AutoFillData);
            }
        }

        protected override void ResetAll(long time)
        {
        }
    }
}
