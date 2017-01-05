using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PluginUtils
{
    public class AMLException : Exception
    {
        public AMLException()
        {
        }

        public AMLException(string message) : base(message)
        {
        }

        public AMLException(string message, Exception innerException) : base(message, innerException)
        { 
        }
    }

    public class PluginException : AMLException
    {
        public Type ConcernedPlugin { get; }

        public PluginException()
        {
            ConcernedPlugin = null;
        }

        public PluginException(string message) : this(null, message)
        {
        }

        public PluginException(Type pluginType, string message) : base(message)
        {
            ConcernedPlugin = pluginType;
        }
    }
}
