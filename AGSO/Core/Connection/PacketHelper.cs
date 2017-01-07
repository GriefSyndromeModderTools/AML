using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Buffer = AGSO.Network.Buffer;

namespace AGSO.Core.Connection
{
    static class PacketHelper
    {
        public static void Write(this Buffer buffer, PacketType type, byte sid)
        {
            buffer.Reset(0);
            buffer.WriteByte((byte)type);
            buffer.WriteByte(sid);
            buffer.WriteSum();
        }

        public static void Write(this Buffer buffer, PacketType type, byte sid, byte[] data)
        {
            buffer.Reset(0);
            buffer.WriteByte((byte)type);
            buffer.WriteByte(sid);
            buffer.WriteBytes(data);
            buffer.WriteSum();
        }
    }
}
