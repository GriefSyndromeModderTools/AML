using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AGSO.Core.Connection
{
    interface IClientSequenceExceptionHandler
    {
        void WaitFor(byte time);
        void ResetAll(long time);
    }
}
