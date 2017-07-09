using PluginUtils;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
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
        private GZipStream _zip;
        private FileStream _raw;

        private string _NewFilename, _FileExt;
        private int _NextId = 0;
        private string GetNextFileName()
        {
            return _NewFilename + "_" + (_NextId++).ToString() + _FileExt;
        }

        private int _FrameCount = 0;

        public Recorder()
        {
            for (int i = 0; i < ArgHelper.Count; ++i)
            {
                if (ArgHelper.Get(i) == "/DirectxAPIRecorder:Output")
                {
                    var fullFileName = ArgHelper.Get(i + 1);
                    _NewFilename = Path.ChangeExtension(fullFileName, null);
                    _FileExt = Path.GetExtension(fullFileName);
                    OpenFile();
                    return;
                }
            }
            bw = new BinaryWriter(new MemoryStream());
        }

        private void OpenFile()
        {
            var raw = File.OpenWrite(GetNextFileName());
            var zip = new GZipStream(raw, CompressionMode.Compress, false);
            bw = new BinaryWriter(zip, Encoding.UTF8, false);
            //TODO
            _zip = zip;
            _raw = raw;
        
        }
        private void Flush()
        {
            bw.Flush();
            _zip.Flush();
            _raw.Flush();
        }

        private void SwitchFile()
        {
            bw.Dispose();
            _zip.Dispose();
            _raw.Dispose();
            OpenFile();
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
            _Tex[tex] = _Tex.Count;
            //to release server cpu pressure
            //System.Threading.Thread.Sleep(3);
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

        Dictionary<int, int> _RS = new Dictionary<int, int>();
        Dictionary<int, int> _RSNew = new Dictionary<int, int>();

        Dictionary<int, int> _SS = new Dictionary<int, int>();
        Dictionary<int, int> _SSNew = new Dictionary<int, int>();

        private void FlushState()
        {
            foreach (var e in _RSNew)
            {
                int oldVal;
                if (!_RS.TryGetValue(e.Key, out oldVal) || oldVal != e.Value)
                {
                    _RS[e.Key] = e.Value;
                    bw.Write((byte)5);
                    bw.Write(e.Key);
                    bw.Write(e.Value);
                }
            }
            _RSNew.Clear();
            foreach (var e in _SSNew)
            {
                int oldVal;
                if (!_SS.TryGetValue(e.Key, out oldVal) || oldVal != e.Value)
                {
                    _SS[e.Key] = e.Value;
                    bw.Write((byte)6);
                    bw.Write(e.Key);
                    bw.Write(e.Value);
                }
            }
            _SSNew.Clear();
        }

        public void SetRenderState(int a, int b)
        {
            //bw.Write((byte)5);
            //bw.Write(a);
            //bw.Write(b);
            _RSNew[a] = b;
        }

        public void SetSamplerState(int a, int b)
        {
            //bw.Write((byte)6);
            //bw.Write(a);
            //bw.Write(b);
            _SSNew[a] = b;
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
                    _Tex[surface] = id;
                }
                bw.Write((byte)7);
                bw.Write(id);
            }
        }

        private byte[] _DrawData = new byte[4 * 7 * 4];

        public void DrawUP(IntPtr data)
        {
            FlushState();
            Marshal.Copy(data, _DrawData, 0, _DrawData.Length);
            bw.Write((byte)8);
            bw.Write(_DrawData);
        }

        public void NewFrame()
        {
            bw.Write((byte)9);
            Flush();

            if (((++_FrameCount) & 65535) == 0)
            {
                SwitchFile();
            }
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
