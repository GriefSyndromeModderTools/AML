using PluginUtils;
using PluginUtils.Injection.Input;
using PluginUtils.Injection.SaveData;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace GSORepPlayer
{
    [Plugin(Name = "GSORepPlayer", RawVersion = "0.1")]
    public class GSORepPlayerPlugin : IAMLPlugin, IInputHandler
    {
        private static ushort[] _Rep;
        private static int _RepOffset;

        private static readonly ushort[] _Mask = new ushort[] {
            0x01, 0x02, 0x04, 0x08, 0x10, 0x20, 0x40, 0x80, 0x100
        };

        private bool _AutoExit = false;

        public void Init()
        {
            for (int i = 0; i < ArgHelper.Count; ++i)
            {
                if (ArgHelper.Get(i) == "/GSORepPlayer:AutoExit")
                {
                    _AutoExit = true;
                }
            }
            for (int i = 0; i < ArgHelper.Count; ++i)
            {
                if (ArgHelper.Get(i) == "/GSORepPlayer:RepPath")
                {
                    InitWithPath(ArgHelper.Get(i + 1));
                    return;
                }
            }
            //else
            InitWithDialog();
        }

        public void InitWithDialog()
        {
            WindowsHelper.RunAndWait(delegate()
            {
                try
                {
                    var dialog = new System.Windows.Forms.OpenFileDialog();
                    if (dialog.ShowDialog() != System.Windows.Forms.DialogResult.Cancel)
                    {
                        var rep = new GSO2ReplayFile(dialog.FileName);

                        SaveDataHelper.ModifySaveData += delegate(GSDataFile.CompoundType obj)
                        {
                            obj["lastPlayLap"] = 0;
                            obj["loopNum"] = rep.BaseLap; //TODO
                        };

                        _Rep = rep.InputData;

                        KeyConfigInjector.Inject();

                        InputManager.RegisterHandler(this);
                    }
                }
                catch
                {
                    System.Windows.Forms.MessageBox.Show("Replay error");
                }
            });
        }

        private void InitWithPath(string path)
        {
            try
            {
                if (File.Exists(path))
                {
                    var rep = new GSO2ReplayFile(path);

                    SaveDataHelper.ModifySaveData += delegate(GSDataFile.CompoundType obj)
                    {
                        obj["lastPlayLap"] = 0;
                        obj["loopNum"] = rep.BaseLap; //TODO
                    };

                    _Rep = rep.InputData;

                    KeyConfigInjector.Inject();

                    InputManager.RegisterHandler(this);
                }
            }
            catch
            {
                System.Windows.Forms.MessageBox.Show("Replay error");
            }
        }

        public void Load()
        {
        }

        public bool HandleInput(IntPtr ptr)
        {
            if (_Rep == null)
            {
                return false;
            }
            if (_RepOffset + 2 >= _Rep.Length)
            {
                if (_AutoExit)
                {
                    Environment.Exit(1001);
                }
                _Rep = null;
                return false;
            }
            for (int p = 0; p < 3; p++)
            {
                int playerOffset = _RepOffset + p;
                var pp = p;
                for (int k = 0; k < 9; ++k)
                {
                    var dik = KeyConfigInjector.GetInjectedKeyIndex(pp * 9 + k);
                    if ((_Rep[playerOffset] & _Mask[k]) != 0)
                    {
                        Marshal.WriteByte(ptr, dik, 0x80);
                    }
                    else
                    {
                        Marshal.WriteByte(ptr, dik, 0x0);
                    }
                }
            }

            _RepOffset += 3;
            return true;
        }
    }
}
