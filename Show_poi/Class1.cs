using PluginUtils;
using PluginUtils.Injection.Squirrel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.Text;
using System.Threading.Tasks;

namespace Show_poi
{
    public class Class1 : IAMLPlugin
    {
        private static int type_poi; //信息类型
        private static int value_poi; //信息的值
        private static MessagePoi text_poi; //窗口对象

        public void Init()
        {
            SquirrelHelper.RegisterGlobalFunction("SendMessagePoi", GetMessagePoi);
            PluginUtils.WindowsHelper.Run(delegate ()
            {
                text_poi = new MessagePoi();
                text_poi.Show();
            });
        }

        public void Load()
        {
        }

        public static int GetMessagePoi(IntPtr p)
        {
            int a, b;
            SquirrelFunctions.getinteger(p, 2, out a);
            SquirrelFunctions.getinteger(p, 3, out b);
            ShowMessagePoi(a, b);
            return 0;
        }

        public static void ShowMessagePoi(int a, int b)
        {
            text_poi.textBox1.AppendText("poi " + a + " " + b + "\n");
        }
    }
}
