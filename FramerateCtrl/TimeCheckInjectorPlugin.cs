using PluginUtils;
using PluginUtils.Injection.Native;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace FramerateCtrl
{
    class TimeCheckInjectorPlugin : IAMLPlugin
    {
        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool VirtualProtect(IntPtr lpAddress, uint dwSize,
           Protection flNewProtect, out Protection lpflOldProtect);

        public enum Protection
        {
            PAGE_NOACCESS = 0x01,
            PAGE_READONLY = 0x02,
            PAGE_READWRITE = 0x04,
            PAGE_WRITECOPY = 0x08,
            PAGE_EXECUTE = 0x10,
            PAGE_EXECUTE_READ = 0x20,
            PAGE_EXECUTE_READWRITE = 0x40,
            PAGE_EXECUTE_WRITECOPY = 0x80,
            PAGE_GUARD = 0x100,
            PAGE_NOCACHE = 0x200,
            PAGE_WRITECOMBINE = 0x400
        }

        public void Init()
        {
            Protection oldP;
            //disable time check at the startup
            {
                IntPtr time = AddressHelper.CodeOffset(0xD252B + 6);//(later modify this)0x2AC51C
                VirtualProtect(time, 4, Protection.PAGE_EXECUTE_READWRITE, out oldP);
                Marshal.WriteInt32(time, 0);
            }

            //show fps on title (very small effect on speed)
            PluginUtils.Injection.Direct3D.Direct3DHelper.InjectDevice(d =>
            {
                IntPtr fpx = AddressHelper.CodeOffset(0x2AC11E);
                VirtualProtect(fpx, 1, Protection.PAGE_EXECUTE_READWRITE, out oldP);
                Marshal.WriteByte(fpx, 1);
            });

            //remove render code (set to nop)
            {
                IntPtr render = AddressHelper.CodeOffset(0xC613A);
                int len = 0xC61BD - 0xC613A;
                VirtualProtect(render, (uint)len, Protection.PAGE_EXECUTE_READWRITE, out oldP);
                byte[] data = new byte[len];
                for (int i = 0; i < len; ++i) data[i] = 0x90;
                Marshal.Copy(data, 0, render, len);
            }

            //PlaySE
            {
                //no effect
                IntPtr playSE = AddressHelper.CodeOffset(0x58230);
                VirtualProtect(playSE, 3, Protection.PAGE_EXECUTE_READWRITE, out oldP);
                Marshal.WriteInt32(playSE, 0xC3C033);//xor eax,eax ret
            }
            //BitBlt
            {
                IntPtr bitBlt = AddressHelper.CodeOffset(0xF1CC0);
                VirtualProtect(bitBlt, 3, Protection.PAGE_EXECUTE_READWRITE, out oldP);
                Marshal.WriteInt32(bitBlt, 0xC3C033);//xor eax,eax ret
            }
            //StretchBlt
            {
                IntPtr stretchBlt = AddressHelper.CodeOffset(0xF1F00);
                VirtualProtect(stretchBlt, 3, Protection.PAGE_EXECUTE_READWRITE, out oldP);
                Marshal.WriteInt32(stretchBlt, 0xC3C033);//xor eax,eax ret
            }
        }

        public void Load()
        {
        }
    }
}
