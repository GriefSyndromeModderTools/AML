using PluginUtils;
using PluginUtils.Injection.Squirrel;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Results_poi
{
    public enum Type : int
    {
        ERROR,
        LOAD_PAT_START,     //[ Type, Frame, Time, Point ]
        LOAD_PAT_END,       //[ Type, Frame, Time, Point ]
        STAGE_START,        //[ Type, Frame, Time, Point ]
        STAGE_CLEAR,        //[ Type, Frame, Time, Point ]
        GET_EXP,            //[ Type, Player, Character, Exp_pre, Exp_add ]
        LEVEL_UP,           //[ Type, Player, Character, Lv_pre ]
        NULL                //[ Type, Value ]
    };

    public class Data : IAMLPlugin
    {
        private static ArrayList list = new ArrayList();
        private static ShowDataForm text_poi = new ShowDataForm(); //窗口对象
        private static Stopwatch time_poi = new Stopwatch();

        public void Init()
        {
            var func_get_gameTime = SquirrelHelper.CompileScriptFunction("return ::gameData.gameTime;", "<my_test>");
            PluginUtils.WindowsHelper.Run(delegate ()
            {
                text_poi.Show();
                time_poi.Start();
            });

            SquirrelHelper.InjectCompileFile("data/stage/stage1.nut", "Stage1_MasterInit").AddBefore(vm =>
            {
                list.Add(new int[4] { (int)Type.LOAD_PAT_START, Get(vm, func_get_gameTime), (int)time_poi.ElapsedMilliseconds, 100 });
                UpdateText();
            });
            SquirrelHelper.InjectCompileFile("data/stage/stage1.nut", "Stage1_MasterInit").AddAfter(vm =>
            {
                list.Add(new int[4] { (int)Type.LOAD_PAT_END, Get(vm, func_get_gameTime), (int)time_poi.ElapsedMilliseconds, 101 });
                list.Add(new int[4] { (int)Type.STAGE_START, Get(vm, func_get_gameTime), (int)time_poi.ElapsedMilliseconds, 1 });
                UpdateText();
            });
            SquirrelHelper.InjectCompileFile("data/stage/stage1.nut", "ClearEventActor").AddAfter(vm =>
            {
                list.Add(new int[4] { (int)Type.STAGE_CLEAR, Get(vm, func_get_gameTime), (int)time_poi.ElapsedMilliseconds, 1 });
                UpdateText();
            });

        }

        public void Load()
        {
        }

        public static void AddText(int[] poi)
        {
            switch(poi[0])
            {
                case (int)Type.LOAD_PAT_START:
                    text_poi.textBox1.AppendText(" \n");
                    text_poi.textBox1.AppendText("Frame: " + poi[1] + "  " + "Time: " + poi[2] / 1000.0 + "s\n");
                    text_poi.textBox1.AppendText("Load pat start: " + poi[3] + "\n");
                    break;
                case (int)Type.LOAD_PAT_END:
                    text_poi.textBox1.AppendText(" \n");
                    text_poi.textBox1.AppendText("Frame: " + poi[1] + "  " + "Time: " + poi[2] / 1000.0 + "s\n");
                    text_poi.textBox1.AppendText("Load pat end: " + poi[3] + "\n");
                    break;
                case (int)Type.STAGE_START:
                    text_poi.textBox1.AppendText(" \n");
                    text_poi.textBox1.AppendText("Frame: " + poi[1] + "  " + "Time: " + poi[2] / 1000.0 + "s\n");
                    text_poi.textBox1.AppendText("Stage start: " + poi[3] + "\n");
                    break;
                case (int)Type.STAGE_CLEAR:
                    text_poi.textBox1.AppendText(" \n");
                    text_poi.textBox1.AppendText("Frame: " + poi[1] + "  " + "Time: " + poi[2] / 1000.0 + "s\n");
                    text_poi.textBox1.AppendText("Stage clear: " + poi[3] + "\n");
                    break;
                default:
                    text_poi.textBox1.AppendText("poi " + poi[0] + "\n");
                    break;
            }
        }

        public static void UpdateText()
        {
            text_poi.textBox1.Clear();
            for(int i=0;i<list.Count;i++)
            {
                AddText((int[])list[i]);
            }
        }

        public static int Get(IntPtr vm,IntPtr func)
        {
            int lap;
            SquirrelFunctions.pushobject(vm, func);
            SquirrelFunctions.pushroottable(vm);
            SquirrelFunctions.call(vm, 1, 1, 0);
            SquirrelFunctions.getinteger(vm, -1, out lap);
            SquirrelFunctions.pop(vm, 2);
            return lap;
        }


    }
}
