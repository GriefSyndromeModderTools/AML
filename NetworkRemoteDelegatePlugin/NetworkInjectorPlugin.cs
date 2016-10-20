using PluginUtils;
using PluginUtils.Injection.Native;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetworkRemoteDelegatePlugin
{
    public class NetworkInjectorPlugin : IAMLPlugin
    {
        public void Init()
        {
            new CloseSocket(this);
        }

        public void Load()
        {
        }

        private int ProcessReturnValue(int val)
        {
            return val;
        }

        private int CloseSocket(int socket)
        {
            int ret = 0;
            return ProcessReturnValue(ret);
        }

        private class CloseSocket : NativeWrapper
        {
            private readonly NetworkInjectorPlugin _Parent;
            private delegate int FuncType(int i);

            public CloseSocket(NetworkInjectorPlugin parent)
            {
                _Parent = parent;
                this.InjectFunctionPointer<FuncType>(AddressHelper.CodeOffset("gso", 0x1C264), 4);
            }

            protected override void Triggered(NativeWrapper.NativeEnvironment env)
            {
                _Parent.CloseSocket(env.GetParameterI(0));
            }
        }


    }
}
/*

	//check for SOCKET_ERROR(-1)
	//100, int -> int, int
	ret_i = closesocket(s);

	//check error INVALID_SOCKET(-1)
	//101, int, int, int -> int, int
	s = socket(int_any, int_any, int_any);

	//check for SOCKET_ERROR(-1)
	//102, int, buffer_in -> int, int
	ret_i = bind(s, (const sockaddr*)addr_buffer, sizeof(addr_buffer));

	//check for SOCKET_ERROR(-1)
	//103, int, buffer_in, int, buffer_in -> int, int
	ret_i = sendto(s, data_buffer, data_buffer_len, int_any, (const sockaddr*)addr_buffer, sizeof(addr_buffer));

	//check for SOCKET_ERROR(-1)
	//104, int, int, int -> int, int
	ret_i = ioctlsocket(s, int_any, NULL);

	//check for SOCKET_ERROR(-1)
	//105, int, buffer_out, int, buffer_out_set -> int, int
	ret_i = recvfrom(s, data_buffer, data_buffer_len, int_any, (sockaddr*)addr_buffer, &addr_buffer_len);

*/
