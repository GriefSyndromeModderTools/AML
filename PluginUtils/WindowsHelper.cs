﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PluginUtils
{
    public class WindowsHelper
    {
        private static Thread _WindowsThread;
        private static ConcurrentQueue<Action> _Queue = new ConcurrentQueue<Action>();

        static WindowsHelper()
        {
            _WindowsThread = new Thread(WindowsThreadStart);
            _WindowsThread.SetApartmentState(ApartmentState.STA);
            _WindowsThread.Start();
        }

        public static void MessageBox(string text)
        {
            System.Windows.Forms.MessageBox.Show(text);
        }

        private static void WindowsThreadStart()
        {
            Action a;
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            while (true)
            {
                while (_Queue.TryDequeue(out a))
                {
                    DoEvents();
                    a.Invoke();
                }
                Thread.Sleep(10);
                DoEvents();
            }
        }

        public static void Run(Action callback)
        {
            if (Thread.CurrentThread == _WindowsThread)
            {
                callback();
            }
            _Queue.Enqueue(callback);
        }

        public static void RunAndWait(Action a)
        {
            var wait = new Wait { Original = a };
            Run(wait.Run);
            while (!wait.Finished)
            {
                Thread.Sleep(5);
            }
        }

        private class Wait
        {
            public volatile bool Finished;
            public Action Original;

            public void Run()
            {
                Original();
                Finished = true;
            }
        }

        //use the same method as SharpDX

        [StructLayout(LayoutKind.Sequential)]
        public struct NativeMessage
        {
            public IntPtr handle;
            public uint msg;
            public IntPtr wParam;
            public IntPtr lParam;
            public uint time;
        }

        [DllImport("user32.dll", EntryPoint = "PeekMessage"), SuppressUnmanagedCodeSecurity]
        public static extern int PeekMessage(out NativeMessage lpMsg, IntPtr hWnd, int wMsgFilterMin,
                                              int wMsgFilterMax, int wRemoveMsg);

        [DllImport("user32.dll", EntryPoint = "GetMessage"), SuppressUnmanagedCodeSecurity]
        public static extern int GetMessage(out NativeMessage lpMsg, IntPtr hWnd, int wMsgFilterMin,
                                             int wMsgFilterMax);

        [DllImport("user32.dll", EntryPoint = "TranslateMessage"), SuppressUnmanagedCodeSecurity]
        public static extern int TranslateMessage(ref NativeMessage lpMsg);

        [DllImport("user32.dll", EntryPoint = "DispatchMessage"), SuppressUnmanagedCodeSecurity]
        public static extern int DispatchMessage(ref NativeMessage lpMsg);

        private static void DoEvents()
        {
            NativeMessage msg;
            while (PeekMessage(out msg, IntPtr.Zero, 0, 0, 0) != 0)
            {
                if (GetMessage(out msg, IntPtr.Zero, 0, 0) == -1)
                {
                    return;
                }

                var message = new Message() { HWnd = msg.handle, LParam = msg.lParam, Msg = (int)msg.msg, WParam = msg.wParam };
                if (!Application.FilterMessage(ref message))
                {
                    TranslateMessage(ref msg);
                    DispatchMessage(ref msg);
                }
            }
        }
    }
}
