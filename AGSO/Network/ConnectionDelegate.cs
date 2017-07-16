using PluginUtils;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AGSO.Network
{
    class NamedPipeDelegateConnection
    {
        private static NamedPipeServerStream _Income;
        private static NamedPipeServerStream _Outcome;
        private static NamedPipeServerStream _CmdOutcome;

        private static byte[] _Buffer = new byte[1024 * 10];

        private static AutoResetEvent _IncomeReady = new AutoResetEvent(false);
        private static AutoResetEvent _IncomeClear = new AutoResetEvent(false);

        public static void Init()
        {
            int procId = System.Diagnostics.Process.GetCurrentProcess().Id;
            _Income = new NamedPipeServerStream("agso_named_pipe_i" + procId, PipeDirection.InOut);
            _Outcome = new NamedPipeServerStream("agso_named_pipe_o" + procId, PipeDirection.InOut);
            _CmdOutcome = new NamedPipeServerStream("agso_named_pipe_co" + procId, PipeDirection.InOut);

            _Income.WaitForConnection();
            _Outcome.WaitForConnection();
            _CmdOutcome.WaitForConnection();

            var th = new Thread(ReadThread);
            th.Start();
        }

        public static void Bind(int port)
        {
            _CmdOutcome.WriteByte(0x01);
            _CmdOutcome.Write(BitConverter.GetBytes((ushort)port), 0, 2);
        }

        public static void Client()
        {
            _CmdOutcome.WriteByte(0x02);
        }

        public static void Close()
        {
        }

        public static bool Receive(IntPtr buffer, int capacity, ref WinSock.sockaddr_in addr, out int length)
        {
            if (_IncomeReady.WaitOne(0))
            {
                addr.sin_addr = ReadInt(_Income);
                addr.sin_port = ReadShort(_Income);
                length = (int)ReadInt(_Income);
                if (_Buffer.Length < length)
                {
                    throw new InvalidDataException();
                }
                _Income.Read(_Buffer, 0, length);
                Marshal.Copy(_Buffer, 0, buffer, length > capacity ? capacity : length);

                _IncomeClear.Set();
                return true;
            }
            length = 0;
            return false;
        }

        public static void Send(IntPtr buffer, int length, ref WinSock.sockaddr_in addr)
        {
            Marshal.Copy(buffer, _Buffer, 0, length);

            _Outcome.WriteByte(0x01);
            WriteInt(_Outcome, addr.sin_addr);
            WriteShort(_Outcome, addr.sin_port);
            WriteInt(_Outcome, (uint)length);
            _Outcome.Write(_Buffer, 0, length);
        }

        private static void ReadThread()
        {
            while (true)
            {
                var b = _Income.ReadByte();
                if (b == 1)
                {
                    _IncomeReady.Set();
                    _IncomeClear.WaitOne();
                }
                else
                {
                    throw new InvalidDataException();
                }
            }
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

    class DirectUDPConnection
    {
        private static object _Mutex = new object();
        private static IntPtr _Socket;

        public static void Init()
        {
            lock (_Mutex)
            {
                if (_Socket != IntPtr.Zero)
                {
                    throw new InvalidOperationException();
                }
                _Socket = WinSock.socket(WinSock.AF_INET, WinSock.SOCK_DGRAM, WinSock.IPPROTO_UDP);
                int val = 1;
                WinSock.ioctlsocket(_Socket, 0x8004667e, ref val);
            }
        }

        public static void Bind(int port)
        {
            WinSock.sockaddr_in addr = new WinSock.sockaddr_in();
            addr.sin_family = WinSock.AF_INET;
            addr.sin_port = WinSock.htons((ushort)port);
            addr.sin_addr = 0; //any

            WinSock.bind(_Socket, ref addr, WinSock.sockaddr_in.Size);
        }

        public static void Close()
        {
            lock (_Mutex)
            {
                WinSock.closesocket(_Socket);
                _Socket = IntPtr.Zero;
            }
        }

        public static void Send(IntPtr buffer, int length, ref WinSock.sockaddr_in addr)
        {
            if (WinSock.sendto(_Socket, buffer, length, 0, ref addr, WinSock.sockaddr_in.Size) < 0)
            {
                throw new NetworkException(Marshal.GetLastWin32Error());
            }
        }

        public static bool Receive(IntPtr buffer, int capacity, ref WinSock.sockaddr_in addr, out int length)
        {
            int addrlen = WinSock.sockaddr_in.Size;
            var len = WinSock.recvfrom(_Socket, buffer, capacity, 0, ref addr, ref addrlen);
            if (len >= 0)
            {
                length = len;
                return true;
            }
            var e = Marshal.GetLastWin32Error();
            if (e == 10035)
            {
                length = 0;
                return false;
            }
            throw new NetworkException(e);
        }
    }
}
