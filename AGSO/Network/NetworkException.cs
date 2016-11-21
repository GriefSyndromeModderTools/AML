using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AGSO.Network
{
    class NetworkException : Exception
    {
        public NetworkException(int code)
        {
            ErrorCode = code;
        }
        public int ErrorCode { get; private set; }
    }
}
