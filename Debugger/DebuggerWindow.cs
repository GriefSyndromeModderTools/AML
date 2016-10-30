using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Debugger
{
    public partial class DebuggerWindow : Form
    {
        private readonly BreakPointSetter _breakPointSetter;
        private readonly Interactive _interactive;
        public readonly Dictionary<string, HashSet<int>> BreakPointMap = new Dictionary<string, HashSet<int>>();

        public enum State
        {
            Running,       // Normally running
            Breaking,      // Breaking or will be breaking next line
            BreakReturning,   // Will be breaking when returning
            BreakAfterReturn,
            StepOver,
        }

        private volatile State _debuggerState = State.Running;

        public State DebuggerState
        {
            get
            {
                return _debuggerState;
            }

            private set
            {
                _debuggerState = value;
            }
        }

        public DebuggerPlugin Plugin { get; }

        public MessageHandler DebuggerMessageHandler { get; }

        public class MessageHandler : IMessageHandler
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

            public void RegisterSource(string source, string code)
            {
                _sourceMap[source] = code;
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
                        else if (src.EndsWith("cv4"))
                        {
                            stream = WebRequest.Create(uri + src.Substring(0, src.Length - 3) + "nut")
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

                // Change state
                var ctype = (char) type;
                switch (ctype)
                {
                    case 'l':
                        if (_window.DebuggerState == State.StepOver)
                        {
                            _window.DebuggerState = State.Breaking;
                        }

                        if (_window.DebuggerState == State.Breaking)
                        {
                            break;
                        }
                        
                        HashSet<int> breakPoints;
                        if (_window.BreakPointMap.TryGetValue(_currentSrcName, out breakPoints) && breakPoints != null && breakPoints.Contains(_currentLine))
                        {
                            ShowLine(srcname, line);
                            _window.DebuggerState = State.Breaking;
                        }

                        break;
                    case 'c':
                        if (_window.DebuggerState == State.StepOver)
                        {
                            _window.DebuggerState = State.BreakAfterReturn;
                        }
                        break;
                    case 'r':
                        switch (_window.DebuggerState)
                        {
                            case State.BreakReturning:
                                _window.DebuggerState = State.Breaking;
                                break;
                            case State.BreakAfterReturn:
                                _window.DebuggerState = State.Breaking;
                                break;
                        }
                        break;
                }

                // Apply state
                if (_window.DebuggerState == State.Breaking)
                {
                    ShowLine(srcname, line);
                    if (Thread.CurrentThread == _window.Invoke((Func<Thread>)(() => Thread.CurrentThread)))
                        return;
                    _window.SetControlEnabled();
                    _window.Plugin.Suspend();
                }
            }

            private void ShowLine(string srcname, int line)
            {
                string src;
                if (_sourceMap.TryGetValue(srcname, out src))
                {
                    _window.Invoke((Action)(() => { _window.AddMessage($"(Source: {srcname} : {line}){src.GetLine(line)}"); }));
                    return;
                }

                var uris = _window.Plugin.CurrentConfig?.SourceUri;
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

                    src = GetSource(uri, srcname);
                    if (src != null)
                    {
                        var linestr = src.GetLine(line);
                        if (linestr != null)
                        {
                            _window.Invoke((Action)(() => { _window.AddMessage($"(Source: {uri + srcname} : {line}){linestr}"); }));
                        }

                        return;
                    }
                }

                _window.Invoke((Action)(() => { _window.AddMessage($"Could not load source \"{srcname}\" from provided uris"); }));
            }

            public void CompilerExceptionArrived(string exceptiondesc, string srcname, int line, int column)
            {
                _window.Invoke((Action)(() =>
                {
                    ShowLine(srcname, line);
                    _window.AddMessage($"Unhandled compiler exception detected, description: {exceptiondesc}, source: {srcname}, line: {line}, column: {column}");
                }));
            }

            public void RuntimeExceptionArrived(string exceptiondesc)
            {
                _window.Invoke((Action) (() =>
                {
                    _window.AddMessage($"Unhandled exception detected, description: {exceptiondesc}");
                }));
                
                ShowLine(_currentSrcName, _currentLine);
                if (Thread.CurrentThread == _window.Invoke((Func<Thread>) (() => Thread.CurrentThread)))
                    return;
                _window.DebuggerState = State.Breaking;
                _window.Plugin.Suspend();
            }
        }

        public DebuggerWindow(DebuggerPlugin plugin)
        {
            Plugin = plugin;
            DebuggerMessageHandler = new MessageHandler(this);
            Plugin.RegisterMessageHandler(DebuggerMessageHandler);
            _breakPointSetter = new BreakPointSetter(this);
            _interactive = new Interactive(this);
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

        private void SetControlEnabled()
        {
            SetControlEnabled(DebuggerState != State.Breaking);
        }

        private void SetControlEnabled(bool value)
        {
            btnPause.Enabled = value;
            btnContinue.Enabled = btnExeToRet.Enabled = btnStepInto.Enabled = btnStepOver.Enabled = !value;
        }

        public void AddMessage(string msg)
        {
            listBox1.Items.Add(msg);
        }

        private void btnPause_Click(object sender, EventArgs e)
        {
            DebuggerState = State.Breaking;
        }
        
        private void btnContinue_Click(object sender, EventArgs e)
        {
            DebuggerState = State.Running;
            Plugin.Resume();
            SetControlEnabled();
        }

        private void btnSetBreakPoint_Click(object sender, EventArgs e)
        {
            _breakPointSetter.Show();
        }

        private void btnStepInto_Click(object sender, EventArgs e)
        {
            DebuggerState = State.Breaking;
            Plugin.Resume();
            SetControlEnabled();
        }

        private void btnStepOver_Click(object sender, EventArgs e)
        {
            DebuggerState = State.StepOver;
            Plugin.Resume();
            SetControlEnabled();
        }

        private void btnExeToRet_Click(object sender, EventArgs e)
        {
            DebuggerState = State.BreakReturning;
            Plugin.Resume();
            SetControlEnabled();
        }
        
        private void btnInteractive_Click(object sender, EventArgs e)
        {
            _interactive.Show();
        }

        private void DebuggerWindow_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (e.CloseReason == CloseReason.UserClosing)
            {
                e.Cancel = true;
                Hide();
            }
        }
    }
}
