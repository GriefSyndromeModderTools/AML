using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace AGSO.Network
{
    public class WinSock
    {
        [DllImport("ws2_32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern Int32 WSAStartup(short wVersionRequested, out WSAData wsaData);

        [DllImport("ws2_32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        public static extern IntPtr socket(ushort af, ushort socket_type, ushort protocol);

        [DllImport("Ws2_32.dll")]
        public static extern int bind(IntPtr s, ref sockaddr_in addr, int addrsize);

        [DllImport("Ws2_32.dll", CharSet = CharSet.Ansi)]
        public static extern uint inet_addr(string cp);

        [DllImport("Ws2_32.dll")]
        public static extern ushort htons(ushort hostshort);

        [DllImport("ws2_32.dll", SetLastError = true)]
        public static extern int sendto(IntPtr Socket, IntPtr buff, int len, int flags, ref sockaddr_in To, int tomlen);
        
        [DllImport("ws2_32.dll", SetLastError = true)]
        public static extern int recvfrom(IntPtr Socket, IntPtr buf, int len, int flags, ref sockaddr_in from, ref int fromlen);
        
        [DllImport("ws2_32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        public static extern int closesocket(IntPtr s);

        [DllImport("Ws2_32.dll")]
        public static extern int ioctlsocket(IntPtr s, uint cmd, ref int argp);

        public const ushort AF_INET = 2;
        public const ushort SOCK_DGRAM = 2;
        public const ushort IPPROTO_UDP = 17;

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
        public struct sockaddr_in
        {
            public ushort sin_family;
            public ushort sin_port;
            public uint sin_addr;
            public ulong sin_zero;

            public const int Size = 16;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
        public struct WSAData
        {
            public Int16 version;
            public Int16 highVersion;

            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 257)]
            public String description;

            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 129)]
            public String systemStatus;

            public Int16 maxSockets;
            public Int16 maxUdpDg;
            public IntPtr vendorInfo;
        }

        static WinSock()
        {
        }

    }
}
