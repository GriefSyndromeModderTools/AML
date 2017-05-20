using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace DirectxAPIRecorder
{
    class Recorder
    {
        private BinaryWriter bw;
        private Dictionary<IntPtr, int> _Tex = new Dictionary<IntPtr, int>();
        private IntPtr _Screen;

        public Recorder()
        {
            bw = new BinaryWriter(File.OpenWrite(@"E:\dx.render.dat"),
                Encoding.UTF8, false);
        }

        public void SetScreenTarget(IntPtr surface)
        {
            _Screen = surface;
        }

        public void Clear(int clearOption)
        {
            bw.Write((byte)11);
            bw.Write((byte)clearOption);
        }

        public void CreateTexture(string filename, IntPtr tex)
        {
            bw.Write((byte)2);
            bw.Write(filename);
            _Tex.Add(tex, _Tex.Count); //TODO duplicate texture
        }

        public void SetTexture(IntPtr tex)
        {
            int id;
            IntPtr surface;
            if (!_Tex.TryGetValue(tex, out id))
            {
                surface = GetSurfaceLevel(tex);
                if (!_Tex.TryGetValue(surface, out id))
                {
                    id = _Tex.Count;
                    bw.Write((byte)3);
                    _Tex.Add(surface, id); //assume it is a target
                }
            }
            bw.Write((byte)4);
            bw.Write(id);
        }

        public void SetRenderState(int a, int b)
        {
            bw.Write((byte)5);
            bw.Write(a);
            bw.Write(b);
        }

        public void SetSamplerState(int a, int b)
        {
            bw.Write((byte)6);
            bw.Write(a);
            bw.Write(b);
        }

        public void SetTarget(IntPtr surface)
        {
            int id;
            if (surface == _Screen)
            {
                bw.Write((byte)10);
            }
            else
            {
                if (!_Tex.TryGetValue(surface, out id))
                {
                    id = _Tex.Count;
                    bw.Write((byte)3);
                    _Tex.Add(surface, id);
                }
                bw.Write((byte)7);
                bw.Write(id);
            }
        }

        private byte[] _DrawData = new byte[4 * 7 * 4];

        public void DrawUP(IntPtr data)
        {
            Marshal.Copy(data, _DrawData, 0, _DrawData.Length);
            bw.Write((byte)8);
            bw.Write(_DrawData);
        }

        public void NewFrame()
        {
            bw.Write((byte)9);
            bw.Flush();
        }

        public void BeginBlock()
        {

        }

        public void EndBlock()
        {

        }

        public void SetDefaultPixelShader()
        {
            bw.Write((byte)12);
        }

        public void SetPixelShader(int index)
        {
            bw.Write((byte)13);
            bw.Write(index);
        }

        private delegate int GetSurfaceLevelDelegate(IntPtr p0, int p1, out IntPtr p2);

        private static Dictionary<IntPtr, IntPtr> _SurfaceCache = new Dictionary<IntPtr, IntPtr>();
        public static IntPtr GetSurfaceLevel(IntPtr t)
        {
            if (t == IntPtr.Zero) return t;
            IntPtr ret;
            if (_SurfaceCache.TryGetValue(t, out ret))
            {
                return ret;
            }

            IntPtr vtab = Marshal.ReadIntPtr(t);
            IntPtr f = Marshal.ReadIntPtr(vtab, 18 * 4);
            var d = Marshal.GetDelegateForFunctionPointer(f, typeof(GetSurfaceLevelDelegate));
            var td = (GetSurfaceLevelDelegate)d;
            td(t, 0, out ret);
            _SurfaceCache[t] = ret;
            return ret;
        }
    }
}
