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
        private static volatile bool _Closed;

        public static void ShowForm()
        {
            _Closed = false;
            WindowsHelper.Run(delegate()
            {
                var dialog = new ConnectionSelectForm();
                dialog.FormClosed += delegate(object sender, System.Windows.Forms.FormClosedEventArgs e)
                {
                    _Closed = true;
                };
                dialog.Show();
            });
            while (!_Closed)
            {
                Thread.Sleep(200);
            }
        }
    }
}
