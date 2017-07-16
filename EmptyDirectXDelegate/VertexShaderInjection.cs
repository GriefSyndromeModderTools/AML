﻿using PluginUtils.Injection.Direct3D;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace EmptyDirectXDelegate
{
    class VertexShaderInjection
    {
        public static IntPtr Instance
        {
            get
            {
                return _Com.Instance;
            }
        }

        private static ComInterfaceGenerator _Com =
            new ComInterfaceGenerator(typeof(ComFunctions));

        [ComClass(5)]
        private class ComFunctions
        {
            private static Guid _Guid = new Guid("EFC5557E-6265-4613-8A94-43857889EB36");

            [ComMethodAttribute(0)]
            public static int QueryInterface(IntPtr ptr, IntPtr pGuid, IntPtr ret)
            {
                byte[] d = new byte[16];
                Marshal.Copy(pGuid, d, 0, 16);
                if (new Guid(d) == _Guid)
                {
                    Marshal.WriteIntPtr(ret, ptr);
                    return 0;
                }
                return unchecked((int)0x80004002);
            }

            [ComMethodAttribute(1)]
            public static int AddRef(IntPtr ptr)
            {
                return 4;
            }

            [ComMethodAttribute(2)]
            public static int Release(IntPtr ptr)
            {
                return 3;
            }

            [ComMethodAttribute(3)]
            public static int GetDevice(IntPtr ptr, IntPtr ret)
            {
                Marshal.WriteIntPtr(ret, Direct3DHelper.Device); //const ref
                return 0;
            }

            [ComMethodAttribute(4)]
            public static int GetFunc(IntPtr ptr, IntPtr buffer, IntPtr len)
            {
                Marshal.WriteInt32(len, 0);
                return 0;
            }
        }
    }
}
