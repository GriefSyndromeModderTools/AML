using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PluginUtils.Injection.Input
{
    public class InputManager
    {
        private static List<IInputHandler> _Hanlders = new List<IInputHandler>();
        private static object _Mutex = new object();
        private static bool _RunFP;

        internal static bool HandleAll(IntPtr ptr)
        {
            lock (_Mutex)
            {
                if (!_RunFP)
                {
                    _RunFP = true;
                    FPCtrl.Reset();
                }
                foreach (var h in _Hanlders)
                {
                    if (h.HandleInput(ptr))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        public static void RegisterHandler(IInputHandler h)
        {
            lock (_Mutex)
            {
                _Hanlders.Add(h);
            }
        }
    }
}
