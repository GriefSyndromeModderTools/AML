using PluginUtils;
using PluginUtils.Injection.Squirrel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

namespace Show_poi
{
    public class Show : IAMLPlugin
    {
        private static MessagePoi text_poi; //窗口对象
        private static Stopwatch time_poi = new Stopwatch();
        private static long time_pre = 0;

        public void Init()
        {
            SquirrelHelper.RegisterGlobalFunction("SendMessagePoi", GetMessagePoi);
            PluginUtils.WindowsHelper.Run(delegate ()
            {
                text_poi = new MessagePoi();
                text_poi.Show();
                time_poi.Start();
            });
            
            SquirrelHelper.InjectCompileFile("data/stage/stage1.nut", "Stage1_MasterInit").AddBefore(vm =>
            {
                Show.ShowMessagePoi(1, 9);
                Show.ShowMessagePoi(2, 101);
                Show.ShowMessagePoi(3, 102);
            });
            SquirrelHelper.InjectCompileFile("data/stage/stage1.nut", "Stage1_MasterInit").AddAfter(vm =>
            {
                Show.ShowMessagePoi(1, 9);
                Show.ShowMessagePoi(4, 100);
            });
            SquirrelHelper.InjectCompileFile("data/stage/stage1.nut", "ClearEventActor").AddAfter(vm =>
            {
                Show.ShowMessagePoi(1, 9);
                Show.ShowMessagePoi(5, 1);
            });

            SquirrelHelper.InjectCompileFile("data/stage/stage2.nut", "Stage2_MasterInit").AddBefore(vm =>
            {
                Show.ShowMessagePoi(1, 9);
                Show.ShowMessagePoi(2, 201);
                Show.ShowMessagePoi(3, 202);
            });
            SquirrelHelper.InjectCompileFile("data/stage/stage2.nut", "Stage2_MasterInit").AddAfter(vm =>
            {
                Show.ShowMessagePoi(1, 9);
                Show.ShowMessagePoi(4, 200);
            });
            SquirrelHelper.InjectCompileFile("data/stage/stage2.nut", "ClearEventActor").AddAfter(vm =>
            {
                Show.ShowMessagePoi(1, 9);
                Show.ShowMessagePoi(5, 2);
            });

            SquirrelHelper.InjectCompileFile("data/stage/stage3.nut", "Stage3_MasterInit").AddBefore(vm =>
            {
                Show.ShowMessagePoi(1, 9);
                Show.ShowMessagePoi(2, 301);
                Show.ShowMessagePoi(3, 302);
            });
            SquirrelHelper.InjectCompileFile("data/stage/stage3.nut", "Stage3_MasterInit").AddAfter(vm =>
            {
                Show.ShowMessagePoi(1, 9);
                Show.ShowMessagePoi(4, 300);
            });
            SquirrelHelper.InjectCompileFile("data/stage/stage3.nut", "ClearEventActor").AddAfter(vm =>
            {
                Show.ShowMessagePoi(1, 9);
                Show.ShowMessagePoi(5, 3);
            });

            SquirrelHelper.InjectCompileFile("data/stage/stage4.nut", "Stage4_MasterInit").AddBefore(vm =>
            {
                Show.ShowMessagePoi(1, 9);
                Show.ShowMessagePoi(2, 401);
                Show.ShowMessagePoi(3, 402);
            });
            SquirrelHelper.InjectCompileFile("data/stage/stage4.nut", "Stage4_MasterInit").AddAfter(vm =>
            {
                Show.ShowMessagePoi(1, 9);
                Show.ShowMessagePoi(4, 400);
            });
            SquirrelHelper.InjectCompileFile("data/stage/stage4.nut", "ClearEventActor").AddAfter(vm =>
            {
                Show.ShowMessagePoi(1, 9);
                Show.ShowMessagePoi(5, 4);
            });

            SquirrelHelper.InjectCompileFile("data/stage/stage6.nut", "Stage6_MasterInit").AddBefore(vm =>
            {
                Show.ShowMessagePoi(1, 9);
                Show.ShowMessagePoi(2, 601);
                Show.ShowMessagePoi(3, 602);
            });
            SquirrelHelper.InjectCompileFile("data/stage/stage6.nut", "Stage6_MasterInit").AddAfter(vm =>
            {
                Show.ShowMessagePoi(1, 9);
                Show.ShowMessagePoi(4, 600);
            });
            SquirrelHelper.InjectCompileFile("data/stage/stage6.nut", "ClearEventActor").AddAfter(vm =>
            {
                Show.ShowMessagePoi(1, 9);
                Show.ShowMessagePoi(5, 6);
            });
            
        }

        public void Load()
        {
        }

        public static int GetMessagePoi(IntPtr p)
        {
            int type = 0, value = 0;
            SquirrelFunctions.getinteger(p, 3, out value);
            SquirrelFunctions.getinteger(p, 2, out type);
            ShowMessagePoi(type, value);
            return 0;
        }

        public static void ShowMessagePoi(int type_poi, int value_poi)
        {
            WindowsHelper.Run(delegate()
            {
                /*
                type_poi取值
                * 0: 直接显示
                * 1: 帧数显示
                * 2: 加载pat
                * 3: 加载pat
                * 4: 加载结束
                * 5: 关卡结束
                */
                switch (type_poi)
                {
                    case 0:
                        text_poi.textBox1.AppendText(value_poi + "\n");
                        break;
                    case 1:
                        text_poi.textBox1.AppendText("----------------------------------------\n");
                        text_poi.textBox1.AppendText("Frame: " + value_poi + "  "
                            + "Time: " + time_poi.ElapsedMilliseconds / 1000.0 + "s  "
                            + "Pre:" + ((time_poi.ElapsedMilliseconds - time_pre) / 1000.0) + "s\n");
                        time_pre = time_poi.ElapsedMilliseconds;
                        break;
                    case 2:
                        text_poi.textBox1.AppendText("Load enemy pat: " + value_poi + "\n");
                        break;
                    case 3:
                        text_poi.textBox1.AppendText("Load boss pat: " + value_poi + "\n");
                        break;
                    case 4:
                        text_poi.textBox1.AppendText("Load end: " + value_poi + "\n");
                        break;
                    case 5:
                        text_poi.textBox1.AppendText("Stage Clear: " + value_poi + "\n");
                        break;
                    default:
                        text_poi.textBox1.AppendText("poi " + type_poi + " " + value_poi + "\n");
                        break;
                }
            });
        }
    }
}
