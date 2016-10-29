using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Debugger
{
    public partial class DebuggerWindow : Form
    {
        private readonly DebuggerPlugin _plugin;
        private readonly BreakPointSetter _breakPointSetter;
        public readonly Dictionary<string, HashSet<int>> BreakPointMap = new Dictionary<string, HashSet<int>>();

        public enum State
        {
            Running,       // Normally running
            Breaking,      // Breaking or will be breaking next line
            BreakReturning,   // Will be breaking when returning
            BreakAfterReturn,
            StepOver,
        }

        public State DebuggerState { get; private set; } = State.Running;

        private class MessageHandler : IMessageHandler
        {
            private readonly DebuggerWindow _window;

            // Key: SrcName, Value: Content
            private readonly Dictionary<string, string> _sourceMap = new Dictionary<string, string>();

            private string _currentSrcName;
            private int _currentLine;
            private string _currentFuncName;

            public MessageHandler(DebuggerWindow window)
            {
                _window = window;
            }

            private string GetSource(Uri uri, string src)
            {
                string source = null;
                if (uri == null || src == null || _sourceMap.TryGetValue(src, out source))
                    return source;
                Stream stream = null;
                try
                {
                    try
                    {
                        stream = WebRequest.Create(uri + src)
                            .GetResponse()
                            .GetResponseStream();
                    }
                    catch
                    {
                        if (src.EndsWith("nut"))
                        {
                            stream = WebRequest.Create(uri + src.Substring(0, src.Length - 3) + "cv4")
                                .GetResponse()
                                .GetResponseStream();
                        }
                        else
                        {
                            throw;
                        }
                    }
                }
                catch
                {
                    _sourceMap[src] = null;
                }

                if (stream != null)
                {
                    using (var reader = new StreamReader(stream, Encoding.UTF8))
                    {
                        source = reader.ReadToEnd();
                        _sourceMap[src] = source;
                    }
                }

                return source;
            }

            public void DebugInfomationArrived(int type, string srcname, int line, string funcname)
            {
                _currentSrcName = srcname;
                _currentLine = line;
                _currentFuncName = funcname;

                var ctype = (char) type;

                switch (ctype)
                {
                    case 'l':
                        if (_window.DebuggerState == State.Breaking || _window.DebuggerState == State.StepOver)
                        {
                            _window._plugin.SuspendVm();
                            _window.Invoke((Action) (() =>
                            {
                                _window.SetControlEnabled(false);
                            }));
                            
                            ShowLine(srcname, line);
                            break;
                        }
                        
                        HashSet<int> breakPoints;
                        if (_window.BreakPointMap.TryGetValue(_currentSrcName, out breakPoints) && breakPoints != null && breakPoints.Contains(_currentLine))
                        {
                            _window._plugin.SuspendVm();
                            _window.DebuggerState = State.Breaking;
                            _window.Invoke((Action)(() =>
                            {
                                _window.SetControlEnabled(false);
                            }));

                            ShowLine(srcname, line);
                            break;
                        }

                        _window.Invoke((Action)(() =>
                        {
                            _window.SetControlEnabled(true);
                        }));

                        break;
                    case 'c':
                        if (_window.DebuggerState == State.StepOver)
                        {
                            _window.DebuggerState = State.BreakAfterReturn;
                        }
                        break;
                    case 'r':
                        if (_window.DebuggerState == State.BreakReturning)
                        {
                            _window._plugin.SuspendVm();
                            _window.DebuggerState = State.Breaking;
                            _window.Invoke((Action)(() =>
                            {
                                _window.SetControlEnabled(false);
                            }));

                            ShowLine(srcname, line);
                        }
                        else if (_window.DebuggerState == State.BreakAfterReturn)
                        {
                            _window.DebuggerState = State.Breaking;
                        }
                        break;
                }
                
                /*_window.Invoke((Action) (() =>
                {
                    _window.listBox1.Items.Add($"type {(char)type}, source {srcname}, line {line}, function {funcname}");
                }));*/
            }

            private void ShowLine(string srcname, int line)
            {
                var originLine = line;
                --line;
                var uris = _window._plugin.CurrentConfig?.SourceUri;
                if (uris == null)
                {
                    return;
                }

                foreach (var uri in uris)
                {
                    if (uri == null)
                    {
                        continue;
                    }

                    var source = GetSource(uri, srcname);
                    if (source != null)
                    {
                        using (var reader = new StringReader(source))
                        {
                            string linestr = null;
                            while (line-- >= 0)
                            {
                                linestr = reader.ReadLine();
                            }

                            if (linestr != null)
                            {
                                _window.Invoke((Action)(() => { _window.listBox1.Items.Add($"(Source: {uri + srcname} : {originLine}){linestr}"); }));
                            }
                        }

                        return;
                    }
                }
                _window.Invoke((Action)(() => { _window.listBox1.Items.Add($"Could not load source \"{srcname}\" from provided uris"); }));
            }

            public void CompilerExceptionArrived(string exceptiondesc, string srcname, int line, int column)
            {
                _window.Invoke((Action)(() =>
                {
                    ShowLine(srcname, line);
                    _window.listBox1.Items.Add($"Unhandled compiler exception detected, description: {exceptiondesc}, source: {srcname}, line: {line}, column: {column}");
                }));
            }

            public void RuntimeExceptionArrived(string exceptiondesc)
            {
                _window.Invoke((Action) (() =>
                {
                    _window.listBox1.Items.Add($"Unhandled exception detected, description: {exceptiondesc}");
                }));

                _window._plugin.SuspendVm();
                _window.DebuggerState = State.Breaking;
                ShowLine(_currentSrcName, _currentLine);
            }
        }

        public DebuggerWindow(DebuggerPlugin plugin)
        {
            _plugin = plugin;
            _plugin.RegisterMessageHandler(new MessageHandler(this));
            _breakPointSetter = new BreakPointSetter(this);
            InitializeComponent();
        }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            var index = listBox1.SelectedIndex;
            if (index == -1)
            {
                textBox1.Clear();
            }
            else
            {
                textBox1.Text = (string)listBox1.Items[index];
            }
        }

        private void SetControlEnabled(bool value)
        {
            btnPause.Enabled = value;
            btnContinue.Enabled = btnExeToRet.Enabled = btnStepInto.Enabled = btnStepOver.Enabled = !value;
        }

        private void btnPause_Click(object sender, EventArgs e)
        {
            _plugin.SuspendVm();
            DebuggerState = State.Breaking;
            SetControlEnabled(false);
        }
        
        private void btnContinue_Click(object sender, EventArgs e)
        {
            DebuggerState = State.Running;
            _plugin.WakeupVm(true, true, true);
            SetControlEnabled(true);
        }

        private void btnSetBreakPoint_Click(object sender, EventArgs e)
        {
            _breakPointSetter.Show();
        }

        private void btnStepInto_Click(object sender, EventArgs e)
        {
            DebuggerState = State.Breaking;
            _plugin.WakeupVm(true, true, true);
            SetControlEnabled(false);
        }

        private void btnStepOver_Click(object sender, EventArgs e)
        {
            DebuggerState = State.StepOver;
            _plugin.WakeupVm(true, true, true);
            SetControlEnabled(false);
        }

        private void btnExeToRet_Click(object sender, EventArgs e)
        {
            DebuggerState = State.BreakReturning;
            _plugin.WakeupVm(true, true, true);
            SetControlEnabled(false);
        }
    }
}
