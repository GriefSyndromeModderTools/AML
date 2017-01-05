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
        HIT_PLAYER,         
        HIT_ENEMY,          //[ Type, Player, Character, Hit_type, Enemy, Dam, Enemy_life ]
        HIT_DEAD,           //[ Type, Player, Character, Hit_type, Enemy, Dam, Enemy_life ]
        NULL                //[ Type, Value ]
    };

    [Plugin]
    public class Data : IAMLPlugin
    {
        private static ArrayList list = new ArrayList();
        private static ShowDataForm text_poi = new ShowDataForm(); //窗口对象
        private static Stopwatch time_poi = new Stopwatch();//计时器
        private static int flag_poi; //临时变量
        private static int flag_poi2; //临时变量
        private static int flag_poi3; //临时变量

        public void Init()
        {
            //var func_get_gameTime = SquirrelHelper.CompileScriptFunction("return ::gameData.gameTime;", "<my_test>");

            PluginUtils.WindowsHelper.Run(delegate ()
            {
                text_poi.Show();
                time_poi.Start();
            });

            SquirrelHelper.InjectCompileFile("data/stage/stage1.nut", "Stage1_MasterInit").AddBefore(vm =>
            {
                list.Add(new int[4] { (int)Type.LOAD_PAT_START, Get(vm, "gameData", "stageTime"), (int)time_poi.ElapsedMilliseconds, 100 });
                UpdateText();
            });
            SquirrelHelper.InjectCompileFile("data/stage/stage1.nut", "Stage1_MasterInit").AddAfter(vm =>
            {
                list.Add(new int[4] { (int)Type.LOAD_PAT_END, Get(vm, "gameData", "stageTime"), (int)time_poi.ElapsedMilliseconds, 101 });
                UpdateText();
                list.Add(new int[4] { (int)Type.STAGE_START, Get(vm, "gameData", "stageTime"), (int)time_poi.ElapsedMilliseconds, 1 });
                UpdateText();
            });
            SquirrelHelper.InjectCompileFile("data/stage/stage1.nut", "ClearEventActor").AddAfter(vm =>
            {
                list.Add(new int[4] { (int)Type.STAGE_CLEAR, Get(vm, "gameData", "stageTime"), (int)time_poi.ElapsedMilliseconds, 1 });
                UpdateText();
            });
            SquirrelHelper.InjectCompileFile("data/script/hit.nut", "OnHitActor").AddBefore(vm =>
            {
                flag_poi = GetCs(vm, 2, "life"); //def.life
                flag_poi2 = GetCs(vm, 1, "exp"); //atk.exp
                flag_poi3 = GetCs(vm, 2, "exp"); //def.exp
            });
            SquirrelHelper.InjectCompileFile("data/script/hit.nut", "OnHitActor").AddAfter(vm =>
            {
                int life_start = flag_poi;
                int life_end = GetCs(vm, 2, "life"); //def.life
                int dam = life_start - life_end;
                string def_names = GetCsStr(vm, 2, "name");
                string atk_names = GetCsStr(vm, 1, "name");
                //int def_cbG = Get(vm, "actor", def_names, "callbackGroup");
                int def_cbG = 2;

                if (def_cbG == 2 && life_end <= 0 && dam>0)
                {
                    string atk_own = GetCsStr(vm, 1, "owner");
                    int atk_u_ID = Get(vm, "actor", atk_own, "u", "playerID") + 1;
                    int atk_u_CA = Get(vm, "actor", atk_own, "u", "CA");
                    int atk_exp = flag_poi2;
                    int atk_level = Get(vm, "actor", atk_own, "level");
                    int atk_expM = 100;
                    int def_exp = Get(vm, "actor", atk_own, "exp") - atk_exp;
                    int def_u_CA = GetCs(vm, 2, "u", "CA");
                    int atk_type = Get(vm, "actor", atk_names, "motion");
                    int def_mot = Get(vm, "actor", def_names, "motion");

                    if (def_exp <= 0 && flag_poi3 > 0)
                    {
                        def_exp = 100 - atk_exp;
                    }
                    if (atk_u_CA < 700 && life_start > 0)
                    {
                        // HIT_ENEMY:[ Type, Player, Character, Hit_type, Enemy, Dam, Enemy_life ]
                        list.Add(new int[7] { (int)Type.HIT_ENEMY, atk_u_ID, atk_u_CA, atk_type, def_u_CA, dam, life_start });
                        UpdateText();
                    }
                    if (atk_u_CA < 700 && def_exp > 0)
                    {
                        if(life_start <= 0)
                        {
                            // HIT_DEAD:[ Type, Player, Character, Hit_type, Enemy, Dam, Enemy_life ]
                            list.Add(new int[7] { (int)Type.HIT_DEAD, atk_u_ID, atk_u_CA, atk_type, def_u_CA, dam, life_start });
                            UpdateText();
                        }
                        // GET_EXP:[ Type, Player, Character, Exp_pre, Exp_add ]
                        list.Add(new int[5] { (int)Type.GET_EXP, atk_u_ID, atk_u_CA, atk_exp, def_exp });
                        UpdateText();
                    }
                    if (atk_u_CA < 700 && atk_exp + def_exp >= atk_expM)
                    {
                        // LEVEL_UP:[ Type, Player, Character, Lv_pre ]
                        list.Add(new int[4] { (int)Type.LEVEL_UP, atk_u_ID, atk_u_CA, atk_level });
                        UpdateText();
                    }
                }
            });

        }
        public void Load()
        {
        }

        public static void UpdateText()
        {
                AddText((int[])list[list.Count - 1]);
        }
        public static void UpdateTextAll()
        {
            text_poi.textBox1.Clear();
            for (int i = 0; i < list.Count; i++)
            {
                AddText((int[])list[i]);
            }
        }

        #region function AddText
         
        //Show text in the windows
        public static void AddText(int[] poi)
        {
        PluginUtils.WindowsHelper.Run(delegate ()
        {

            switch (poi[0])
                {
                    case (int)Type.LOAD_PAT_START:
                        AddText_StartEnd(poi, "Load pat start");
                        break;
                    case (int)Type.LOAD_PAT_END:
                        AddText_StartEnd(poi, "Load pat end");
                        break;
                    case (int)Type.STAGE_START:
                        AddText_StartEnd(poi, "stage start");
                        break;
                    case (int)Type.STAGE_CLEAR:
                        AddText_StartEnd(poi, "stage clear");
                        break;
                    case (int)Type.HIT_ENEMY:
                        AddText_HIT_ENEMY(poi);
                        break;
                    case (int)Type.HIT_DEAD:
                        AddText_HIT_DEAD(poi);
                        break;
                    case (int)Type.GET_EXP:
                        AddText_GET_EXP(poi);
                        break;
                    case (int)Type.LEVEL_UP:
                        AddText_LEVEL_UP(poi);
                        break;
                    default:
                        text_poi.textBox1.AppendText("\npoi " + poi[1]);
                        break;
                }
        });

        }
        public static void AddText_StartEnd(int[] poi, string msg)
        {
            text_poi.textBox1.AppendText(" \n");
            text_poi.textBox1.AppendText(" \n");
            text_poi.textBox1.AppendText("Frame: " + poi[1] + "  " + "Time: " + poi[2] / 1000.0 + "s\n");
            text_poi.textBox1.AppendText(msg + ": " + poi[3]);
        }
        public static void AddText_HIT_ENEMY(int[] poi)
        {
            //[ Type, Player, Character, Hit_type, Enemy, Dam, Enemy_life ]
            text_poi.textBox1.AppendText(" \n");
            text_poi.textBox1.AppendText(
                 poi[1] + "P:" + AT_GetPlayerName(poi[2]) + 
                 " " + AT_GetHitType(poi[3]) + " " + AT_GetEnemy(poi[4]) + "  " + poi[5] + " => " + poi[6]);
        }
        public static void AddText_HIT_DEAD(int[] poi)
        {
            //[ Type, Player, Character, Hit_type, Enemy, Dam, Enemy_life ]
            text_poi.textBox1.AppendText(" \n");
            text_poi.textBox1.AppendText(
                poi[1] + "P:" + AT_GetPlayerName(poi[2]) + 
                " " + AT_GetHitType(poi[3]) + " " + AT_GetEnemy(poi[4]) + "  " + poi[5] + " => " + poi[6] + " (鞭尸吃经验)");
        }
        public static void AddText_GET_EXP(int[] poi)
        {
            //[ Type, Player, Character, Exp_pre, Exp_add ]
            int t = list.Count - 2;
            if (t >= 0)
            {
                int[] a = (int[])list[t];
                if (a[0] == (int)Type.HIT_ENEMY || a[0] == (int)Type.HIT_DEAD || a[0] == (int)Type.GET_EXP)
                {
                    text_poi.textBox1.AppendText(
                        "   " + "Exp +" + poi[4] + "  " + poi[3] + " -> " + (poi[3] + poi[4]));
                    return;
                }
            }
            text_poi.textBox1.AppendText(" \n");
            text_poi.textBox1.AppendText(
                poi[1] + "P:" + AT_GetPlayerName(poi[2]) + " " +
                "Exp +" + poi[4] + "  " + poi[3] + " -> " + (poi[3] + poi[4]));
        }
        public static void AddText_LEVEL_UP(int[] poi)
        {
            //[ Type, Player, Character, Lv_pre ]
            int t = list.Count - 2;
            if (t >= 0)
            {
                int[] a = (int[])list[t];
                if (a[0] == (int)Type.HIT_ENEMY || a[0] == (int)Type.HIT_DEAD || a[0] == (int)Type.GET_EXP || a[0] == (int)Type.LEVEL_UP)
                {
                    text_poi.textBox1.AppendText(
                        "   " + "LEVEL UP" + "  " + (poi[3] - 1) + " -> " + poi[3]);
                    return;
                }
            }
            text_poi.textBox1.AppendText(" \n");
            text_poi.textBox1.AppendText(
                poi[1] + "P:" + AT_GetPlayerName(poi[2]) + " " + "LEVEL UP" + "  " + (poi[3]-1) + " -> " + poi[3]);
        }


        //Get string from u.CA or motion
        public static string AT_GetPlayerName(int CA)
        {
            string[] poi = {
                "晓美",
                "美树",
                "杏子",
                "学姐",
                "鹿目",
                "麻花",
                "QB  ",
            };
            int t = CA / 100; 
            if(t <= 6)
            {
                return poi[t];
            }
            return CA + "?不认识的孩子呢";
        }
        public static string AT_GetHitType(int motion)
        {
            return "hit";
        }
        public static string AT_GetEnemy(int CA)
        {
            switch(CA)
            {
                case 2000:
                    return "品客";
                case 2100:
                    return "小白";
                case 2200:
                    return "蝴蝶";
                case 2900:
                    return "蔷薇";
                case 3000:
                    return "老鼠";
                case 3800:
                    return "小夏";
                case 3900:
                    return "大夏";
                case 4000:
                    return "天使";
                case 4100:
                    return "大天使";
                case 4900:
                    return "电视";
                case 6000:
                    return "兔头";
                case 6100:
                    return "蛇头";
                case 6200:
                    return "狼头";
                case 5900:
                    return "影魔";
                case 6900:
                    return "人鱼";
                case 7000:
                    return "魔法使魔(空)";
                case 7100:
                    return "魔法使魔(地)";
                case 7200:
                    return "体术使魔";
                case 7900:
                    return "瓦夜";
                default:
                    return CA + "";
            }
        }

        #endregion

        #region function Get

        //Get globel 
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
        public static int Get(IntPtr vm, string str1, string str2)
        {
            int poi;
            SquirrelFunctions.pushroottable(vm);
            SquirrelFunctions.pushstring(vm, str1, -1);
            SquirrelFunctions.rawget(vm, -2);
            SquirrelFunctions.pushstring(vm, str2, -1);
            SquirrelFunctions.rawget(vm, -2);
            SquirrelFunctions.getinteger(vm, -1, out poi);
            SquirrelFunctions.pop(vm, 3);
            return poi;
        }
        public static int Get(IntPtr vm, string str1, string str2, string str3)
        {
            int poi;
            SquirrelFunctions.pushroottable(vm);
            SquirrelFunctions.pushstring(vm, str1, -1);
            SquirrelFunctions.rawget(vm, -2);
            SquirrelFunctions.pushstring(vm, str2, -1);
            SquirrelFunctions.rawget(vm, -2);
            SquirrelFunctions.pushstring(vm, str3, -1);
            SquirrelFunctions.rawget(vm, -2);
            SquirrelFunctions.getinteger(vm, -1, out poi);
            SquirrelFunctions.pop(vm, 4);
            return poi;
        }
        public static int Get(IntPtr vm, string str1, string str2, string str3, string str4)
        {
            int poi;
            SquirrelFunctions.pushroottable(vm);
            SquirrelFunctions.pushstring(vm, str1, -1);
            SquirrelFunctions.rawget(vm, -2);
            SquirrelFunctions.pushstring(vm, str2, -1);
            SquirrelFunctions.rawget(vm, -2);
            SquirrelFunctions.pushstring(vm, str3, -1);
            SquirrelFunctions.rawget(vm, -2);
            SquirrelFunctions.pushstring(vm, str4, -1);
            SquirrelFunctions.rawget(vm, -2);
            SquirrelFunctions.getinteger(vm, -1, out poi);
            SquirrelFunctions.pop(vm, 5);
            return poi;
        }

        //
        public static int GetCs(IntPtr vm, int t,string str)
        {       
            int poi;
            SquirrelFunctions.push(vm, t+1);
            SquirrelFunctions.pushstring(vm, str, -1);
            SquirrelFunctions.rawget(vm, -2);
            SquirrelFunctions.getinteger(vm, -1, out poi);
            SquirrelFunctions.pop(vm, 2);
            return poi;
        }
        public static int GetCs(IntPtr vm, int t, string str1, string str2)
        {
            int poi;
            SquirrelFunctions.push(vm, t + 1);
            SquirrelFunctions.pushstring(vm, str1, -1);
            SquirrelFunctions.rawget(vm, -2);
            SquirrelFunctions.pushstring(vm, str2, -1);
            SquirrelFunctions.rawget(vm, -2);
            SquirrelFunctions.getinteger(vm, -1, out poi);
            SquirrelFunctions.pop(vm, 3);
            return poi;
        }
        public static string GetCsStr(IntPtr vm, int t, string str)
        {
            string poi;
            SquirrelFunctions.push(vm, t + 1);
            SquirrelFunctions.pushstring(vm, str, -1);
            SquirrelFunctions.rawget(vm, -2);
            SquirrelFunctions.getstring(vm, -1, out poi);
            SquirrelFunctions.pop(vm, 2);
            return poi;
        }

        #endregion

    }
}
