using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GSORepPlayer
{
    class GSO2ReplayFile
    {
        public class FileFormatException : Exception
        {
            public FileFormatException(string msg)
                : base(msg)
            {
            }
        }

        private static UInt32 Magic = 0x50525347;

        public int BaseLap;
        public UInt16[] InputData;
        public ChatMessage[] ChatMessages;

        public int FileSize;

        public class ChatMessage
        {
            public UInt32 Time;
            public UInt32 Player;
            public string Message;
        }

        public GSO2ReplayFile(string filename)
        {
            var data = File.ReadAllBytes(filename);

            if (BitConverter.ToUInt32(data, 0) != Magic)
            {
                throw new FileFormatException("not a replay file");
            }
            var baseLap = BitConverter.ToUInt32(data, 4);
            var length = BitConverter.ToUInt32(data, 8);
            var chat = BitConverter.ToUInt32(data, 12);

            if ((length % 3) != 0)
            {
                //throw new FileFormatException("invalid file length");
            }

            BaseLap = (int)baseLap;

            InputData = new UInt16[length];
            Buffer.BlockCopy(data, 16, InputData, 0, (int)length * 2);
            return;
            int chatOffset = 16 + (int)length * 2;
            ChatMessages = new ChatMessage[chat];

            for (int i = 0; i < chat; ++i)
            {
                try
                {
                    ChatMessages[i] = ReadChat(data, ref chatOffset);
                }
                catch
                {
                    throw new FileFormatException("chat message reading error");
                }
            }

            FileSize = data.Length;
        }

        public GSO2ReplayFile(int lap, int time)
        {
            BaseLap = lap;
            InputData = new UInt16[time * 3];
            ChatMessages = new ChatMessage[0];
        }

        private static ChatMessage ReadChat(byte[] data, ref int offset)
        {
            var t = BitConverter.ToUInt32(data, offset);
            var p = BitConverter.ToUInt32(data, offset + 4);
            var l = BitConverter.ToUInt32(data, offset + 8);
            char[] str = new char[l];
            Buffer.BlockCopy(data, offset + 12, str, 0, (int)l * 2);
            offset += (int)(12 + ((l + 1) * 2));
            return new ChatMessage
            {
                Time = t,
                Player = p,
                Message = new string(str),
            };
        }

        public void Save(string filename)
        {
            if (File.Exists(filename))
            {
                File.Delete(filename);
            }
            using (var file = File.OpenWrite(filename))
            {
                using (var w = new BinaryWriter(file))
                {
                    w.Write(Magic);
                    w.Write(BaseLap);
                    w.Write(InputData.Length);
                    w.Write(ChatMessages.Length);

                    foreach (var input in InputData)
                    {
                        w.Write(input);
                    }
                    foreach (var msg in ChatMessages)
                    {
                        w.Write(msg.Time);
                        w.Write(msg.Player);
                        w.Write(msg.Message.Length);
                        var buffer = new byte[(msg.Message.Length + 1) * 2];
                        Buffer.BlockCopy(msg.Message.ToCharArray(), 0, buffer, 0, msg.Message.Length * 2);
                        w.Write(buffer);
                    }
                }
            }
        }
    }
}
