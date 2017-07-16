using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace EmptyDirectXDelegate
{
    class LockedMemoryRegion
    {
        public static readonly IntPtr Ptr = Marshal.AllocHGlobal(16 * 1024 * 1024);
    }
}
