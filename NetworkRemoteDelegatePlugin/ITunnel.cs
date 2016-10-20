using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetworkRemoteDelegatePlugin
{
    interface IInputBuffer
    {
        int Length { get; }
        void CopyToManaged(byte[] dest, int offset);
        void CopyToUnmanaged(IntPtr dest, int offset);
    }

    interface IOutputBuffer
    {
        int MaxLength { get; }
        int Length { get; }
        void CopyFromManaged(byte[] dest, int offset, int len);
        void CopyFromUnmanaged(IntPtr dest, int offset, int len);
    }

    interface ITunnel
    {
        APIReturnValue Socket(int af, int type, int protocol);
        APIReturnValue CloseSocket(int s);
        APIReturnValue Bind(int socket, IInputBuffer addr);
        APIReturnValue IOControl(int socket, int cmd, int arg);
        void Send(int socket, int flags, IInputBuffer data, IInputBuffer addr);
        bool Receive(int socket, int flags, IOutputBuffer data, IOutputBuffer addr);
    }
}
