using AGSO.Network;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AGSO.Core.Connection
{
    interface IConnectionStage
    {
        void OnStart();
        void OnPacket(Remote r);
        void OnTick();
        void OnAbort();
        int Interval { get; }
    }
}
