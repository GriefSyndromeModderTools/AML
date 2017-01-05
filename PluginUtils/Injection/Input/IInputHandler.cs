using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PluginUtils.Injection.Input
{
    public interface IInputHandler
    {
        bool HandleInput(IntPtr ptr);
    }
}
