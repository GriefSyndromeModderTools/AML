using System;
using System.IO;
using PluginUtils.Injection.Squirrel;

namespace Debugger
{
    public static class StringUtils
    {
        /// <summary>
        /// 获得从字符串开头开始的从1开始的行号的字符串
        /// </summary>
        /// <param name="src">源字符串</param>
        /// <param name="line">行号</param>
        /// <returns>该行的字符串，若无法获得则为null</returns>
        public static string GetLine(this string src, int line)
        {
            --line;
            string linestr = null;
            using (var reader = new StringReader(src))
            {
                while (line-- >= 0)
                {
                    linestr = reader.ReadLine();
                    if (linestr == null)
                    {
                        return null;
                    }
                }
            }
            return linestr;
        }

        public static string GetTypeString(this SquirrelHelper.SQObjectType type)
        {
            switch (type)
            {
                case SquirrelHelper.SQObjectType.OT_NULL:
                    return "null";
                case SquirrelHelper.SQObjectType.OT_INTEGER:
                    return "integer";
                case SquirrelHelper.SQObjectType.OT_FLOAT:
                    return "float";
                case SquirrelHelper.SQObjectType.OT_BOOL:
                    return "bool";
                case SquirrelHelper.SQObjectType.OT_STRING:
                    return "string";
                case SquirrelHelper.SQObjectType.OT_TABLE:
                    return "table";
                case SquirrelHelper.SQObjectType.OT_ARRAY:
                    return "array";
                case SquirrelHelper.SQObjectType.OT_USERDATA:
                    return "userdata";
                case SquirrelHelper.SQObjectType.OT_CLOSURE:
                    return "closure";
                case SquirrelHelper.SQObjectType.OT_NATIVECLOSURE:
                    return "native closure";
                case SquirrelHelper.SQObjectType.OT_GENERATOR:
                    return "generator";
                case SquirrelHelper.SQObjectType.OT_USERPOINTER:
                    return "userpointer";
                case SquirrelHelper.SQObjectType.OT_THREAD:
                    return "thread";
                case SquirrelHelper.SQObjectType.OT_FUNCPROTO:
                    return "funcproto";
                case SquirrelHelper.SQObjectType.OT_CLASS:
                    return "class";
                case SquirrelHelper.SQObjectType.OT_INSTANCE:
                    return "instance";
                case SquirrelHelper.SQObjectType.OT_WEAKREF:
                    return "weakref";
                default:
                    throw new ArgumentOutOfRangeException(nameof(type), type, null);
            }
        }
    }
}
