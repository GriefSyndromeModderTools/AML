using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace PluginUtils.Injection.Native
{
    public abstract class NativeWrapper
    {
        public enum Register
        {
            EAX,
            EBP,
        }

        public NativeWrapper()
        {
            Log.LoggerManager.NativeInjectorCreated(this);
        }

        public class NativeEnvironment
        {
            private readonly NativeWrapper _Parent;
            private readonly IntPtr _Data;

            public NativeEnvironment(NativeWrapper parent, IntPtr data)
            {
                _Parent = parent;
                _Data = data;
            }

            public IntPtr GetRegister(Register r)
            {
                return (IntPtr)Marshal.ReadInt32(_Data, _Parent._RegisterIndex[(int)r] * 4);
            }

            public void SetRegister(Register r, IntPtr val)
            {
                throw new InvalidOperationException();
            }

            public void SetReturnValue(IntPtr val)
            {
                SetReturnValue(val.ToInt32());
            }

            public void SetReturnValue(int val)
            {
                if (_Parent._ReturnValueIndex == -1)
                {
                    throw new InvalidOperationException();
                }
                Marshal.WriteInt32(_Data, _Parent._ReturnValueIndex * 4, val);
            }

            public IntPtr GetParameterP(int index)
            {
                return (IntPtr)GetParameterI(index);
            }

            public int GetParameterI(int index)
            {
                var ebp = GetRegister(Register.EBP);
                return Marshal.ReadInt32(ebp + 8 + index * 4);
            }
        }

        private int[] _RegisterIndex = new int[] { -1, -1, -1, -1, -1, -1, -1, -1 };
        private int _ReturnValueIndex = -1;
        private int _Count = 0;

        public void AddRegisterRead(Register r)
        {
            var ir = (int)r;
            if (_RegisterIndex[ir] != -1)
            {
                throw new ArgumentException("register already added");
            }
            _RegisterIndex[ir] = _Count++;
        }

        public void InjectBefore(IntPtr addr, int len)
        {
            if (len < 6)
            {
                throw new ArgumentOutOfRangeException();
            }

            ProtectCode(addr, len, false);

            IntPtr pCode;
            IntPtr pJumpForward = AssemblyCodeStorage.AllocateIndirect();
            IntPtr pJumpBackward = AssemblyCodeStorage.AllocateIndirect();

            using (MemoryStream ms = new MemoryStream())
            {
                using (BinaryWriter bw = new BinaryWriter(ms))
                {
                    WriteCode(bw);

                    byte[] moved = new byte[len];
                    Marshal.Copy(addr, moved, 0, len);
                    bw.Write(moved);

                    bw.Write((byte)0xFF);
                    bw.Write((byte)0x25);

                    //pointer to pointer
                    bw.Write(pJumpBackward.ToInt32());

                    var assemlyCode = ms.ToArray();
                    pCode = AssemblyCodeStorage.WriteCode(assemlyCode);
                }
            }

            Marshal.WriteByte(addr, 0, 0xFF);
            Marshal.WriteByte(addr, 1, 0x25);
            var jmpForwardPtr = pJumpForward.ToInt32();
            Marshal.WriteInt32(addr, 2, jmpForwardPtr);

            //finally setup jump table
            AssemblyCodeStorage.WriteIndirect(pJumpForward, pCode);
            AssemblyCodeStorage.WriteIndirect(pJumpBackward, IntPtr.Add(addr, len));

            ProtectCode(addr, len, true);
        }

        //TODO moved should go after injected code. modify InjectGSOLoaded when fixed.
        [Obsolete]
        public void Inject(IntPtr addr, int len)
        {
            if (len < 6)
            {
                throw new ArgumentOutOfRangeException();
            }

            ProtectCode(addr, len, false);

            IntPtr pCode;
            IntPtr pJumpForward = AssemblyCodeStorage.AllocateIndirect();
            IntPtr pJumpBackward = AssemblyCodeStorage.AllocateIndirect();

            using (MemoryStream ms = new MemoryStream())
            {
                using (BinaryWriter bw = new BinaryWriter(ms))
                {
                    byte[] moved = new byte[len];
                    Marshal.Copy(addr, moved, 0, len);
                    bw.Write(moved);

                    WriteCode(bw);

                    bw.Write((byte)0xFF);
                    bw.Write((byte)0x25);

                    //pointer to pointer
                    bw.Write(pJumpBackward.ToInt32());

                    var assemlyCode = ms.ToArray();
                    pCode = AssemblyCodeStorage.WriteCode(assemlyCode);
                    //var checkAssemblyCode = new byte[assemlyCode.Length];
                    //Marshal.Copy(pCode, checkAssemblyCode, 0, checkAssemblyCode.Length);
                }
            }

            Marshal.WriteByte(addr, 0, 0xFF);
            Marshal.WriteByte(addr, 1, 0x25);
            var jmpForwardPtr = pJumpForward.ToInt32();
            Marshal.WriteInt32(addr, 2, jmpForwardPtr);
            //{
            //    var checkPtr = new byte[4];
            //    Marshal.Copy(IntPtr.Add(addr, 4), checkPtr, 0, 4);
            //}

            //finally setup jump table

            AssemblyCodeStorage.WriteIndirect(pJumpForward, pCode);
            AssemblyCodeStorage.WriteIndirect(pJumpBackward, IntPtr.Add(addr, len));

            ProtectCode(addr, len, true);
        }

        private IntPtr _LastFDest, _LastFCode;

        public T InjectFunctionPointer<T>(IntPtr addrFunctionPointer, int ArgSize) where T : class
        {
            _ReturnValueIndex = _Count++;

            var original = Marshal.ReadIntPtr(addrFunctionPointer);
            var ret = (T)(object)Marshal.GetDelegateForFunctionPointer(original, typeof(T));

            IntPtr pCode;

            using (MemoryStream ms = new MemoryStream())
            {
                using (BinaryWriter bw = new BinaryWriter(ms))
                {
                    //push ebp
                    bw.Write((byte)0x55);

                    //mov ebp, esp
                    bw.Write((byte)0x8B);
                    bw.Write((byte)0xEC);

                    WriteCode(bw);

                    //pop ebp
                    bw.Write((byte)0x5D);

                    //retn ?
                    bw.Write((byte)0xC2);
                    bw.Write((short)ArgSize);

                    var assemlyCode = ms.ToArray();
                    pCode = AssemblyCodeStorage.WriteCode(assemlyCode);
                }
            }

            CodeModification.WritePointer(addrFunctionPointer, pCode);
            _LastFDest = addrFunctionPointer;
            _LastFCode = pCode;

            Log.LoggerManager.NativeInjectorInjectedDelegate(addrFunctionPointer, typeof(T));

            return ret;
        }

        public void ReinjectFunctionPointer()
        {
            CodeModification.WritePointer(_LastFDest, _LastFCode);
        }

        private NativeEnvironment GetEnvironment(IntPtr data)
        {
            return new NativeEnvironment(this, data);
        }

        private void ProtectCode(IntPtr addr, int len, bool protect)
        {
            NativeFunctions.Protection p = protect ?
                NativeFunctions.Protection.PAGE_EXECUTE_READ :
                NativeFunctions.Protection.PAGE_EXECUTE_READWRITE;
            NativeFunctions.Protection oldP;
            NativeFunctions.VirtualProtect(addr, (uint)len, p, out oldP);
        }

        protected virtual void WriteCode(BinaryWriter bw)
        {
            var index = NativeEntrance.NextIndex();
            NativeEntrance.Register(index, WrappedNativeCallback);
            /*
             * push eax
             * 
             * sub esp, 4/8/12/...
             * mov [esp+0], eax
             * mov [esp+4], ...
             * 
             * push esp
             * push ???? (index)
             * 
             * mov eax, 0x????????
             * call eax
             * 
             * ;return value
             * mov eax, [esp+?]
             * 
             * add esp, 4/8/12/...
             * 
             * pop eax
             * 
             */

            if (_ReturnValueIndex == -1)
            {
                //push eax
                bw.Write((byte)0x50);
            }

            if (_Count > 0)
            {
                //sub esp, _Count
                bw.Write((byte)0x83);
                bw.Write((byte)0xEC);
                bw.Write((byte)(_Count * 4));
            }

            //save registers
            if (_RegisterIndex[(int)Register.EAX] != -1)
            {
                //mov [esp+?], eax
                var offset = _RegisterIndex[(int)Register.EAX] * 4;
                bw.Write((byte)0x89);
                bw.Write((byte)0x44);
                bw.Write((byte)0x24);
                bw.Write((byte)offset);
            }
            if (_RegisterIndex[(int)Register.EBP] != -1)
            {
                //mov [esp+?], ebp
                var offset = _RegisterIndex[(int)Register.EBP] * 4;
                bw.Write((byte)0x89);
                bw.Write((byte)0x6C);
                bw.Write((byte)0x24);
                bw.Write((byte)offset);
            }

            //push esp
            bw.Write((byte)0x54);

            //push ????????
            bw.Write((byte)0x68);
            bw.Write((int)index);

            //mov eax, ????????
            bw.Write((byte)0xB8);
            bw.Write(NativeEntrance.EntrancePtr.ToInt32());

            //call eax
            bw.Write((byte)0xFF);
            bw.Write((byte)0xD0);

            if (_ReturnValueIndex != -1)
            {
                //mov eax, [esp+?]
                bw.Write((byte)0x8B);
                bw.Write((byte)0x44);
                bw.Write((byte)0x24);
                bw.Write((byte)(_ReturnValueIndex * 4));
            }

            if (_Count > 0)
            {
                //add esp, _Count
                bw.Write((byte)0x83);
                bw.Write((byte)0xC4);
                bw.Write((byte)(_Count * 4));
            }

            if (_ReturnValueIndex == -1)
            {
                //pop eax
                bw.Write((byte)0x58);
            }
        }

        private void WrappedNativeCallback(IntPtr env)
        {
            Triggered(this.GetEnvironment(env));
        }

        protected abstract void Triggered(NativeEnvironment env);
    }
}
