using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AGSO.Core.Connection
{
    struct ByteData
    {
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
    }
}
