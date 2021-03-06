﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using KeyConfig = PluginUtils.Injection.Input.KeyConfigInjector;

namespace AGSO.Core.Connection
{
    class ClientSequenceHandler : SequenceHandler
    {
        private IClientSequenceExceptionHandler _Exception;

        public ClientSequenceHandler(IClientSequenceExceptionHandler exceptionHandler) :
            base(28)
        {
            _Exception = exceptionHandler;
        }

        protected override bool CheckInterpolate(byte length, byte a, byte b)
        {
            return SimpleByteData.CheckInterpolate(length, a, b);
        }

        protected override byte Interpolate(byte length, byte t, byte a, byte b)
        {
            return SimpleByteData.Interpolate(length, t, a, b);
        }

        protected override void WaitFor(byte time)
        {
            _Exception.WaitFor(time);
        }

        protected override void ResetAll(long time)
        {
            _Exception.ResetAll(time);
        }

        public void Next(IntPtr ptr)
        {
            var data = Next();
            NetworkLogHelper.Write("Client next", data);
            for (int i = 0; i < 27; ++i)
            {
                bool k = SimpleByteData.Status(data[i + 1]);
                Marshal.WriteByte(ptr, KeyConfig.GetInjectedKeyIndex(i), (byte)(k ? 0x80 : 0));
            }
        }
    }
}
