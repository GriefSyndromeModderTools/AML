using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using PluginUtils;
using PluginUtils.Injection.Squirrel;
using Newtonsoft.Json;

namespace Debugger
{
    public interface IMessageHandler
    {
        void DebugInfomationArrived(int type, string srcname, int line, string funcname);
        void CompilerExceptionArrived(string exceptiondesc, string srcname, int line, int column);
        void RuntimeExceptionArrived(string exceptiondesc);
    }

    public class DebuggerPlugin : IAMLPlugin
    {
        private readonly List<IMessageHandler> _messageHandlers = new List<IMessageHandler>();
        private DebuggerWindow _window;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate void CompilerErrorHandler(
            IntPtr vm, [MarshalAs(UnmanagedType.LPStr)] string exceptiondesc,
            [MarshalAs(UnmanagedType.LPStr)] string srcname, int line, int column);

        public class Config
        {
            public Uri[] SourceUri { get; set; } = null;
        }

        public Config CurrentConfig { get; private set; }

        internal class GSPackRequestCreate : IWebRequestCreate
        {
            internal class GSPackRequest : WebRequest
            {
                
            }

            public WebRequest Create(Uri uri)
            {
                throw new NotImplementedException();
            }
        }

        public DebuggerPlugin()
        {
        }

        public void RegisterMessageHandler(IMessageHandler handler)
        {
            _messageHandlers.Add(handler);
        }

        public void Init()
        {
            MessageBox.Show("help!");
            WebRequest.RegisterPrefix("gspack", new GSPackRequestCreate());

            ReadConfig();
            SaveConfig();

            SquirrelHelper.RegisterGlobalFunction("DebugHook", vm =>
            {
                int type;
                string srcname;
                int line;
                string funcname;
                SquirrelFunctions.getinteger(vm, 2, out type);
                SquirrelFunctions.getstring(vm, 3, out srcname);
                SquirrelFunctions.getinteger(vm, 4, out line);
                SquirrelFunctions.getstring(vm, 5, out funcname);

                foreach (var handler in _messageHandlers)
                {
                    handler.DebugInfomationArrived(type, srcname, line, funcname);
                }

                return 0;
            });

            SquirrelHelper.RegisterGlobalFunction("ErrorHandler", vm =>
            {
                SquirrelFunctions.tostring(vm, 2);
                string exceptionDescription;
                SquirrelFunctions.getstring(vm, -1, out exceptionDescription);
                SquirrelFunctions.poptop(vm);

                foreach (var handler in _messageHandlers)
                {
                    handler.RuntimeExceptionArrived(exceptionDescription);
                }

                return 0;
            });

            SquirrelHelper.Run(vm =>
            {
                SquirrelFunctions.enabledebuginfo(vm, 1);

                SquirrelHelper.InjectCompileFileMain("data/script/boot.nut").AddBefore(sqvm =>
                {
                    SquirrelFunctions.pushroottable(vm);
                    SquirrelFunctions.pushstring(vm, "DebugHook", -1);
                    SquirrelFunctions.get(vm, -2);
                    SquirrelFunctions.setdebughook(vm);
                    SquirrelFunctions.pushstring(vm, "ErrorHandler", -1);
                    SquirrelFunctions.get(vm, -2);
                    SquirrelFunctions.seterrorhandler(vm);
                    SquirrelFunctions.poptop(vm);

                    var compilerErrorHandler =
                        Marshal.GetFunctionPointerForDelegate(
                            (CompilerErrorHandler) ((sqvm1, desc, src, line, column) =>
                            {
                                foreach (var handler in _messageHandlers)
                                {
                                    handler.CompilerExceptionArrived(desc, src, line, column);
                                }
                            }));

                    SquirrelFunctions.setcompilererrorhandler(vm, compilerErrorHandler);
                });
            });

            WindowsHelper.Run(() =>
            {
                _window = new DebuggerWindow(this);
                _window.Show();
            });
        }

        public void Load()
        {
        }

        public void ReadConfig()
        {
            try
            {
                if (!Directory.Exists("Config"))
                {
                    Directory.CreateDirectory("Config");
                    CurrentConfig = new Config();
                }
                else
                {
                    var configFile = new FileStream("Config/DebuggerConfig.json", FileMode.Open,
                            FileAccess.Read);

                    using (var reader = new StreamReader(configFile))
                    {
                        var configStr = reader.ReadToEnd();
                        var config = JsonConvert.DeserializeObject<Config>(configStr);
                        for (int i = 0; i < config.SourceUri.Length; ++i)
                        {
                            var uristr = config.SourceUri[i].OriginalString;
                            if (!uristr.EndsWith("/"))
                            {
                                config.SourceUri[i] = new Uri(uristr + "/");
                            }
                        }

                        CurrentConfig = config;
                    }
                }
            }
            catch
            {
                CurrentConfig = new Config();
            }
        }

        public void SaveConfig()
        {
            try
            {
                if (!Directory.Exists("Config"))
                {
                    Directory.CreateDirectory("Config");
                }
                if (File.Exists("Config/DebuggerConfig.json"))
                {
                    File.Delete("Config/DebuggerConfig.json");
                }
                var configFile = new FileStream("Config/DebuggerConfig.json", FileMode.OpenOrCreate, FileAccess.ReadWrite);
                var configStr = JsonConvert.SerializeObject(CurrentConfig);
                using (var writer = new StreamWriter(configFile))
                {
                    writer.Write(configStr);
                }
            }
            catch
            {
                // ignored
            }
        }

        public void SuspendVm()
        {
            SquirrelHelper.Run(vm =>
            {
                SquirrelFunctions.suspendvm(vm);
            });
        }

        public void WakeupVm(bool resumedret, bool retval, bool raiseerror)
        {
            SquirrelHelper.Run(vm =>
            {
                SquirrelFunctions.wakeupvm(vm, resumedret ? 1 : 0, retval ? 1 : 0, raiseerror ? 1 : 0);
            });
        }
    }
}
