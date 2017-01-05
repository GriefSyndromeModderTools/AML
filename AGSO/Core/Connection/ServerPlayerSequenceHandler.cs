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
            for (int i = 0; i < NetworkServerInputHandler.InitEmptyCount; ++i)
            {
                this.ReceiveEmpty(15);
            }
        }

        protected override bool CheckInterpolate(byte length, byte a, byte b)
        {
            return length < 10;
        }

        protected override byte Interpolate(byte length, byte t, byte a, byte b)
        {
            return ByteData.Interpolate(length, t, a, b);
        }

        protected override void WaitFor(byte time)
        {
        }

        protected override void ResetAll(long time)
        {
        }
    }
}
