using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AGSO.Core.Connection
{
    public partial class ConnectionSelectForm : Form
    {
        private static ConnectionSelectForm _Instance;
        private static ConcurrentQueue<string> _Message = new ConcurrentQueue<string>();
        private static ConcurrentQueue<Action> _Action = new ConcurrentQueue<Action>();

        private Client _Client;
        private Server _Server;
        private bool _StartGame;

        public ConnectionSelectForm()
        {
            InitializeComponent();
            _Instance = this;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (_Client != null || _Server != null)
            {
                return;
            }
            _Server = new Server(int.Parse(textBox2.Text));
            button1.Enabled = false;
            button2.Enabled = false;
            button3.Enabled = true;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (_Client != null || _Server != null)
            {
                return;
            }
            _Client = new Client(textBox1.Text, int.Parse(textBox2.Text));
            button1.Enabled = false;
            button2.Enabled = false;
            button3.Enabled = false;
        }

        private void button3_Click(object sender, EventArgs e)
        {
            if (_Server != null && !_StartGame)
            {
                _StartGame = true;
                _Server.StartGame();
            }
        }

        public static void Log(string msg)
        {
            var m = _Message;
            if (m != null)
            {
                m.Enqueue(msg);
            }
        }

        public static void CloseWindow()
        {
            var f = _Instance;
            if (f != null)
            {
                _Instance = null;
                _Message = null;
                _Action = null;
                f.Invoke((Action)delegate()
                {
                    f.Close();
                });
            }
        }

        public static void Ping(int ms)
        {
            _Action.Enqueue(delegate()
            {
                _Instance.textBox4.Text = "Ping " + ms + "ms";
            });
        }

        public static void Ping(int[] ms)
        {
            _Action.Enqueue(delegate()
            {
                _Instance.textBox4.Text = "Ping " + String.Concat(ms.Select(t => t + "ms "));
            });
        }

        private void ConnectionSelectForm_FormClosed(object sender, FormClosedEventArgs e)
        {
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            string msg;
            var m = _Message;
            if (m != null)
            {
                while (m.TryDequeue(out msg))
                {
                    _Instance.textBox3.AppendText(msg + "\n");
                }
            }
            var a = _Action;
            Action action;
            if (a != null)
            {
                while (a.TryDequeue(out action))
                {
                    action();
                }
            }
        }
    }
}
