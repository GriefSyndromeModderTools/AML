using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using PluginUtils.Injection.Squirrel;

namespace Debugger
{
    public partial class Interactive : Form
    {
        private readonly DebuggerWindow _window;
        public bool ShouldResume { private get; set; } = true;

        public Interactive(DebuggerWindow window)
        {
            _window = window;
            InitializeComponent();
        }

        private void Interactive_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (e.CloseReason == CloseReason.UserClosing)
            {
                e.Cancel = true;
                Hide();
            }
        }

        private void btnExecute_Click(object sender, EventArgs e)
        {
            var code = txtCode.Text;
            _window.DebuggerMessageHandler.RegisterSource("Interactive", code);
            bool errored;
            var ret = _window.Plugin.Execute(code, "Interactive", out errored);
            var msg = new StringBuilder(errored ? "Interactive error: " : ">> ");

            var str = ret.ToString();
            msg.Append(str != string.Empty ? str : $"({ret.Type.GetTypeString()})");

            SquirrelFunctions.release_(SquirrelHelper.SquirrelVM, ref ret);

            _window.AddMessage(msg.ToString());
        }

        private void Interactive_VisibleChanged(object sender, EventArgs e)
        {
            if (Visible)
            {
                _window.DebuggerMessageHandler.Enabled = false;
            }
            else
            {
                _window.DebuggerMessageHandler.Enabled = true;
                if (ShouldResume)
                {
                    _window.DebuggerState = DebuggerWindow.State.Running;
                    _window.DebuggerMessageHandler.Resume();
                }
            }
        }
    }
}
