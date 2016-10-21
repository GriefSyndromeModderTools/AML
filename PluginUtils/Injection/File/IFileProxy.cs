using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PluginUtils.Injection.File
{
    public interface IFileProxyFactory
    {
        IFileProxy Create();
    }

    public interface IFileProxy
    {
        int Seek(int method, int num);
        int Read(IntPtr buffer, int max);
    }
}
