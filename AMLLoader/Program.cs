using AMLLoader.NativeEnums;
using AMLLoader.NativeStructs;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using NamedPipeDelegateServer = AMLLoader.Network.NamedPipeDelegateServer;

namespace AMLLoader
{
    static class Program
    {
        /// <summary>
        /// 应用程序的主入口点。
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            const uint NORMAL_PRIORITY_CLASS = 0x00000020;
            const uint CREATE_SUSPENDED = 0x00000004;

            bool retValue;
            PROCESS_INFORMATION pInfo = new PROCESS_INFORMATION();
            STARTUPINFO sInfo = new STARTUPINFO();
            SECURITY_ATTRIBUTES pSec = new SECURITY_ATTRIBUTES();
            SECURITY_ATTRIBUTES tSec = new SECURITY_ATTRIBUTES();
            pSec.nLength = Marshal.SizeOf(pSec);
            tSec.nLength = Marshal.SizeOf(tSec);

            var processName = "griefsyndrome.exe";
            {
                int index = Array.FindIndex(args, x => x == "/process");
                if (index != -1 && index != args.Length - 1)
                {
                    processName = args[index + 1];
                    if (!processName.EndsWith(".exe"))
                    {
                        processName += ".exe";
                    }
                }
            }
            retValue = Natives.CreateProcess(processName, null,
                ref pSec, ref tSec, false, NORMAL_PRIORITY_CLASS | CREATE_SUSPENDED,
                IntPtr.Zero, null, ref sInfo, out pInfo);

            Thread.Sleep(250);

            //Start network delegate
            //TODO: move this to loader plugin of AGSO
            //NamedPipeDelegateServer.Run(pInfo.dwProcessId);

            //First call: LoadLibrary
            IntPtr injectedHandle;
            {
                var remoteAddr = WriteRemoteString(pInfo.hProcess, "aml/core/AMLInjected.dll");
                var pStart = Natives.GetProcAddress(Natives.GetModuleHandle("Kernel32"), "LoadLibraryW");

                IntPtr lpThreadID;
                var hThread = Natives.CreateRemoteThread(pInfo.hProcess, IntPtr.Zero, 0,
                    pStart, remoteAddr, 0, out lpThreadID);
                var ret = Natives.WaitForSingleObject(hThread, Natives.INFINITE);
                uint returnedValue;
                Natives.GetExitCodeThread(hThread, out returnedValue);
                injectedHandle = (IntPtr)returnedValue;
            }

            //Prepare for command-line arguments
            IntPtr dataPtr;
            {
                using (var ms = new MemoryStream())
                {
                    using (var bw = new BinaryWriter(ms))
                    {
                        bw.Write((int)0);

                        foreach (var arg in args)
                        {
                            bw.Write(arg);
                        }

                        bw.Flush();

                        var data = ms.ToArray();
                        Buffer.BlockCopy(BitConverter.GetBytes((int)data.Length), 0, data, 0, 4);
                        dataPtr = WriteRemoteData(pInfo.hProcess, data);
                    }
                }
            }

            //Second call: GetProcAddress
            IntPtr loader;
            {
                var remoteAddrProcName = WriteRemoteData(pInfo.hProcess, StringToByteArrayANSI("loadcore"));

                Int32[] ud = new Int32[3];
                ud[0] = Natives.GetProcAddress(Natives.GetModuleHandle("Kernel32"), "GetProcAddress").ToInt32(); ;
                ud[1] = injectedHandle.ToInt32();
                ud[2] = remoteAddrProcName.ToInt32();
                var udBytes = new byte[12];
                Buffer.BlockCopy(ud, 0, udBytes, 0, 12);
                var remoteAddrUd = WriteRemoteData(pInfo.hProcess, udBytes);

                var remoteAddrFunc = WriteRemoteData(pInfo.hProcess, GenerateGetProcAddress(remoteAddrUd));

                IntPtr lpThreadID;
                var hThread = Natives.CreateRemoteThread(pInfo.hProcess, IntPtr.Zero, 0,
                    remoteAddrFunc, IntPtr.Zero, 0, out lpThreadID);
                var ret = Natives.WaitForSingleObject(hThread, Natives.INFINITE);
                uint returnedValue;
                Natives.GetExitCodeThread(hThread, out returnedValue);
                loader = (IntPtr)returnedValue;
            }

            if (loader.ToInt32() == 0)
            {
                MessageBox.Show("Loader function not found.");
            }
            //Third call: run loader
            {
                IntPtr lpThreadID;
                var hThread = Natives.CreateRemoteThread(pInfo.hProcess, IntPtr.Zero, 0,
                    loader, dataPtr, 0, out lpThreadID);
                var ret = Natives.WaitForSingleObject(hThread, Natives.INFINITE);
                uint returnedValue;
                Natives.GetExitCodeThread(hThread, out returnedValue);
            }

            Natives.ResumeThread(pInfo.hThread);
        }

        private static byte[] StringToByteArray(string str)
        {
            var ret = new byte[str.Length * 2 + 2];
            Buffer.BlockCopy(str.ToCharArray(), 0, ret, 0, str.Length * 2);
            return ret;
        }

        private static byte[] StringToByteArrayANSI(string str)
        {
            var ret = new byte[str.Length + 1];
            for (int i = 0; i < str.Length; ++i)
            {
                ret[i] = (byte)str[i];
            }
            return ret;
        }

        private static IntPtr WriteRemoteString(IntPtr hProcess, string str)
        {
            var data = StringToByteArray(str);
            return WriteRemoteData(hProcess, data);
        }

        private static IntPtr WriteRemoteData(IntPtr hProcess, byte[] data)
        {
            var remoteAddr = Natives.VirtualAllocEx(hProcess, IntPtr.Zero, new IntPtr(data.Length),
                AllocationType.Commit, MemoryProtection.ExecuteReadWrite);

            IntPtr lpNumberOfBytesWritten;
            Natives.WriteProcessMemory(hProcess, remoteAddr, data, data.Length, out lpNumberOfBytesWritten);

            return remoteAddr;
        }

        private static byte[] GenerateGetProcAddress(IntPtr lpRemoteData)
        {
            //generate a function like this (in C)
            /*
             * DWORD STDCALL ThreadStart()
             * {
             *      struct {
             *          void *(*f)(HMODULE hModule, LPCSTR lpProcName);
             *          HMODULE hModule;
             *          LPCSTR lpProcName;
             *      } *pData = ????????;
             *      return pData->f(pData->hModule, pData->lpProcName);
             *  }
             */
            //assembly
            /*
             * ; start
             * push ebp
             * mov ebp, esp
             * 
             * ; save used registers (esi, edi, ebx)
             * push esi
             * 
             * mov esi, ????????       ; pData
             * push dword ptr[esi + 8] ; lpProcName
             * push dword ptr[esi + 4] ; hModule
             * mov esi, ptr[esi]       ; f
             * call esi
             * 
             * ; restore registers
             * pop esi
             * 
             * ; finish
             * pop ebp
             * retn 4
             */
            byte[] assemblyCode =
            {
                0x55,
                0x89, 0xE5,
                0x56,
                0xBE, 0x00, 0x00, 0x00, 0x00,
                0xFF, 0x76, 0x08,
                0xFF, 0x76, 0x04,
                0x8B, 0xB6, 0x00, 0x00, 0x00, 0x00,
                0xFF, 0xD6,
                0x5E,
                0x5D,
                0xC2, 0x04, 0x00,
            };
            byte[] d = BitConverter.GetBytes(lpRemoteData.ToInt32());
            Array.Copy(d, 0, assemblyCode, 5, 4);
            return assemblyCode;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct GetProcAddressDelegateUserData
        {
            public IntPtr lpFunc;
            public IntPtr hModule;
            public IntPtr lpProcName;
        }
    }
}
