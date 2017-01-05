using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AMLLoader.Network
{
    static class NamedPipeDelegateServer
    {
        private static IntPtr _Socket;
        private static NamedPipeClientStream _Income, _Outcome, _CmdIncome;
        private static Thread _Send, _Recv, _Cmd;
        private static object _Mutex = new object();
        private static ManualResetEvent _CmdReceived = new ManualResetEvent(false);
        private static ManualResetEvent _ClientBind = new ManualResetEvent(false);

        public static void Run(int proc)
        {
            WinSock.WSAData wsa;
            WinSock.WSAStartup(0x22, out wsa);

            _Socket = WinSock.socket(WinSock.AF_INET, WinSock.SOCK_DGRAM, WinSock.IPPROTO_UDP);
            int val = 1;
            WinSock.ioctlsocket(_Socket, 0x8004667e, ref val);

            _Income = new NamedPipeClientStream(".", "agso_named_pipe_o" + proc, PipeDirection.InOut);
            _Outcome = new NamedPipeClientStream(".", "agso_named_pipe_i" + proc, PipeDirection.InOut);
            _CmdIncome = new NamedPipeClientStream(".", "agso_named_pipe_co" + proc, PipeDirection.InOut);

            var th = new Thread(InitThreadEntry);
            th.Start();
        }

        private static void InitThreadEntry()
        {
            _Income.Connect();
            _Outcome.Connect();
            _CmdIncome.Connect();

            _Send = new Thread(SendThreadEntry);
            _Send.Start();
            _Recv = new Thread(ReceiveThreadEntry);
            _Recv.Start();
            _Cmd = new Thread(CmdThreadEntry);
            _Cmd.Start();
        }

        public static void Stop()
        {
            _Send.Abort();
            _Recv.Abort();
            _Cmd.Abort();
        }

        private static void SendThreadEntry()
        {
            byte[] buffer = new byte[1024 * 10];
            WinSock.sockaddr_in addr = new WinSock.sockaddr_in();
            addr.sin_family = WinSock.AF_INET;

            _CmdReceived.WaitOne();
            while (true)
            {
                if (_Income.ReadByte() == 1)
                {
                    var ip = ReadInt(_Income);
                    var port = ReadShort(_Income);
                    var length = (int)ReadInt(_Income);
                    if (length > buffer.Length)
                    {
                        throw new InvalidDataException();
                    }
                    _Income.Read(buffer, 0, length);

                    addr.sin_port = port;
                    addr.sin_addr = ip;

                    lock (_Mutex)
                    {
                        if (WinSock.sendto(_Socket, buffer, length, 0, ref addr, WinSock.sockaddr_in.Size) < 0)
                        {
                            throw new NetworkException(Marshal.GetLastWin32Error());
                        }
                    }
                    _ClientBind.Set();
                }
                else
                {
                    throw new InvalidDataException();
                }
            }
        }

        private static void ReceiveThreadEntry()
        {
            byte[] buffer = new byte[1024 * 10];
            WinSock.sockaddr_in addr = new WinSock.sockaddr_in();

            _CmdReceived.WaitOne();
            _ClientBind.WaitOne();
            while (true)
            {
                int addrlen = WinSock.sockaddr_in.Size;
                int len;
                lock (_Mutex)
                {
                    len = WinSock.recvfrom(_Socket, buffer, buffer.Length, 0, ref addr, ref addrlen);
                }
                if (len >= 0)
                {
                    _Outcome.WriteByte(0x01);
                    WriteInt(_Outcome, addr.sin_addr);
                    WriteShort(_Outcome, addr.sin_port);
                    WriteInt(_Outcome, (uint)len);
                    _Outcome.Write(buffer, 0, len);
                }
                else
                {
                    var e = Marshal.GetLastWin32Error();
                    if (e != 10035)
                    {
                        throw new NetworkException(e);
                    }
                }
                Thread.Sleep(1);
            }
        }

        private static void CmdThreadEntry()
        {
            switch (_CmdIncome.ReadByte())
            {
                case 1:
                    {
                        var port = ReadShort(_CmdIncome);

                        WinSock.sockaddr_in addr = new WinSock.sockaddr_in();
                        addr.sin_family = WinSock.AF_INET;
                        addr.sin_port = WinSock.htons(port);
                        addr.sin_addr = 0; //any

                        WinSock.bind(_Socket, ref addr, WinSock.sockaddr_in.Size);

                        _ClientBind.Set();
                        break;
                    }
                case 2:
                    break;
                default:
                    throw new InvalidDataException();
            }
            _CmdReceived.Set();
        }

        private static void WriteInt(Stream s, uint i)
        {
            s.WriteByte((byte)(i & 255));
            s.WriteByte((byte)((i >> 8) & 255));
            s.WriteByte((byte)((i >> 16) & 255));
            s.WriteByte((byte)((i >> 24) & 255));
        }

        private static void WriteShort(Stream s, ushort i)
        {
            s.WriteByte((byte)(i & 255));
            s.WriteByte((byte)((i >> 8) & 255));
        }

        private static uint ReadInt(Stream s)
        {
            return (uint)(s.ReadByte() | s.ReadByte() << 8 |
                s.ReadByte() << 16 | s.ReadByte() << 24);
        }

        private static ushort ReadShort(Stream s)
        {
            return (ushort)(s.ReadByte() | s.ReadByte() << 8);
        }
    }
}
