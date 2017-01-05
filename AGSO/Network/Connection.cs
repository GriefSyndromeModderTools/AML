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
        public int ReceiveCount;
        public object Data;

        public Remote(ref WinSock.sockaddr_in addr, long time)
        {
            Address = addr;
            LastSend = time;
            LastReceived = time;
        }

        public override string ToString()
        {
            var addr = BitConverter.GetBytes(Address.sin_addr);
            var port = BitConverter.GetBytes(Address.sin_port);
            return string.Format("{0}.{1}.{2}.{3}:{4}", addr[0], addr[1], addr[2], addr[3], port[0] << 8 | port[1]);
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
        private HashSet<ulong> _BlockRemote = new HashSet<ulong>();

        private List<ulong> _ToRemove = new List<ulong>();

        private ulong GetKey(ref WinSock.sockaddr_in addr)
        {
            var ip = addr.sin_addr;
            var port = addr.sin_port;

            return ((ulong)ip) << 8 | port;
        }

        private Remote FindRemote(ref WinSock.sockaddr_in addr)
        {
            var val = GetKey(ref addr);
            if (_BlockRemote.Contains(val))
            {
                return null;
            }
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

        public void UpdateRemoteList(long receiveTimeout, long activeTimeout)
        {
            long removeSend = _Clock.ElapsedMilliseconds - receiveTimeout;
            long activeTime = _Clock.ElapsedMilliseconds - activeTimeout;
            _ToRemove.Clear();
            foreach (var e in _RemoteList)
            {
                if (e.Value.LastReceived < e.Value.LastSend && e.Value.LastSend < removeSend ||
                    e.Value.LastSend < activeTime && e.Value.LastReceived < activeTime)
                {
                    _ToRemove.Add(e.Key);
                }
            }
            foreach (var k in _ToRemove)
            {
                _RemoteList.Remove(k);
            }
        }

        public bool UpdateRemoteList(Remote r, long receiveTimeout, long activeTimeout)
        {
            //
            return true;

            long removeSend = _Clock.ElapsedMilliseconds - receiveTimeout;
            long activeTime = _Clock.ElapsedMilliseconds - activeTimeout;
            _ToRemove.Clear();

            if (r.LastReceived < r.LastSend && r.LastSend < removeSend ||
                r.LastSend < activeTime && r.LastReceived < activeTime)
            {
                _RemoteList.Remove(GetKey(ref r.Address));
                return false;
            }
            return true;
        }

        public Remote Receive()
        {
            WinSock.sockaddr_in recvaddr = new WinSock.sockaddr_in();
            int addrlen = WinSock.sockaddr_in.Size;

            var len = WinSock.recvfrom(_Socket, Buffer.Pointer, Buffer.Capacity, 0,
                ref recvaddr, ref addrlen);
            if (len >= 0)
            {
                var ret = FindRemote(ref recvaddr);
                if (ret == null)
                {
                    //block
                    return null;
                }
                Buffer.Reset(len);
                ret.ReceiveCount += 1;
                return ret;
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
            if (WinSock.sendto(_Socket, Buffer.Pointer, Buffer.Length, 0, ref r.Address, WinSock.sockaddr_in.Size) < 0)
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

            if (remote == null)
            {
                return null;
            }

            Send(remote);
            return remote;
        }

        public void Block(Remote r)
        {
            var k = GetKey(ref r.Address);
            _BlockRemote.Add(k);
            _RemoteList.Remove(k);
        }

        public void Unblock(Remote r)
        {
            var k = GetKey(ref r.Address);
            _BlockRemote.Remove(k);
        }
    }
}
