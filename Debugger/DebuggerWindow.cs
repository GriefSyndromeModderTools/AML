using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Debugger
{
    public partial class DebuggerWindow : Form
    {
        private readonly DebuggerPlugin _plugin;
        private class MessageHandler : IMessageHandler
        {
            private readonly DebuggerWindow _window;

            public MessageHandler(DebuggerWindow window)
            {
                _window = window;
            }

            public void DebugInfomationArrived(int type, string srcname, int line, string funcname)
            {
                var ctype = (char)type;
                if (ctype != 'l')
                {
                    // Too slow, faster method requested= =
                    //_window.textBox1.Text += $"type {(char)type}, source {srcname}, line {line}, funcname {funcname}.{Environment.NewLine}";
                }
            }

            public void CompilerExceptionArrived(string exceptiondesc, string srcname, int line, int column)
            {
                _window.textBox1.Text += $"Unhandled compiler exception detected, description: {exceptiondesc}, source: {srcname}, line: {line}, column: {column}.{Environment.NewLine}";
            }

            public void RuntimeExceptionArrived(string exceptiondesc)
            {
                _window.textBox1.Text += $"Unhandled exception detected, description: {exceptiondesc}.{Environment.NewLine}";
                _window._plugin.SuspendVm();
            }
        }

        public DebuggerWindow(DebuggerPlugin plugin)
        {
            _plugin = plugin;
            _plugin.RegisterMessageHandler(new MessageHandler(this));
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            _plugin.WakeupVm(true, true, true);
        }
    }
}
