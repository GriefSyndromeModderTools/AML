using PluginUtils;
using PluginUtils.Injection.Direct3D;
using PluginUtils.Injection.Native;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace DirectxAPIRecorder
{
    [Plugin(Name = "DirectxAPIRecorder", RawVersion = "0.1")]
    public class DirectxAPIRecorderPlugin : IAMLPlugin
    {
        private class Logger
        {
            //private StreamWriter _Log = new StreamWriter("E:\\dx.log", true);
            
            public void WriteLine(string str, params object[] args)
            {
                //lock (_Log)
                //{
                //    _Log.WriteLine(str, args);
                //    _Log.Flush();
                //}
            }

            public void Flush()
            {
            }
        }

        public void Init()
        {
        }

        public void Load()
        {
            WindowsHelper.MessageBox("record");
            Direct3DHelper.InjectDevice(InjectDevice);
            new BeforeCreateTextureInjection().InjectSelf();
            new AfterCreateTextureInjection().InjectSelf();
        }

        private static Action _Reinject;

        private delegate int GetRenderTargetDelegate(IntPtr device, int index, out IntPtr ret);
        private static void InjectDevice(IntPtr device)
        {
            for (int i = 0; i < 119; ++i)
            {
                //new MethodIndexInjection().InjectSelf(device, i);
            }
            var pf = Marshal.ReadIntPtr(AddressHelper.VirtualTable(device, 38));
            var f = (GetRenderTargetDelegate)Marshal.GetDelegateForFunctionPointer(pf, typeof(GetRenderTargetDelegate));
            IntPtr target;
            f(device, 0, out target);
            _Rec.SetScreenTarget(target);
            new SetTextureInjection().InjectSelf(device);
            new DrawInjection().InjectSelf(device);
            new SetRenderTargetInjection().InjectSelf(device);
            new SetRenderStateInjection().InjectSelf(device);
            new ClearInjection().InjectSelf(device);
            new SetSamplerStateInjection().InjectSelf(device);
            new CreatePixelShader().InjectSelf(device);
            //new CreateEffect().InjectSelf();
            new SetPS().InjectSelf(device);
            //new BeginStateBlockInjection().InjectSelf(device);
            //new EndStateBlockInjection().InjectSelf(device);
            new DeviceSwapchainPresentInjection().InjectDevice(device);
            new SetTextureStageStateInjection().InjectSelf(device);
            //new CreateTextureInjection().InjectSelf(device);
        }

        private static Logger _Log = new Logger();
        private static Recorder _Rec = new Recorder();

        private class MethodIndexInjection : SimpleLogInjection
        {
            public int Index;

            public void InjectSelf(IntPtr obj, int index)
            {
                Index = index;
                InjectFunctionPointer(AddressHelper.VirtualTable(obj, index));
                _Reinject += Reinject;
            }

            protected override void Triggered()
            {
                _Log.WriteLine(Index.ToString());
            }
        }

        private static IntPtr _pTexture;
        private static IntPtr _pFileName;
        
        private class BeforeCreateTextureInjection : NativeWrapper
        {
            public void InjectSelf()
            {
                this.AddRegisterRead(Register.EAX);
                this.AddRegisterRead(Register.EBP);
                this.InjectBefore(AddressHelper.CodeOffset(0xB2E49), 8);
            }
            
            protected override void Triggered(NativeWrapper.NativeEnvironment env)
            {
                _pTexture = env.GetRegister(Register.EAX);
                _pFileName = Marshal.ReadIntPtr(env.GetRegister(Register.EBP) + 0xC);
            }
        }

        private class AfterCreateTextureInjection : NativeWrapper
        {

            public void InjectSelf()
            {
                this.AddRegisterRead(Register.EAX);
                this.AddRegisterRead(Register.EBP);
                this.InjectBefore(AddressHelper.CodeOffset(0xB2F01), 8);
            }

            private static string GamePath = PathHelper.GetPath("").Replace('\\', '/');

            protected override void Triggered(NativeWrapper.NativeEnvironment env)
            {
                var filename = Marshal.PtrToStringAnsi(_pFileName);
                var texture = Marshal.ReadIntPtr(_pTexture);

                if (filename.StartsWith(GamePath))
                {
                    filename = filename.Substring(GamePath.Length + 1);
                }
                if (filename.StartsWith("./"))
                {
                    filename = filename.Substring(2);
                }
                _Rec.CreateTexture(filename, texture);
            }

            private delegate int GetSurfaceLevelDelegate(IntPtr p0, int p1, ref IntPtr p2);

            public static IntPtr GetSurfaceLevel(IntPtr t)
            {
                if (t == IntPtr.Zero) return t;
                IntPtr vtab = Marshal.ReadIntPtr(t);
                IntPtr f = Marshal.ReadIntPtr(vtab, 18 * 4);
                var d = Marshal.GetDelegateForFunctionPointer(f, typeof(GetSurfaceLevelDelegate));
                var td = (GetSurfaceLevelDelegate)d;
                IntPtr ret = IntPtr.Zero;
                td(t, 0, ref ret);
                return ret;
            }

            private delegate int GetLevelDescDelegate(IntPtr p0, int p1, IntPtr p2);

            private static IntPtr _Desc = Marshal.AllocHGlobal(4 * 8);

            public static void GetSize(IntPtr t)
            {
                IntPtr vtab = Marshal.ReadIntPtr(t);
                IntPtr f = Marshal.ReadIntPtr(vtab, 17 * 4);
                var d = Marshal.GetDelegateForFunctionPointer(f, typeof(GetLevelDescDelegate));
                var td = (GetLevelDescDelegate)d;
                td(t, 0, _Desc);
                int x = Marshal.ReadInt32(_Desc, 4 * 6);
                int y = Marshal.ReadInt32(_Desc, 4 * 7);
            }
        }

        private class SetTextureInjection : NativeWrapper
        {
            private delegate int SetTextureDelegate(IntPtr p0, int p1, IntPtr p2);
            private SetTextureDelegate _Original;

            public void InjectSelf(IntPtr d)
            {
                this.AddRegisterRead(Register.EAX);
                this.AddRegisterRead(Register.EBP);
                _Original = InjectFunctionPointer<SetTextureDelegate>(AddressHelper.VirtualTable(d, 65), 12);
                _Reinject += this.ReinjectFunctionPointer;
            }

            protected override void Triggered(NativeWrapper.NativeEnvironment env)
            {
                var p0 = env.GetParameterP(0);
                var p1 = env.GetParameterI(1);
                var p2 = env.GetParameterP(2);
                env.SetReturnValue(_Original(p0, p1, p2));
                _Rec.SetTexture(p2);
            }
        }

        private class DrawInjection : NativeWrapper
        {
            private delegate int DrawPrimitiveUPDelegate(IntPtr p0, int p1, int p2, IntPtr p3, int p4);
            private DrawPrimitiveUPDelegate _Original;

            public void InjectSelf(IntPtr d)
            {
                this.AddRegisterRead(Register.EAX);
                this.AddRegisterRead(Register.EBP);
                _Original = this.InjectFunctionPointer<DrawPrimitiveUPDelegate>(AddressHelper.VirtualTable(d, 83), 20);
                _Reinject += this.ReinjectFunctionPointer;
            }

            protected override void Triggered(NativeWrapper.NativeEnvironment env)
            {
                var p0 = env.GetParameterP(0);
                var p1 = env.GetParameterI(1);
                var p2 = env.GetParameterI(2);
                var p3 = env.GetParameterP(3);
                var p4 = env.GetParameterI(4);
                env.SetReturnValue(_Original(p0, p1, p2, p3, p4));
                env.SetReturnValue(0);
                _Rec.DrawUP(p3);
            }
        }

        private class SetRenderTargetInjection : NativeWrapper
        {
            private delegate int SetRenderTargetDelegate(IntPtr p0, int p1, IntPtr p2);
            private SetRenderTargetDelegate _Original;

            public void InjectSelf(IntPtr d)
            {
                this.AddRegisterRead(Register.EAX);
                this.AddRegisterRead(Register.EBP);
                _Original = InjectFunctionPointer<SetRenderTargetDelegate>(AddressHelper.VirtualTable(d, 37), 12);
                _Reinject += this.ReinjectFunctionPointer;
            }

            protected override void Triggered(NativeWrapper.NativeEnvironment env)
            {
                var p0 = env.GetParameterP(0);
                var p1 = env.GetParameterI(1);
                var p2 = env.GetParameterP(2);
                env.SetReturnValue(_Original(p0, p1, p2));
                _Rec.SetTarget(p2);
            }
        }

        private class ClearInjection : NativeWrapper
        {
            private delegate int ClearDelegate(IntPtr p0, int p1,
                IntPtr p2, int p3, int p4, int p5, int p6);
            private ClearDelegate _Original;

            public void InjectSelf(IntPtr d)
            {
                this.AddRegisterRead(Register.EAX);
                this.AddRegisterRead(Register.EBP);
                _Original = InjectFunctionPointer<ClearDelegate>(AddressHelper.VirtualTable(d, 43), 28);
                _Reinject += this.ReinjectFunctionPointer;
            }

            protected override void Triggered(NativeEnvironment env)
            {
                var p0 = env.GetParameterP(0);
                var p1 = env.GetParameterI(1);
                var p2 = env.GetParameterP(2);
                var p3 = env.GetParameterI(3);
                var p4 = env.GetParameterI(4);
                var p5 = env.GetParameterI(5);
                var p6 = env.GetParameterI(6);
                env.SetReturnValue(_Original(p0, p1, p2, p3, p4, p5, p6));
                _Rec.Clear(p3);
            }
        }

        private class SetRenderStateInjection : NativeWrapper
        {
            private delegate int SetRenderStateDelegate(IntPtr p0, int p1, int p2);
            private SetRenderStateDelegate _Original;

            public void InjectSelf(IntPtr d)
            {
                this.AddRegisterRead(Register.EAX);
                this.AddRegisterRead(Register.EBP);
                _Original = InjectFunctionPointer<SetRenderStateDelegate>(AddressHelper.VirtualTable(d, 57), 12);
                _Reinject += this.ReinjectFunctionPointer;
            }

            protected override void Triggered(NativeWrapper.NativeEnvironment env)
            {
                var p0 = env.GetParameterP(0);
                var p1 = env.GetParameterI(1);
                var p2 = env.GetParameterI(2);
                env.SetReturnValue(_Original(p0, p1, p2));
                _Rec.SetRenderState(p1, p2);
            }
        }

        private class SetSamplerStateInjection : NativeWrapper
        {
            private delegate int SetSamplerStateDelegate(IntPtr p0, int p1, int p2, int p3);
            private SetSamplerStateDelegate _Original;

            public void InjectSelf(IntPtr d)
            {
                this.AddRegisterRead(Register.EAX);
                this.AddRegisterRead(Register.EBP);
                _Original = InjectFunctionPointer<SetSamplerStateDelegate>(AddressHelper.VirtualTable(d, 69), 16);
                _Reinject += this.ReinjectFunctionPointer;
            }

            protected override void Triggered(NativeWrapper.NativeEnvironment env)
            {
                var p0 = env.GetParameterP(0);
                var p1 = env.GetParameterI(1);
                var p2 = env.GetParameterI(2);
                var p3 = env.GetParameterI(3);
                env.SetReturnValue(_Original(p0, p1, p2, p3));
                _Rec.SetSamplerState(p2, p3);
            }
        }

        private static Dictionary<IntPtr, int> _ShaderIndex = new Dictionary<IntPtr, int>();

        private class CreatePixelShader : NativeWrapper
        {
            private delegate int CreatePixelShaderDelegate(IntPtr p0, IntPtr p1, IntPtr p2);
            private CreatePixelShaderDelegate _Original;

            public void InjectSelf(IntPtr d)
            {
                this.AddRegisterRead(Register.EAX);
                this.AddRegisterRead(Register.EBP);
                _Original = InjectFunctionPointer<CreatePixelShaderDelegate>(AddressHelper.VirtualTable(d, 106), 12);
                _Reinject += this.ReinjectFunctionPointer;
            }

            private static int _Index = 0;

            protected override void Triggered(NativeWrapper.NativeEnvironment env)
            {
                var p0 = env.GetParameterP(0);
                var p1 = env.GetParameterP(1);
                var p2 = env.GetParameterP(2);
                env.SetReturnValue(_Original(p0, p1, p2));

                //WriteData(p1);
                _ShaderIndex[Marshal.ReadIntPtr(p2)] = _Index;
                _Index++;
            }

            private void WriteData(IntPtr p1)
            {
                using (var f = File.OpenWrite(@"E:\\ps" + CreateEffect._Index.ToString() + "-" +
                    _Index.ToString() + ".dat"))
                {
                    using (var bw = new BinaryWriter(f))
                    {
                        int i = 0;
                        int data = Marshal.ReadInt32(p1, i++);
                        while (data != 0x0000FFFF)
                        {
                            bw.Write(data);
                            data = Marshal.ReadInt32(p1, i++);
                        }
                    }
                }
            }

            private string Trace(ref IntPtr ebp)
            {
                var ret = Marshal.ReadIntPtr(ebp, 4).ToInt32();
                ebp = Marshal.ReadIntPtr(ebp);
                var b = AddressHelper.CodeOffset(0).ToInt32();
                if (ret < b || ret > b + 0x210000)
                {
                    return "00000000";
                }
                return (ret - b).ToString("X8");

                //usage:
                //var ebp = env.GetRegister(Register.EBP);
                //var t1 = Trace(ref ebp);
                //var t2 = Trace(ref ebp);
                //var t3 = Trace(ref ebp);
                //var t4 = Trace(ref ebp);
                //var t5 = Trace(ref ebp);
                //var t6 = Trace(ref ebp);
                ////if (t1 != "00000000")
                //{
                //    _Log.WriteLine("CreatePixelShader {0}, {1}, {2}, {3}, {4}, {5}", t1, t2, t3, t4, t5, t6);
                //}
            }
        }

        private class CreateEffect : NativeWrapper
        {
            private delegate int CreateEffectDelegate(
                IntPtr p0, IntPtr p1, int p2,
                IntPtr p3, IntPtr p4, int p5,
                IntPtr p6, IntPtr p7, IntPtr p8);
            private CreateEffectDelegate _Original;

            public void InjectSelf()
            {
                this.AddRegisterRead(Register.EAX);
                this.AddRegisterRead(Register.EBP);
                _Original = InjectFunctionPointer<CreateEffectDelegate>(AddressHelper.CodeOffset(0x20E36C), 36);
            }

            public static int _Index;

            protected override void Triggered(NativeWrapper.NativeEnvironment env)
            {
                var p0 = env.GetParameterP(0);
                var p1 = env.GetParameterP(1);
                var p2 = env.GetParameterI(2);
                var p3 = env.GetParameterP(3);
                var p4 = env.GetParameterP(4);
                var p5 = env.GetParameterI(5);
                var p6 = env.GetParameterP(6);
                var p7 = env.GetParameterP(7);
                var p8 = env.GetParameterP(8);
                env.SetReturnValue(_Original(p0, p1, p2, p3, p4, p5, p6, p7, p8));

                var eff = Marshal.ReadIntPtr(p7);
                //var l = new List<int>() { 58, 63, 64, 65, 66, 67 };
                var l = Enumerable.Range(0, 79);
                foreach (var i in l)
                {
                    new MethodIndexInjection { Index = i }.InjectFunctionPointer(
                        AddressHelper.VirtualTable(eff, i));
                }
                //byte[] data = new byte[p2];
                //Marshal.Copy(p1, data, 0, data.Length);
                //File.WriteAllBytes("E:\\effect" + (_Index++).ToString() + ".dat", data);
            }
        }

        private class SetPS : NativeWrapper
        {
            public delegate int SetPixelShaderDelegate(
                IntPtr p0, IntPtr p1);
            public static SetPixelShaderDelegate _Original;
            
            public void InjectSelf(IntPtr d)
            {
                this.AddRegisterRead(Register.EAX);
                this.AddRegisterRead(Register.EBP);
                _Original = InjectFunctionPointer<SetPixelShaderDelegate>(AddressHelper.VirtualTable(d, 107), 8);
                _Reinject += this.ReinjectFunctionPointer;
            }

            protected override void Triggered(NativeWrapper.NativeEnvironment env)
            {
                var p0 = env.GetParameterP(0);
                var p1 = env.GetParameterP(1);
                env.SetReturnValue(_Original(p0, p1));
                //if (p1 == IntPtr.Zero)
                //{
                //    _Log.WriteLine("SetPixelShader {0}", "<null>");
                //}
                //else if (_ShaderIndex.ContainsKey(p1))
                //{
                //    _Log.WriteLine("SetPixelShader {0}", _ShaderIndex[p1].ToString());
                //}
                //else
                //{
                //    _Log.WriteLine("SetPixelShader {0}", "<unknown>");
                //}
                if (p1 == IntPtr.Zero)
                {
                    _Rec.SetDefaultPixelShader();
                }
                else if (_ShaderIndex.ContainsKey(p1))
                {
                    _Rec.SetPixelShader(_ShaderIndex[p1]);
                }
                else
                {
                    _Rec.SetPixelShader(-1);
                }
            }
        }

        private class BeginStateBlockInjection : NativeWrapper
        {
            public delegate int BeginStateBlockDelegate(IntPtr p0);
            public static BeginStateBlockDelegate _Original;

            public void InjectSelf(IntPtr d)
            {
                this.AddRegisterRead(Register.EAX);
                this.AddRegisterRead(Register.EBP);
                _Original = InjectFunctionPointer<BeginStateBlockDelegate>(AddressHelper.VirtualTable(d, 60), 4);
                _Reinject += this.ReinjectFunctionPointer;
            }

            protected override void Triggered(NativeWrapper.NativeEnvironment env)
            {
                var d = env.GetParameterP(0);
                //env.SetReturnValue(0);
                env.SetReturnValue(_Original(d));
                if (_Reinject != null)
                {
                    _Reinject();
                }
                //_Log.WriteLine("BeginBlock");
                _Rec.BeginBlock();
            }
        }

        private class EndStateBlockInjection : NativeWrapper
        {
            public delegate int EndStateBlockDelegate(IntPtr p0, IntPtr p1);
            public static EndStateBlockDelegate _Original;

            public void InjectSelf(IntPtr d)
            {
                this.AddRegisterRead(Register.EAX);
                this.AddRegisterRead(Register.EBP);
                _Original = InjectFunctionPointer<EndStateBlockDelegate>(AddressHelper.VirtualTable(d, 61), 8);
                _Reinject += this.ReinjectFunctionPointer;
            }

            protected override void Triggered(NativeWrapper.NativeEnvironment env)
            {
                var d = env.GetParameterP(0);
                var p0 = env.GetParameterP(0);
                var p1 = env.GetParameterP(1);
                //Marshal.WriteIntPtr(p1, IntPtr.Zero);
                //env.SetReturnValue(0);
                env.SetReturnValue(_Original(p0, p1));
                //_Log.WriteLine("EndBlock");
                _Rec.EndBlock();
            }
        }

        private class DeviceSwapchainPresentInjection : SimpleLogInjection
        {
            public delegate int GetSwapChainDelegate(IntPtr p0, int p1, out IntPtr p2);
            public static GetSwapChainDelegate _Original;

            public void InjectDevice(IntPtr device)
            {
                var f = Marshal.ReadIntPtr(AddressHelper.VirtualTable(device, 14));
                var d = (GetSwapChainDelegate)Marshal.GetDelegateForFunctionPointer(f,
                    typeof(GetSwapChainDelegate));
                IntPtr swapChain;
                d(device, 0, out swapChain);
                this.InjectFunctionPointer(AddressHelper.VirtualTable(swapChain, 3));
            }

            protected override void Triggered()
            {
                _Rec.NewFrame();
            }
        }
        
        private class SetTextureStageStateInjection : NativeWrapper
        {
            private delegate int SetTextureStageStateDelegate(IntPtr p0, int p1, int p2, int p3);
            private SetTextureStageStateDelegate _Original;

            public void InjectSelf(IntPtr d)
            {
                this.AddRegisterRead(Register.EAX);
                this.AddRegisterRead(Register.EBP);
                _Original = InjectFunctionPointer<SetTextureStageStateDelegate>(AddressHelper.VirtualTable(d, 67), 16);
                _Reinject += this.ReinjectFunctionPointer;
            }

            protected override void Triggered(NativeWrapper.NativeEnvironment env)
            {
                var p0 = env.GetParameterP(0);
                var p1 = env.GetParameterI(1);
                var p2 = env.GetParameterI(2);
                var p3 = env.GetParameterI(3);
                //_Log.WriteLine("SetTextureStageState {0}, {1}, {2}", p1, p2, p3);
                env.SetReturnValue(_Original(p0, p1, p2, p3));
                if (p2 != 4 || p3 != 4)
                {
                }
                //env.SetReturnValue(0);
            }
        }

        private class CreateTextureInjection : NativeWrapper
        {
            private delegate int CreateTextureDelegate(IntPtr p0,
                int p1, int p2, int p3, int p4, int p5, int p6,
                IntPtr p7, IntPtr p8);
            private CreateTextureDelegate _Original;

            public void InjectSelf(IntPtr d)
            {
                this.AddRegisterRead(Register.EAX);
                this.AddRegisterRead(Register.EBP);
                _Original = InjectFunctionPointer<CreateTextureDelegate>(AddressHelper.VirtualTable(d, 23), 36);
                _Reinject += this.ReinjectFunctionPointer;
            }

            private static HashSet<int> _Formats = new HashSet<int>();

            protected override void Triggered(NativeWrapper.NativeEnvironment env)
            {
                //WindowsHelper.MessageBox("createtexture");
                var p0 = env.GetParameterP(0);
                var p1 = env.GetParameterI(1);
                var p2 = env.GetParameterI(2);
                var p3 = env.GetParameterI(3);
                var p4 = env.GetParameterI(4);
                var p5 = env.GetParameterI(5);
                var p6 = env.GetParameterI(6);
                var p7 = env.GetParameterP(7);
                var p8 = env.GetParameterP(8);
                env.SetReturnValue(_Original(p0, p1, p2, p3, p4, p5, p6, p7, p8));
                //IntPtr t = Marshal.ReadIntPtr(p7);
                if (_Formats.Add(p5))
                {
                }
            }
        }
    }
}
