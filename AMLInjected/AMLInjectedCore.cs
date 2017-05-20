using RGiesecke.DllExport;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AMLInjected
{
    //Be careful when modifying this class.
    //It CANNOT directly reference class in PluginUtils project, or loading will fail.
    public class AMLInjectedCore
    {
        [DllExport("loadcore")]
        public static uint LoadCore(IntPtr ud)
        {
            MessageBox.Show("LoadCore");
            var uri = new UriBuilder(typeof(AMLInjectedCore).Assembly.CodeBase).Path;
            var dir = Path.GetDirectoryName(Uri.UnescapeDataString(uri));
            var dllFiles = Directory.EnumerateFiles(Path.Combine(dir, "../mods"), "*.dll",
                SearchOption.TopDirectoryOnly);
            
            AppDomain.CurrentDomain.AppendPrivatePath("aml/core");
            AppDomain.CurrentDomain.AppendPrivatePath("aml/mods");

            Helper.SetupArgs(ud);
            Helper.LoadPlugins(dllFiles);
            
            Helper.LogSystem("Core loaded");
            return 0;
        }
    }
}
