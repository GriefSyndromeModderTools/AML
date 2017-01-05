using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PluginUtils
{
    //higher priority makes plugin be loaded earlier
    public enum PluginLoadPriority
    {
        High,
        Normal,
        Low
    }

    [AttributeUsage(AttributeTargets.Class)]
    public class PluginAttribute : Attribute
    {
        public PluginAttribute()
        {
            Priority = PluginLoadPriority.Normal;
        }

        public string Name { get; set; }

        public string RawVersion { get; set; }

        public Version Version { get { return new Version(RawVersion == null ? "0.0" : RawVersion); } }

        public PluginLoadPriority Priority { get; set; }

        //null for independent plugin
        public Type DependentPlugin { get; set; }
        
        //specify plugins and optional lowest requested version which this plugin depends on, and they should be loaded before this plugin
        public Dictionary<string, Version> Dependencies { get; set; }

        //similar to above property but only check their existence
        public Dictionary<string, Version> WeakDependencies { get; set; }
    }
}
