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
        private static IntPtr _IntBuffer = Marshal.AllocHGlobal(4);
        private static int type_poi; //信息类型
        private static int value_poi; //信息的值
        private static MessagePoi text_poi; //窗口对象

        public void Init()
        {
            SquirrelHelper.RegisterGlobalFunction("SendMessagePoi", GetMessagePoi);
            PluginUtils.WindowsHelper.Run(delegate ()
            {
                text_poi = new MessagePoi();
                //text_poi.Show();
                new System.Windows.Forms.Form().Show();
            });
        }

        public void Load()
        {
        }

        public static int GetMessagePoi(IntPtr p)
        {
            SquirrelFunctions.getinteger(p, 2, _IntBuffer);
            type_poi = Marshal.ReadInt32(_IntBuffer);
            SquirrelFunctions.getinteger(p, 3, _IntBuffer);
            value_poi = Marshal.ReadInt32(_IntBuffer);
            ShowMessagePoi();
            return 0;
        }

        public static void ShowMessagePoi()
        {
            text_poi.textBox1.AppendText("poi " + type_poi + " " + value_poi + "\n");
        }
    }
}
