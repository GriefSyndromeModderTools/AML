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
            /*var task = Task.Run(() => _window.Plugin.Execute(code, "Interactive"));
            task.Wait();
            var ret = task.Result;*/
            var ret = _window.Plugin.Execute(code, "Interactive");

            var msg = new StringBuilder(">> ");
            switch (ret.Type)
            {
                case SquirrelHelper.SQObjectType.OT_NULL:
                    msg.Append("null");
                    break;
                case SquirrelHelper.SQObjectType.OT_INTEGER:
                    msg.Append(ret.Value.Integer);
                    break;
                case SquirrelHelper.SQObjectType.OT_FLOAT:
                    msg.Append(ret.Value.Float);
                    break;
                case SquirrelHelper.SQObjectType.OT_BOOL:
                    msg.Append(ret.Value.Integer == 0 ? "false" : "true");
                    break;
                case SquirrelHelper.SQObjectType.OT_STRING:
                    msg.Append($"\"{Marshal.PtrToStringAnsi(ret.Value.Pointer + 28)}\"");
                    break;
                case SquirrelHelper.SQObjectType.OT_TABLE:
                    msg.Append("(table)");
                    break;
                case SquirrelHelper.SQObjectType.OT_ARRAY:
                    msg.Append("(array)");
                    break;
                case SquirrelHelper.SQObjectType.OT_USERDATA:
                    msg.Append("(userdata)");
                    break;
                case SquirrelHelper.SQObjectType.OT_CLOSURE:
                    msg.Append("(closure)");
                    break;
                case SquirrelHelper.SQObjectType.OT_NATIVECLOSURE:
                    msg.Append("(native closure)");
                    break;
                case SquirrelHelper.SQObjectType.OT_GENERATOR:
                    msg.Append("(generator)");
                    break;
                case SquirrelHelper.SQObjectType.OT_USERPOINTER:
                    msg.Append("(userpointer)");
                    break;
                case SquirrelHelper.SQObjectType.OT_THREAD:
                    msg.Append("(thread)");
                    break;
                case SquirrelHelper.SQObjectType.OT_FUNCPROTO:
                    msg.Append("(funcproto)");
                    break;
                case SquirrelHelper.SQObjectType.OT_CLASS:
                    msg.Append("(class)");
                    break;
                case SquirrelHelper.SQObjectType.OT_INSTANCE:
                    msg.Append("(instance)");
                    break;
                case SquirrelHelper.SQObjectType.OT_WEAKREF:
                    msg.Append("(weakref)");
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            SquirrelFunctions.release_(SquirrelHelper.SquirrelVM, ref ret);

            _window.AddMessage(msg.ToString());
        }
    }
}
