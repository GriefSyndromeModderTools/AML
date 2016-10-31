using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace AGSO.Network
{
    public class Buffer
    {
        public readonly IntPtr Pointer;
        public readonly int Length;
        public int Size { get { return _Position; } }
        private int _Position;

        public Buffer(int len)
        {
            Pointer = Marshal.AllocHGlobal(len);
            Length = len;
        }

        ~Buffer()
        {
            Marshal.FreeHGlobal(Pointer);
        }

        public void Reset()
        {
            _Position = 0;
        }

        public void WriteInt32(int val)
        {
            Marshal.WriteInt32(Pointer, _Position, val);
            _Position += 4;
        }

        public void WriteInt16(short val)
        {
            Marshal.WriteInt16(Pointer, _Position, val);
            _Position += 2;
        }

        public void WriteByte(byte val)
        {
            Marshal.WriteByte(Pointer, _Position, val);
            _Position += 1;
        }

        public int ReadInt32()
        {
            var ret = Marshal.ReadInt32(Pointer, _Position);
            _Position += 4;
            return ret;
        }

        public short ReadInt16()
        {
            var ret = Marshal.ReadInt16(Pointer, _Position);
            _Position += 2;
            return ret;
        }

        public short ReadByte()
        {
            var ret = Marshal.ReadByte(Pointer, _Position);
            _Position += 1;
            return ret;
        }
    }
}
