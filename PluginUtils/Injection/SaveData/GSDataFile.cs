using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using zlib;

namespace PluginUtils.Injection.SaveData
{
    public static class GSDataFile
    {
        public class CompoundType : Dictionary<object, object>
        {
            public CompoundType(bool a)
            {
                IsArray = a;
            }

            public readonly bool IsArray;
        }

        private enum SqType
        {
            SQOBJECT_REF_COUNTED = 0x08000000,
            SQOBJECT_NUMERIC = 0x04000000,
            SQOBJECT_DELEGABLE = 0x02000000,
            SQOBJECT_CANBEFALSE = 0x01000000,

            _RT_NULL = 0x00000001,
            _RT_INTEGER = 0x00000002,
            _RT_FLOAT = 0x00000004,
            _RT_BOOL = 0x00000008,
            _RT_STRING = 0x00000010,
            _RT_TABLE = 0x00000020,
            _RT_ARRAY = 0x00000040,
            _RT_USERDATA = 0x00000080,
            _RT_CLOSURE = 0x00000100,
            _RT_NATIVECLOSURE = 0x00000200,
            _RT_GENERATOR = 0x00000400,
            _RT_USERPOINTER = 0x00000800,
            _RT_THREAD = 0x00001000,
            _RT_FUNCPROTO = 0x00002000,
            _RT_CLASS = 0x00004000,
            _RT_INSTANCE = 0x00008000,
            _RT_WEAKREF = 0x00010000,

            OT_NULL = (_RT_NULL | SQOBJECT_CANBEFALSE),
            OT_INTEGER = (_RT_INTEGER | SQOBJECT_NUMERIC | SQOBJECT_CANBEFALSE),
            OT_FLOAT = (_RT_FLOAT | SQOBJECT_NUMERIC | SQOBJECT_CANBEFALSE),
            OT_BOOL = (_RT_BOOL | SQOBJECT_CANBEFALSE),
            OT_STRING = (_RT_STRING | SQOBJECT_REF_COUNTED),
            OT_TABLE = (_RT_TABLE | SQOBJECT_REF_COUNTED | SQOBJECT_DELEGABLE),
            OT_ARRAY = (_RT_ARRAY | SQOBJECT_REF_COUNTED),
            OT_USERDATA = (_RT_USERDATA | SQOBJECT_REF_COUNTED | SQOBJECT_DELEGABLE),
            OT_CLOSURE = (_RT_CLOSURE | SQOBJECT_REF_COUNTED),
            OT_NATIVECLOSURE = (_RT_NATIVECLOSURE | SQOBJECT_REF_COUNTED),
            OT_GENERATOR = (_RT_GENERATOR | SQOBJECT_REF_COUNTED),
            OT_USERPOINTER = _RT_USERPOINTER,
            OT_THREAD = (_RT_THREAD | SQOBJECT_REF_COUNTED),
            OT_FUNCPROTO = (_RT_FUNCPROTO | SQOBJECT_REF_COUNTED), //internal usage only
            OT_CLASS = (_RT_CLASS | SQOBJECT_REF_COUNTED),
            OT_INSTANCE = (_RT_INSTANCE | SQOBJECT_REF_COUNTED | SQOBJECT_DELEGABLE),
            OT_WEAKREF = (_RT_WEAKREF | SQOBJECT_REF_COUNTED)
        }

        public static CompoundType Read(byte[] data)
        {
            var ret = new CompoundType(false);
            byte[] buffer = new byte[1024 * 100]; //TODO
            int len = Inflate(data, buffer);
            int p = 0;
            while (p < len - 8)
            {
                object key;
                var obj = ReadEntry(buffer, ref p, out key);
                ret.Add(key, obj);
            }
            return ret;
        }

        private static object ReadEntry(byte[] data, ref int p, out object key)
        {
            var t = (SqType)ReadInt32(data, ref p);
            if (t == SqType.OT_STRING)
            {
                key = ReadString(data, ref p);
            }
            else if (t == SqType.OT_INTEGER)
            {
                key = ReadInt32(data, ref p);
            }
            else
            {
                throw new IOException();
            }
            return ReadValue(data, ref p);
        }

        private static object ReadValue(byte[] data, ref int p)
        {
            var type = (SqType)ReadInt32(data, ref p);
            switch (type)
            {
                case SqType.OT_NULL:
                    return null;
                case SqType.OT_BOOL:
                    return ReadBool(data, ref p);
                case SqType.OT_INTEGER:
                    return ReadInt32(data, ref p);
                case SqType.OT_FLOAT:
                    return ReadFloat(data, ref p);
                case SqType.OT_STRING:
                    return ReadString(data, ref p);
                case SqType.OT_ARRAY:
                    {
                        var ret = new CompoundType(true);
                        ReadCompound(data, ref p, ret);
                        if ((SqType)ReadInt32(data, ref p) != SqType.OT_NULL)
                        {
                            throw new IOException();
                        }
                        return ret;
                    }
                case SqType.OT_TABLE:
                    {
                        var ret = new CompoundType(false);
                        ReadCompound(data, ref p, ret);
                        if ((SqType)ReadInt32(data, ref p) != SqType.OT_NULL)
                        {
                            throw new IOException();
                        }
                        return ret;
                    }
                default:
                    throw new IOException();
            }
        }

        private static void ReadCompound(byte[] data, ref int p, CompoundType ret)
        {
            var count = ReadInt32(data, ref p);
            for (int i = 0; i < count; ++i)
            {
                object key;
                var value = ReadEntry(data, ref p, out key);
                ret.Add(key, value);
            }
        }

        private static bool ReadBool(byte[] data, ref int p)
        {
            return data[p++] != 0;
        }

        private static float ReadFloat(byte[] data, ref int p)
        {
            var ret = BitConverter.ToSingle(data, p);
            p += 4;
            return ret;
        }

        private static string ReadString(byte[] data, ref int p)
        {
            var len = ReadInt32(data, ref p);
            var ret = Encoding.ASCII.GetString(data, p, len);
            p += len;
            return ret;
        }

        private static int ReadInt32(byte[] data, ref int p)
        {
            int ret = BitConverter.ToInt32(data, p);
            p += 4;
            return ret;
        }

        private static int Inflate(byte[] input, byte[] output)
        {
            ZStream s = new ZStream();
            s.avail_in = input.Length - 4;
            s.next_in = input;
            s.next_in_index = 4;

            s.next_out = output;
            s.avail_out = output.Length;

            s.inflateInit();
            s.inflate(zlibConst.Z_NO_FLUSH);
            s.inflateEnd();

            return s.next_out_index - 4;
        }

        public static byte[] Write(CompoundType dict)
        {
            using (var ms = new MemoryStream())
            {
                using (var bw = new BinaryWriter(ms))
                {
                    foreach (var entry in dict)
                    {
                        WriteEntry(bw, entry.Key, entry.Value);
                    }

                    byte[] buffer = new byte[ms.Length];
                    int len = Deflate(ms.ToArray(), buffer);
                    byte[] ret = new byte[len + 4];
                    Buffer.BlockCopy(new int[] { len }, 0, ret, 0, 4);
                    Buffer.BlockCopy(buffer, 0, ret, 4, len);
                    return ret;
                }
            }
        }

        private static void WriteEntry(BinaryWriter bw, object key, object value)
        {
            if (key is string)
            {
                bw.Write((int)SqType.OT_STRING);
                WriteString(bw, (string)key);
            }
            else if (key is int)
            {
                bw.Write((int)SqType.OT_INTEGER);
                bw.Write((int)key);
            }
            else
            {
                throw new IOException();
            }
            WriteValue(bw, value);
        }

        private static void WriteValue(BinaryWriter bw, object val)
        {
            if (val == null)
            {
                bw.Write((int)SqType.OT_NULL);
            }
            else if (val is bool)
            {
                bw.Write((int)SqType.OT_BOOL);
                bw.Write((byte)((bool)val ? 1 : 0));
            }
            else if (val is int)
            {
                bw.Write((int)SqType.OT_INTEGER);
                bw.Write((int)val);
            }
            else if (val is float)
            {
                bw.Write((int)SqType.OT_FLOAT);
                bw.Write((float)val);
            }
            else if (val is string)
            {
                bw.Write((int)SqType.OT_STRING);
                WriteString(bw, (string)val);
            }
            else if (val is CompoundType)
            {
                var cval = (CompoundType)val;
                bw.Write(cval.IsArray ? (int)SqType.OT_ARRAY : (int)SqType.OT_TABLE);
                bw.Write(cval.Count);
                foreach (var entry in cval)
                {
                    WriteEntry(bw, entry.Key, entry.Value);
                }
                bw.Write((int)SqType.OT_NULL);
            }
            else
            {
                throw new IOException();
            }
        }

        private static void WriteString(BinaryWriter bw, string val)
        {
            var data = Encoding.ASCII.GetBytes(val);
            bw.Write(data.Length);
            bw.Write(data);
        }

        private static int Deflate(byte[] input, byte[] output)
        {
            ZStream s = new ZStream();
            s.avail_in = input.Length;
            s.next_in = input;
            s.next_in_index = 0;

            s.next_out = output;
            s.avail_out = output.Length;

            s.deflateInit(zlibConst.Z_BEST_SPEED);
            while (s.deflate(zlibConst.Z_FULL_FLUSH) == 0) ;
            s.deflateEnd();

            return s.next_out_index;
        }
    }
}
