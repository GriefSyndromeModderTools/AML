using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PluginUtils.Injection.File
{
    public class SimpleReplacementFileProxy : IFileProxy
    {
        public int Seek(int method, int num)
        {
            throw new NotImplementedException();
        }

        public int Read(IntPtr buffer, int max)
        {
            throw new NotImplementedException();
        }
    }
}
