using PluginUtils.Injection.Native;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace EmptyDirectXDelegate
{
    class EmptyCallInjection
    {
        //private delegate int GetDeviceCapsDelegate(IntPtr ptr,
        //    [Out][MarshalAs(UnmanagedType.LPArray, SizeConst = 304)] byte[] ret);

        public static void Inject(IntPtr device)
        {
            //var x = Marshal.ReadIntPtr(AddressHelper.VirtualTable(device, 7));
            //var f = (GetDeviceCapsDelegate)Marshal.GetDelegateForFunctionPointer(x, typeof(GetDeviceCapsDelegate));
            //byte[] data = new byte[304];
            //f(device, data);
            //System.IO.File.WriteAllBytes(@"E:\devicecap.dat", data);
            //
            ComInterfaceGenerator.InjectObject(device, typeof(DeviceFunctions));
        }

        private static ComInterfaceGenerator _Com = new ComInterfaceGenerator(typeof(DeviceFunctions));

        public static IntPtr Instance { get { return _Com.Instance; } }

        private static readonly byte[] _DeviceCap = new byte[]
        {
        #region BinaryData
            0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 
            0x00, 0x00, 0x02, 0x00, 0x00, 0x00, 0x02, 0xe0,
            0x20, 0x03, 0x00, 0x00, 0x0f, 0x00, 0x00, 0x80, 
            0x01, 0x00, 0x00, 0x00, 0x50, 0xae, 0x19, 0x00,
            0xf2, 0xcc, 0x2f, 0x00, 0x91, 0x61, 0x73, 0x0f, 
            0xff, 0x00, 0x00, 0x00, 0xff, 0x3f, 0x00, 0x00,
            0xff, 0x27, 0x00, 0x00, 0xff, 0x00, 0x00, 0x00, 
            0x08, 0x42, 0x08, 0x00, 0x45, 0xec, 0x21, 0x00,
            0x00, 0x07, 0x03, 0x07, 0x00, 0x07, 0x03, 0x07, 
            0x00, 0x07, 0x03, 0x07, 0x3f, 0x00, 0x00, 0x00,
            0x3f, 0x00, 0x00, 0x00, 0x1f, 0x00, 0x00, 0x00, 
            0x00, 0x20, 0x00, 0x00, 0x00, 0x20, 0x00, 0x00,
            0x00, 0x08, 0x00, 0x00, 0x00, 0x20, 0x00, 0x00, 
            0x00, 0x20, 0x00, 0x00, 0xa7, 0x00, 0x00, 0x00,
            0xf9, 0x02, 0x15, 0x50, 0x00, 0x00, 0x80, 0xc6, 
            0x00, 0x00, 0x80, 0xc6, 0x00, 0x00, 0x80, 0x46,
            0x00, 0x00, 0x80, 0x46, 0x00, 0x00, 0x00, 0x00, 
            0xff, 0x01, 0x00, 0x00, 0x08, 0x00, 0x18, 0x00,
            0xff, 0xff, 0xff, 0x03, 0x08, 0x00, 0x00, 0x00, 
            0x08, 0x00, 0x00, 0x00, 0x7b, 0x01, 0x00, 0x00,
            0x0a, 0x00, 0x00, 0x00, 0x06, 0x00, 0x00, 0x00, 
            0x04, 0x00, 0x00, 0x00, 0x08, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x80, 0x43, 0xff, 0xff, 0x7f, 0x00, 
            0xff, 0xff, 0xff, 0x00, 0x10, 0x00, 0x00, 0x00,
            0xff, 0x00, 0x00, 0x00, 0x00, 0x03, 0xfe, 0xff, 
            0x00, 0x01, 0x00, 0x00, 0x00, 0x03, 0xff, 0xff,
            0xff, 0xff, 0x7f, 0x7f, 0x51, 0x00, 0x00, 0x00, 
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 
            0x02, 0x00, 0x00, 0x00, 0x7f, 0x03, 0x00, 0x00,
            0x04, 0x00, 0x00, 0x00, 0x00, 0x03, 0x00, 0x03, 
            0x01, 0x00, 0x00, 0x00, 0x18, 0x00, 0x00, 0x00,
            0x20, 0x00, 0x00, 0x00, 0x04, 0x00, 0x00, 0x00, 
            0x1f, 0x00, 0x00, 0x00, 0x18, 0x00, 0x00, 0x00,
            0x20, 0x00, 0x00, 0x00, 0x04, 0x00, 0x00, 0x00, 
            0x00, 0x02, 0x00, 0x00, 0x00, 0x03, 0x03, 0x03,
            0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 
            0x00, 0x80, 0x00, 0x00, 0x00, 0x80, 0x00, 0x00,
        #endregion
        };

        private class DeviceFunctions
        {
            private static Guid _Device = new Guid("d0223b96-bf7a-43fd-92bd-a43b0d82b9eb");
            private static byte[] _Data = new byte[16];
            [ComMethod(0)]
            public static int QueryInterface(IntPtr ptr, IntPtr guid, IntPtr ret)
            {
                Marshal.Copy(guid, _Data, 0, 16);
                if (new Guid(_Data) != _Device)
                {
                    Marshal.WriteInt32(ret, 0);
                    return unchecked((int)0x80004002);
                }
                Marshal.WriteIntPtr(ret, ptr);
                return 0;
            }
            [ComMethod(1)]
            public static int AddRef(IntPtr ptr)
            {
                return 10;
            }
            [ComMethod(2)]
            public static int Release(IntPtr ptr)
            {
                return 9;
            }
            [ComMethod(3)]
            public static int TestCooperativeLevel(IntPtr ptr)
            {
                return 0;
            }
            [ComMethod(6)]
            public static int GetDirect3D(IntPtr ptr, IntPtr ret)
            {
                Marshal.WriteIntPtr(ret, D3DInjection.Instance);
                return 0;
            }
            [ComMethod(7)]
            public static int GetDeviceCaps(IntPtr ptr, IntPtr ret)
            {
                Marshal.Copy(_DeviceCap, 0, ret, _DeviceCap.Length);
                return 0;
            }
            [ComMethod(8)]
            public static int GetDisplayMode(IntPtr ptr, int i, IntPtr ret)
            {
                Marshal.WriteInt32(ret, 0, 1024);
                Marshal.WriteInt32(ret, 4, 768);
                Marshal.WriteInt32(ret, 8, 0);
                Marshal.WriteInt32(ret, 12, 22);
                return 0;
            }
            [ComMethod(14)]
            public static int GetSwapChain(IntPtr ptr, int i, IntPtr ret)
            {
                Marshal.WriteIntPtr(ret, SwapChainInjection.Instance); //const ref
                return 0;
            }
            private static IntPtr _BackBuffer =
                SurfaceInjection.Create(new int[] { 21, 1, 0, 0, 0, 0, 800, 600}, 100);
            [ComMethod(18)]
            public static int GetBackBuffer(IntPtr ptr, int a, int b, int c, IntPtr ret)
            {
                Marshal.AddRef(_BackBuffer);
                Marshal.WriteIntPtr(ret, _BackBuffer);
                return 0;
            }
            [ComMethod(23)]
            public static int CreateTexture(IntPtr ptr, int w, int h, int level, int usage, int format, int pool, IntPtr ret, IntPtr handle)
            {
                var desc = new int[]
                {
                    format, 3, usage, pool,
                    0, 0, w, h,
                };
                Marshal.WriteIntPtr(ret, TextureInjection.Create(desc));
                return 0;
            }
            [ComMethod(26)]
            public static int CreateVertexBuffer(IntPtr ptr, int a, int b, int c, int d, IntPtr ret, IntPtr e)
            {
                Marshal.WriteIntPtr(ret, BufferInjection.Instance);
                return 0;
            }
            [ComMethod(27)]
            public static int CreateIndexBuffer(IntPtr ptr, int a, int b, int c, int d, IntPtr ret, IntPtr e)
            {
                Marshal.WriteIntPtr(ret, BufferInjection.Instance);
                return 0;
            }
            [ComMethod(34)]
            public static int StretchRect(IntPtr ptr, IntPtr a, IntPtr ra, IntPtr b, IntPtr rb, int f)
            {
                return 0;
            }
            private static IntPtr _Target = _BackBuffer;
            [ComMethod(37)]
            public static int SetRenderTarget(IntPtr ptr, int i, IntPtr v)
            {
                if (_Target != v)
                {
                    Marshal.Release(_Target);
                    Marshal.AddRef(v);
                }
                _Target = v;
                return 0;
            }
            [ComMethod(38)]
            public static int GetRenderTarget(IntPtr ptr, int i, IntPtr v)
            {
                Marshal.AddRef(_Target);
                Marshal.WriteIntPtr(v, _Target);
                return 0;
            }
            private static IntPtr _Depth =
                SurfaceInjection.Create(new int[] { 75, 1, 0, 0, 0, 0, 800, 600 }, 100);
            [ComMethod(39)]
            public static int SetDepthStencilSurface(IntPtr ptr, IntPtr v)
            {
                if (_Depth != v)
                {
                    Marshal.Release(_Depth);
                    Marshal.AddRef(v);
                }
                _Depth = v;
                return 0;
            }
            [ComMethod(40)]
            public static int GetDepthStencilSurface(IntPtr ptr, IntPtr v)
            {
                Marshal.AddRef(_Depth);
                Marshal.WriteIntPtr(v, _Depth);
                return 0;
            }
            [ComMethod(41)]
            public static int BeginScene(IntPtr ptr)
            {
                return 0;
            }
            [ComMethod(42)]
            public static int EndScene(IntPtr ptr)
            {
                return 0;
            }
            [ComMethod(43)]
            public static int Clear(IntPtr p0, int p1, IntPtr p2, int p3, int p4, int p5, int p6)
            {
                return 0;
            }
            private static Dictionary<int, int> _RenderState = new Dictionary<int, int>();
            [ComMethod(57)]
            public static int SetRenderState(IntPtr ptr, int a, int b)
            {
                _RenderState[a] = b;
                return 0;
            }
            [ComMethod(58)]
            public static int GetRenderState(IntPtr ptr, int a, IntPtr b)
            {
                int ret = 0;
                _RenderState.TryGetValue(a, out ret);
                Marshal.WriteInt32(b, ret);
                return 0;
            }
            [ComMethodAttribute(60)]
            public static int BeginStateBlock(IntPtr p0)
            {
                return 0;
            }
            [ComMethodAttribute(61)]
            public static int EndStateBlock(IntPtr p0, IntPtr p1)
            {
                Marshal.WriteIntPtr(p1, StateBlockInjection.Instance);
                return 0;
            }
            [ComMethod(65)]
            public static int SetTexture(IntPtr ptr, int i, IntPtr obj)
            {
                return 0;
            }
            [ComMethod(67)]
            public static int SetTextureStageState(IntPtr ptr, int i, int a, int b)
            {
                return 0;
            }
            private static Dictionary<int, int> _SamplerState = new Dictionary<int, int>();
            [ComMethod(68)]
            public static int GetSamplerState(IntPtr ptr, int i, int a, IntPtr b)
            {
                int ret = 0;
                _SamplerState.TryGetValue(a, out ret);
                Marshal.WriteInt32(b, ret);
                return 0;
            }
            [ComMethod(69)]
            public static int SetSamplerState(IntPtr ptr, int i, int a, int b)
            {
                _SamplerState[a] = b;
                return 0;
            }
            [ComMethod(82)]
            public static int DrawIndexedPrimitive(IntPtr ptr, int i1, int i2, int i3, int i4, int i5, int i6)
            {
                return 0;
            }
            [ComMethod(83)]
            public static int DrawPrimitiveUP(IntPtr ptr, int p1, int p2, IntPtr p3, int p4)
            {
                return 0;
            }
            [ComMethod(86)]
            public static int CreateVertexDeclaration(IntPtr ptr, IntPtr data, IntPtr ret)
            {
                Marshal.WriteInt32(ret, 1);
                return 0;
            }
            [ComMethod(87)]
            public static int SetVertexDeclaration(IntPtr ptr, IntPtr obj)
            {
                return 0;
            }
            [ComMethod(89)]
            public static int SetFVF(IntPtr ptr, int val)
            {
                return 0;
            }
            [ComMethod(91)]
            public static int CreateVertexShader(IntPtr ptr, IntPtr data, IntPtr ret)
            {
                Marshal.WriteIntPtr(ret, VertexShaderInjection.Instance);
                return 0;
            }
            [ComMethod(92)]
            public static int SetVertexShader(IntPtr ptr, IntPtr obj)
            {
                return 0;
            }
            [ComMethod(94)]
            public static int SetVertexShaderConstantF(IntPtr ptr, int i, IntPtr obj, int n)
            {
                return 0;
            }
            [ComMethod(100)]
            public static int SetStreamSource(IntPtr ptr, int n, IntPtr st, int offset, int stride)
            {
                return 0;
            }
            [ComMethod(104)]
            public static int SetIndices(IntPtr ptr, IntPtr obj)
            {
                return 0;
            }
            [ComMethod(106)]
            public static int CreatePixelShader(IntPtr ptr, IntPtr data, IntPtr ret)
            {
                Marshal.WriteIntPtr(ret, PixelShaderInjection.Create());
                return 0;
            }
            [ComMethod(107)]
            public static int SetPixelShader(IntPtr ptr, IntPtr obj)
            {
                return 0;
            }
            [ComMethod(109)]
            public static int SetPixelShaderConstantF(IntPtr ptr, int i, IntPtr obj, int num)
            {
                return 0;
            }
        }
    }
}
