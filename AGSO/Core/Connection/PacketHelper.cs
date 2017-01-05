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
        public static void Write(this Buffer buffer, PacketType type)
        {
            buffer.Reset(0);
            buffer.WriteByte((byte)type);
            buffer.WriteSum();
        }

        public static void Write(this Buffer buffer, PacketType type, byte[] data)
        {
            buffer.Reset(0);
            buffer.WriteByte((byte)type);
            buffer.WriteBytes(data);
            buffer.WriteSum();
        }
    }
}
