using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace PluginUtils.Injection.Native
{
    public class AddressHelper
    {
        public static IntPtr CodeOffset(uint offset)
        {
            //TODO int overflow?
            return IntPtr.Add(NativeFunctions.GetModuleHandle(null), (int)offset);
        }

        public static IntPtr VirtualTable(IntPtr obj, int index)
        {
            IntPtr pTable = Marshal.ReadIntPtr(obj);
            return IntPtr.Add(pTable, 4 * index);
        }
    }
}
