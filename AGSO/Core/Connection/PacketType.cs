using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AGSO.Core.Connection
{
    enum PacketType
    {
        None = 30,
        NewConnection,
        ConnectionAccept,
        PingRequest,
        PingReply,
        ServerStatus,
        ClientStatus,
        ServerStop,
        ClientStop,
        GameStart,
        ClientReady,
        ServerInputData,
        ClientInputData,
    }
}
