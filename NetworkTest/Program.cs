using AGSO.Network;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace NetworkTest
{
    class Program
    {
        static void Main(string[] args)
        {
            WinSock.WSAData wsa;
            WinSock.WSAStartup(0x22, out wsa);

            Console.WriteLine("c/s:");
            if (Console.ReadKey().Key == ConsoleKey.C)
            {
                ClientMain();
            }
            else
            {
                ServerMain();
            }
        }

        private static void ClientMain()
        {
            IntPtr s = WinSock.socket(WinSock.AF_INET, WinSock.SOCK_DGRAM, WinSock.IPPROTO_UDP);

            int val = 1;
            WinSock.ioctlsocket(s, 0x8004667e, ref val);

            WinSock.sockaddr_in addr = new WinSock.sockaddr_in();
            addr.sin_family = WinSock.AF_INET;
            addr.sin_port = WinSock.htons(10800);
            addr.sin_addr = WinSock.inet_addr("127.0.0.1");

            int buffer_len = 64;
            IntPtr buffer = Marshal.AllocHGlobal(buffer_len);
            for (int i = 0; i < buffer_len; ++i)
            {
                Marshal.WriteByte(buffer, i, (byte)i);
            }

            WinSock.sendto(s, buffer, buffer_len, 0, ref addr, WinSock.sockaddr_in.Size);

            WinSock.closesocket(s);
        }

        private static void ServerMain()
        {
            IntPtr s = WinSock.socket(WinSock.AF_INET, WinSock.SOCK_DGRAM, WinSock.IPPROTO_UDP);

            int val = 1;
            WinSock.ioctlsocket(s, 0x8004667e, ref val);

            WinSock.sockaddr_in addr = new WinSock.sockaddr_in();
            addr.sin_family = WinSock.AF_INET;
            addr.sin_port = WinSock.htons(10800);
            addr.sin_addr = 0; //any

            WinSock.bind(s, ref addr, WinSock.sockaddr_in.Size);

            int buffer_len = 64;
            IntPtr buffer = Marshal.AllocHGlobal(buffer_len);
            while (true)
            {
                WinSock.sockaddr_in recvaddr = new WinSock.sockaddr_in();
                int addrlen = WinSock.sockaddr_in.Size;
                if (WinSock.recvfrom(s, buffer, buffer_len, 0, ref recvaddr, ref addrlen) >= 0)
                {
                    break;
                }
                var e = Marshal.GetLastWin32Error();
                if (e != 10035)
                {
                    Console.WriteLine(e);
                    break;
                }
                System.Threading.Thread.Sleep(1);
            }

            WinSock.closesocket(s);
        }
    }
}
