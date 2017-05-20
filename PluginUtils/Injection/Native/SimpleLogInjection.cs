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
            //push eax
            //push 0
            //push index
            //mov eax, entrance
            //call eax
            //pop eax
            //jmp original

            //push eax
            bw.Write((byte)0x50);
            //push 0
            bw.Write((byte)0x68);
            bw.Write((int)0);
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
            //jmp original
            bw.Write((byte)0xFF);
            bw.Write((byte)0x25);
            bw.Write(pJumpBack.ToInt32());
        }

        private void WrappedEntrance(IntPtr ptr)
        {
            Triggered();
        }

        protected virtual void Triggered()
        {
        }
    }
}
