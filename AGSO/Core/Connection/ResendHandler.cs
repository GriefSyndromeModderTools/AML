using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AGSO.Core.Connection
{
    class ResendHandler
    {
        public ResendHandler(int bufferLength)
        {
        }

        public void Acknowledge(byte time)
        {
            //should handle skip packet
            //if more than 8 packets are lost, and 9th is received, the 8 should be sent
        }

        public void NewData(byte[] data)
        {

        }

        public bool TryDequeue(out byte[] data)
        {
            data = null;
            return false;
        }
    }
}
