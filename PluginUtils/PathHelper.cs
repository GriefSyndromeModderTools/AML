using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PluginUtils
{
    public class PathHelper
    {
        private static string _GamePath;

        static PathHelper()
        {
            var uri = new UriBuilder(typeof(PathHelper).Assembly.CodeBase).Path;
            var core = Path.GetDirectoryName(Uri.UnescapeDataString(uri));
            var aml = Path.GetDirectoryName(core);
            _GamePath = Path.GetDirectoryName(aml);
        }

        public static string GetPath(string rel)
        {
            return Path.GetFullPath(Path.Combine(_GamePath, rel));
        }
    }
}
