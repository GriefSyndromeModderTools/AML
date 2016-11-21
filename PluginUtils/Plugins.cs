using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PluginUtils
{
    public class Plugins
    {
        private static IAMLPluginManager _Manager;
        public static IAMLPluginManager Manager
        {
            get
            {
                return _Manager;
            }
            set
            {
                if (_Manager != null)
                {
                    throw new InvalidOperationException();
                }
                _Manager = value;
            }
        }

        public static T GetPlugin<T>() where T : IAMLPlugin
        {
            if (_Manager == null)
            {
                throw new InvalidOperationException();
            }
            return _Manager.GetPlugin<T>();
        }
    }
}
