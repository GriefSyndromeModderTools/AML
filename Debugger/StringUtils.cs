using System.IO;

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
    }
}
