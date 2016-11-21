using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AGSO.Network
{
    public class Remote
    {
        public WinSock.sockaddr_in Address;
        public long LastSend;
        public long LastReceived;

        public Remote(ref WinSock.sockaddr_in addr, long time)
        {
            Address = addr;
            LastSend = time;
            LastReceived = time;
        }
    }

    public class Connection
    {
        static Connection()
        {
            WinSock.WSAData wsa;
            WinSock.WSAStartup(0x22, out wsa);
        }

        public Connection(int buffer)
        {
            IntPtr s = WinSock.socket(WinSock.AF_INET, WinSock.SOCK_DGRAM, WinSock.IPPROTO_UDP);
            int val = 1;
            WinSock.ioctlsocket(s, 0x8004667e, ref val);

            _Socket = s;
            _Clock = new Stopwatch();
            Buffer = new Network.Buffer(buffer);
        }

        public readonly Buffer Buffer;

        private IntPtr _Socket;
        private Dictionary<ulong, Remote> _RemoteList = new Dictionary<ulong, Remote>();
        private Stopwatch _Clock;

        private List<ulong> _ToRemove = new List<ulong>();

        private Remote FindRemote(ref WinSock.sockaddr_in addr)
        {
            var ip = addr.sin_addr;
            var port = addr.sin_port;

            ulong val = ((ulong)ip) << 8 | port;
            Remote ret;
            if (_RemoteList.TryGetValue(val, out ret))
            {
                ret.LastReceived = _Clock.ElapsedMilliseconds;
                return ret;
            }

            ret = new Remote(ref addr, _Clock.ElapsedMilliseconds);
            _RemoteList.Add(val, ret);
            return ret;
        }

        public void Bind(int port)
        {
            WinSock.sockaddr_in addr = new WinSock.sockaddr_in();
            addr.sin_family = WinSock.AF_INET;
            addr.sin_port = WinSock.htons((ushort)port);
            addr.sin_addr = 0; //any

            WinSock.bind(_Socket, ref addr, WinSock.sockaddr_in.Size);
        }

        public void Close()
        {
            WinSock.closesocket(_Socket);
        }

        public void UpdateRemoteList(long time)
        {
            long removeSend = _Clock.ElapsedMilliseconds - time;
            _ToRemove.Clear();
            foreach (var e in _RemoteList)
            {
                if (e.Value.LastReceived < e.Value.LastSend &&
                    e.Value.LastSend < removeSend)
                {
                    _ToRemove.Add(e.Key);
                }
            }
            foreach (var k in _ToRemove)
            {
                _RemoteList.Remove(k);
            }
        }

        public Remote Receive()
        {
            WinSock.sockaddr_in recvaddr = new WinSock.sockaddr_in();
            int addrlen = WinSock.sockaddr_in.Size;

            var len = WinSock.recvfrom(_Socket, Buffer.Pointer, Buffer.Length, 0,
                ref recvaddr, ref addrlen);
            if (len >= 0)
            {
                Buffer.Reset();
                return FindRemote(ref recvaddr);
            }
            var e = Marshal.GetLastWin32Error();
            if (e == 10035)
            {
                return null;
            }
            throw new NetworkException(e);
        }

        public bool Receive(out Remote r)
        {
            var rr = Receive();
            r = rr;
            return rr != null;
        }

        public void Send(Remote r)
        {
            if (WinSock.sendto(_Socket, Buffer.Pointer, Buffer.Size, 0, ref r.Address, WinSock.sockaddr_in.Size) < 0)
            {
                throw new NetworkException(Marshal.GetLastWin32Error());
            }
        }

        public Remote Send(string ip, int port)
        {
            WinSock.sockaddr_in addr = new WinSock.sockaddr_in();
            addr.sin_family = WinSock.AF_INET;
            addr.sin_port = WinSock.htons((ushort)port);
            addr.sin_addr = WinSock.inet_addr(ip);
            var remote = FindRemote(ref addr);

            Send(remote);

            return remote;
        }
    }
}
