using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AGSO.Core.GSO
{
    public partial class NetworkListenerForm : Form
    {
        public NetworkListenerForm()
        {
            InitializeComponent();
        }

        public void Append(string type, string data)
        {
            PluginUtils.WindowsHelper.Run(delegate()
            {
                var item = new ListViewItem(new[] { type, data });
                listView1.Items.Add(item);
                if (listView1.Items.Count > 50)
                {
                    listView1.Items.RemoveAt(0);
                }
                item.EnsureVisible();
            });
        }

        private void NetworkListenerForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            e.Cancel = true;
        }
    }
}
