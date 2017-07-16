using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace AMLLoader.Network
{
    public class Buffer
    {
        public readonly IntPtr Pointer;
        public readonly int Capacity;
        private int _Length;
        private int _Position;

        public int Length { get { return _Length; } }

        public Buffer(int len)
        {
            Pointer = Marshal.AllocHGlobal(len);
            Capacity = len;
        }

        ~Buffer()
        {
            Marshal.FreeHGlobal(Pointer);
        }

        public void Reset(int len)
        {
            _Position = 0;
            _Length = len;
        }

        public void WriteInt32(int val)
        {
            Marshal.WriteInt32(Pointer, _Position, val);
            _Position += 4;
            _Length += 4;
        }

        public void WriteInt16(short val)
        {
            Marshal.WriteInt16(Pointer, _Position, val);
            _Position += 2;
            _Length += 2;
        }

        public void WriteByte(byte val)
        {
            Marshal.WriteByte(Pointer, _Position, val);
            _Position += 1;
            _Length += 1;
        }

        public void WriteBytes(byte[] val)
        {
            int c = val.Length;
            Marshal.Copy(val, 0, Pointer + _Position, c);
            _Position += c;
            _Length += c;
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

        public byte ReadByte()
        {
            var ret = Marshal.ReadByte(Pointer, _Position);
            _Position += 1;
            return ret;
        }

        public void ReadBytes(byte[] buffer, int offset, int count)
        {
            Marshal.Copy(Pointer + _Position, buffer, offset, count);
            _Position += count;
        }

        public void ReadBytes(byte[] buffer)
        {
            int c = buffer.Length;
            Marshal.Copy(Pointer + _Position, buffer, 0, c);
            _Position += c;
        }

        public void WriteSum()
        {
            byte val = 255;
            for (int i = 0; i < _Length; ++i)
            {
                val ^= Marshal.ReadByte(Pointer, i);
            }
            WriteByte(val);
        }

        public bool CheckSum()
        {
            if (_Length <= 0)
            {
                return false;
            }
            byte val = 255;
            for (int i = 0; i < _Length - 1; ++i)
            {
                val ^= Marshal.ReadByte(Pointer, i);
            }
            return Marshal.ReadByte(Pointer, --_Length) == val;
        }
    }
}
