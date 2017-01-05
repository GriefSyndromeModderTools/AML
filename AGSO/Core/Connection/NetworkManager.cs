using PluginUtils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AGSO.Core.Connection
{
    class NetworkManager
    {
        public static void ShowForm()
        {
            var mre = new ManualResetEvent(false);
            WindowsHelper.Run(delegate()
            {
                var dialog = new ConnectionSelectForm();
                dialog.FormClosed += delegate(object sender, System.Windows.Forms.FormClosedEventArgs e)
                {
                    mre.Set();
                };
                dialog.Show();
            });
            mre.WaitOne();
        }
    }
}
