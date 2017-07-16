using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace PluginUtils.Injection.Native
{
    public class SimpleLogInjection
    {
        private IntPtr _LastDest, _LastCode;

        public void InjectFunctionPointer(IntPtr pFunction)
        {
            IntPtr pCode;
            using (MemoryStream ms = new MemoryStream())
            {
                using (BinaryWriter bw = new BinaryWriter(ms))
                {
                    WriteCode(bw, pFunction);
                    var assemlyCode = ms.ToArray();
                    pCode = AssemblyCodeStorage.WriteCode(assemlyCode);
                }
            }
            CodeModification.WritePointer(pFunction, pCode);
            _LastDest = pFunction;
            _LastCode = pCode;
        }

        public void Reinject()
        {
            CodeModification.WritePointer(_LastDest, _LastCode);
        }

        private void WriteCode(BinaryWriter bw, IntPtr pFunction)
        {
            var index = NativeEntrance.NextIndex();
            NativeEntrance.Register(index, WrappedEntrance);

            IntPtr original = Marshal.ReadIntPtr(pFunction);

            IntPtr pJumpBack = AssemblyCodeStorage.AllocateIndirect();
            AssemblyCodeStorage.WriteIndirect(pJumpBack, original);
            //push ebp
            //push eax
            //mov eax, esp
            //add eax, 4
            //push eax
            //push index
            //mov eax, entrance
            //call eax
            //pop eax
            //jmp original

            //push ebp
            bw.Write((byte)0x55);
            //push eax
            bw.Write((byte)0x50);
            //mov eax, esp
            bw.Write((byte)0x89);
            bw.Write((byte)0xE0);
            //add eax, 4
            bw.Write((byte)0x83);
            bw.Write((byte)0xC0);
            bw.Write((byte)0x04);
            //push eax
            bw.Write((byte)0x50);
            //push index
            bw.Write((byte)0x68);
            bw.Write((int)index);
            //mov eax, entrance
            bw.Write((byte)0xB8);
            bw.Write(NativeEntrance.EntrancePtr.ToInt32());
            //call eax
            bw.Write((byte)0xFF);
            bw.Write((byte)0xD0);
            //pop eax
            bw.Write((byte)0x58);
            //pop ebp
            bw.Write((byte)0x5D);
            //jmp original
            bw.Write((byte)0xFF);
            bw.Write((byte)0x25);
            bw.Write(pJumpBack.ToInt32());
        }

        private IntPtr _Ptr;
        private void WrappedEntrance(IntPtr ptr)
        {
            IntPtr last = _Ptr;
            _Ptr = ptr;
            Triggered();
            _Ptr = last;
        }

        protected virtual void Triggered()
        {
        }

        protected List<string> GetRelativeAddr(List<IntPtr> addr)
        {
            var b = AddressHelper.CodeOffset(0).ToInt32();
            List<string> ret = new List<string>();
            for (int i = 0; i < addr.Count; ++i)
            {
                var a = addr[i].ToInt32();
                if (a < b || a > b + 0x210000)
                {
                    ret.Add("00000000");
                }
                else
                {
                    ret.Add((a - b).ToString("X8"));
                }
            }
            return ret;
        }

        protected List<IntPtr> GetStackTrace(int depth)
        {
            List<IntPtr> addr = new List<IntPtr>();
            IntPtr ebp = _Ptr;
            for (int i = 0; i < depth - 1; ++i)
            {
                addr.Add(Marshal.ReadIntPtr(ebp, 4));
                ebp = Marshal.ReadIntPtr(ebp);
            }
            addr.Add(Marshal.ReadIntPtr(ebp, 4));
            return addr;
        }

        protected string GetCallingPoint()
        {
            IntPtr ebp = _Ptr;
            int b = AddressHelper.CodeOffset(0).ToInt32();
            int a = Marshal.ReadIntPtr(ebp, 4).ToInt32();
            while (a < b || a > b + 0x210000)
            {
                ebp = Marshal.ReadIntPtr(ebp);
                a = Marshal.ReadIntPtr(ebp, 4).ToInt32();
            }
            return (a - b).ToString("X8");
        }
    }
}
