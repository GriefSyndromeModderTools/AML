using System;
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
        private Client _Client;
        private Server _Server;
        private bool _StartGame;

        public ConnectionSelectForm()
        {
            InitializeComponent();
            _Instance = this;
        }

        public static void Log(string msg)
        {
            if (!_Instance.IsDisposed && _Instance.Visible)
            {
                _Instance.Invoke((Action)delegate()
                {
                    _Instance.textBox3.AppendText(msg + "\n");
                });
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (_Client != null || _Server != null)
            {
                return;
            }
            _Server = new Server(int.Parse(textBox2.Text));
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (_Client != null || _Server != null)
            {
                return;
            }
            _Client = new Client(textBox1.Text, int.Parse(textBox2.Text));
        }

        private void button3_Click(object sender, EventArgs e)
        {
            if (_Server != null && !_StartGame)
            {
                _StartGame = true;
                _Server.StartGame();
            }
        }

        public static void CloseWindow()
        {
            if (!_Instance.IsDisposed && _Instance.Visible)
            {
                _Instance.Invoke((Action)delegate()
                {
                    _Instance.Close();
                });
            }
        }
    }
}
