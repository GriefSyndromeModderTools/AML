using AGSO.Core.Connection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace NetworkTest
{
    class ConnectFormTest
    {
        public static void Main()
        {
            ConnectionSelectForm f = new ConnectionSelectForm();
            Application.Run(f);
        }
    }
}
