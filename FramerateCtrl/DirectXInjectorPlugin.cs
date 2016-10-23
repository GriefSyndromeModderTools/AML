using PluginUtils;
using PluginUtils.Injection.Direct3D;
using PluginUtils.Injection.Native;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace FramerateCtrl
{
    //much better is achieved by simply remove rendering code
    public class DirectXInjectorPlugin // : IAMLPlugin
    {
        private static IntPtr _SwapChain;

        private static long FrameCount = 0;
        private static bool ShouldSkip()
        {
            return (FrameCount % 1000) != 0;
        }

        public void Init()
        {
            Direct3DHelper.InjectDevice(device =>
            {
                new InjectDrawPrimitiveUP().InjectSelf(device);
                new InjectPresent().InjectSelf(device);
            });
        }

        public void Load()
        {
        }

        private class InjectPresent : NativeWrapper
        {
            private delegate int GetSwapChainDelegate(IntPtr device, int n, IntPtr pOut);
            private delegate int PresentDelegate(IntPtr p0, IntPtr p1, IntPtr p2, int p3, IntPtr p4, int p5);
            private PresentDelegate _Original;

            public InjectPresent()
            {
                this.AddRegisterRead(Register.EAX);
                this.AddRegisterRead(Register.EBP);
            }

            public void InjectSelf(IntPtr d)
            {
                //get swapchain
                IntPtr pVTable = Marshal.ReadIntPtr(d);
                IntPtr pGetSwapChain = Marshal.ReadIntPtr(pVTable, 14 * 4); //#14
                var getSwapChain = (GetSwapChainDelegate)Marshal.GetDelegateForFunctionPointer(pGetSwapChain,
                    typeof(GetSwapChainDelegate));
                IntPtr r = Marshal.AllocHGlobal(4);
                getSwapChain(d, 0, r);
                IntPtr swapChain = Marshal.ReadIntPtr(r);
                Marshal.FreeHGlobal(r);

                _SwapChain = swapChain;

                //#3
                _Original = this.InjectFunctionPointer<PresentDelegate>(AddressHelper.VirtualTable(swapChain, 3), 24);
            }

            protected override void Triggered(NativeWrapper.NativeEnvironment env)
            {
                var p0 = env.GetParameterP(0);
                var p1 = env.GetParameterP(1);
                var p2 = env.GetParameterP(2);
                var p3 = env.GetParameterI(3);
                var p4 = env.GetParameterP(4);
                var p5 = env.GetParameterI(5);

                if (ShouldSkip())
                {
                    env.SetReturnValue(0);
                }
                else
                {
                    env.SetReturnValue(_Original(p0, p1, p2, p3, p4, p5));
                }
                ++FrameCount;
            }
        }

        private class InjectDrawPrimitiveUP : NativeWrapper
        {
            private delegate int DrawPrimitiveUPDelegate(IntPtr p0, int p1, int p2, IntPtr p3, int p4);
            private DrawPrimitiveUPDelegate _Original;

            public InjectDrawPrimitiveUP()
            {
                this.AddRegisterRead(Register.EAX);
                this.AddRegisterRead(Register.EBP);
            }

            public void InjectSelf(IntPtr d)
            {
                _Original = this.InjectFunctionPointer<DrawPrimitiveUPDelegate>(AddressHelper.VirtualTable(d, 83), 20);
            }

            private float[] _Buffer = new float[7 * 4];

            protected override void Triggered(NativeWrapper.NativeEnvironment env)
            {
                if (ShouldSkip())
                {
                    env.SetReturnValue(0);
                }
                else
                {
                    var p0 = env.GetParameterP(0);
                    var p1 = env.GetParameterI(1);
                    var p2 = env.GetParameterI(2);
                    var p3 = env.GetParameterP(3);
                    var p4 = env.GetParameterI(4);
                    env.SetReturnValue(_Original(p0, p1, p2, p3, p4));
                }
            }
        }
    }
}
