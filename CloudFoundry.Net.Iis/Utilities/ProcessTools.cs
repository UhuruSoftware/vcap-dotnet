using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using System.Diagnostics;
using Microsoft.Win32.SafeHandles;
using System.IO;
using System.Threading;
using System.ComponentModel;

namespace CloudFoundry.Net.IIS.Utilities
{
    public class ProcessTools
    {
        static uint CREATE_NEW_PROCESS_GROUP = 0x00000200;

        public struct PROCESS_INFORMATION
        {
            public IntPtr hProcess;
            public IntPtr hThread;
            public uint dwProcessId;
            public uint dwThreadId;
        }

        public struct STARTUPINFO
        {
            public uint cb;
            public string lpReserved;
            public string lpDesktop;
            public string lpTitle;
            public uint dwX;
            public uint dwY;
            public uint dwXSize;
            public uint dwYSize;
            public uint dwXCountChars;
            public uint dwYCountChars;
            public uint dwFillAttribute;
            public uint dwFlags;
            public short wShowWindow;
            public short cbReserved2;
            public IntPtr lpReserved2;
            public IntPtr hStdInput;
            public IntPtr hStdOutput;
            public IntPtr hStdError;
        }

        public struct SECURITY_ATTRIBUTES
        {
            public int length;
            public IntPtr lpSecurityDescriptor;
            public bool bInheritHandle;
        }

        [Flags]
        public enum STARTF : uint
        {
            STARTF_USESHOWWINDOW = 0x00000001,
            STARTF_USESIZE = 0x00000002,
            STARTF_USEPOSITION = 0x00000004,
            STARTF_USECOUNTCHARS = 0x00000008,
            STARTF_USEFILLATTRIBUTE = 0x00000010,
            STARTF_RUNFULLSCREEN = 0x00000020,  // ignored for non-x86 platforms
            STARTF_FORCEONFEEDBACK = 0x00000040,
            STARTF_FORCEOFFFEEDBACK = 0x00000080,
            STARTF_USESTDHANDLES = 0x00000100,
        }

        [Flags]
        enum HANDLE_FLAGS
        {
            INHERIT = 1,
            PROTECT_FROM_CLOSE = 2
        }

        [DllImport("kernel32.dll")]
        static extern bool CreateProcess(string lpApplicationName, string lpCommandLine, IntPtr lpProcessAttributes, IntPtr lpThreadAttributes,
            bool bInheritHandles, uint dwCreationFlags, IntPtr lpEnvironment,
            string lpCurrentDirectory, ref STARTUPINFO lpStartupInfo, out PROCESS_INFORMATION lpProcessInformation);

        [DllImport("kernel32.dll")]
        static extern bool CreatePipe(out IntPtr hReadPipe, out IntPtr hWritePipe,
           ref SECURITY_ATTRIBUTES lpPipeAttributes, uint nSize);

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern bool SetHandleInformation(IntPtr hObject, HANDLE_FLAGS dwMask,
           HANDLE_FLAGS dwFlags);

        public uint StartProcess(string app, string commandLine, string dir, string logType, string logSource)
        {
            //TODO: vladi: need error handling

            STARTUPINFO si = CreateStartUpInfo(true);
            PROCESS_INFORMATION pi = new PROCESS_INFORMATION();

            CreateProcess(app,
                app + " " + commandLine, IntPtr.Zero,
                IntPtr.Zero, true, CREATE_NEW_PROCESS_GROUP,
                IntPtr.Zero, dir, ref si, out pi);

            if (pi.dwProcessId == 0)
            {
                if (pi.dwProcessId == 0)
                {
                    throw new Win32Exception();
                }
            }

            Thread stdReader = new Thread(new ThreadStart(delegate()
                {
                    string output;
                    while (readerStdOut.EndOfStream == false)
                    {
                        output = readerStdOut.ReadLine();
                        ProcessStartInfo psi = new ProcessStartInfo();
                        psi.Arguments = String.Format(
                            @"/t {1} /l application /so {2} /id 1 /d ""{0}""", output, logType, logSource);

                        psi.FileName = "eventcreate";
                        psi.CreateNoWindow = true;
                        psi.WindowStyle = ProcessWindowStyle.Hidden;

                        Process.Start(psi);
                    }
                }));
            stdReader.IsBackground = true;
            stdReader.Start();

            return pi.dwProcessId;
        }

























        SafeFileHandle shStdOutRead;
        StreamReader readerStdOut;
        SafeFileHandle shStdErrRead;
        StreamReader readerStdErr;
        SafeFileHandle shStdInWrite;
        StreamWriter writerStdIn;

        private STARTUPINFO CreateStartUpInfo(bool redirectStds)
        {
            SECURITY_ATTRIBUTES sa = new SECURITY_ATTRIBUTES();
            sa.length = Marshal.SizeOf(sa);
            sa.lpSecurityDescriptor = IntPtr.Zero;
            sa.bInheritHandle = true; // Set the bInheritHandle flag so pipe handles are inherited. 
            STARTUPINFO startupInfo = new STARTUPINFO();
            startupInfo.cb = (uint)Marshal.SizeOf(startupInfo);
            //startupInfo.lpDesktop = @"winsta0\default";
            if (redirectStds == false)
            {
                return startupInfo;
            }

            IntPtr hChildHandle, hHandle;

            bool success = CreatePipe(out hHandle, out hChildHandle, ref sa, 0);

            if (!success)
            {
                throw new System.ComponentModel.Win32Exception();
            }

            // assign write end of pipe to new process
            startupInfo.hStdOutput = hChildHandle;
            // Ensure the read handle to the pipe for STDOUT is not inherited as per SDK example
            success = SetHandleInformation(hHandle, HANDLE_FLAGS.INHERIT, 0);

            if (!success)
            {
                throw new System.ComponentModel.Win32Exception();
            }

            shStdOutRead = new SafeFileHandle(hHandle, true);
            FileStream fs = new FileStream(shStdOutRead, FileAccess.Read);
            readerStdOut = new StreamReader(fs);

            success = CreatePipe(out hHandle, out hChildHandle, ref sa, 0);

            if (!success)
            {
                throw new System.ComponentModel.Win32Exception();
            }

            // assign write end of pipe to new process
            startupInfo.hStdError = hChildHandle;

            // Ensure the read handle to the pipe for STDOUT is not inherited as per SDK example
            success = SetHandleInformation(hHandle, HANDLE_FLAGS.INHERIT, 0);

            if (!success)
            {
                throw new System.ComponentModel.Win32Exception();
            }

            shStdErrRead = new SafeFileHandle(hHandle, true);
            fs = new FileStream(shStdErrRead, FileAccess.Read);
            readerStdErr = new StreamReader(fs);

            // StdIn
            //
            success = CreatePipe(out hChildHandle, out hHandle, ref sa, 0);

            if (!success)
            {
                throw new System.ComponentModel.Win32Exception();
            }

            // assign READ end of pipe to new process
            startupInfo.hStdInput = hChildHandle;

            // Ensure the WRITE handle to the pipe for STDIN is not inherited as per SDK example
            success = SetHandleInformation(hHandle, HANDLE_FLAGS.INHERIT, 0);
            if (!success)
            {
                throw new System.ComponentModel.Win32Exception();
            }
            shStdInWrite = new SafeFileHandle(hHandle, true);
            fs = new FileStream(shStdInWrite, FileAccess.Write);
            writerStdIn = new StreamWriter(fs);
            startupInfo.dwFlags = (int)STARTF.STARTF_USESTDHANDLES;
            return startupInfo;
        }

        //public Process StartProcess(string commandLine, string workingDirectory, bool redirectStds)
        //{
        //    if (hToken == IntPtr.Zero)
        //    {
        //        throw new InvalidOperationException("Authentication Token has not been set with LogonUser");
        //    }

        //    SecurityAttributes sa = new SecurityAttributes();
        //    sa.dwLength = Marshal.SizeOf(sa);
        //    sa.lpSecurityDescriptor = IntPtr.Zero;
        //    sa.bInheritHandle = true; // TODO: don't know the implications of this
        //    StartUpInfo startupInfo = CreateStartUpInfo(redirectStds);
        //    ProcessInformation processInfo;
        //    bool result = CreateProcessAsUser(
        //        hToken, null, commandLine, ref sa, ref sa,
        //        true, (int)creationFlags, IntPtr.Zero, workingDirectory,
        //        ref startupInfo, out processInfo);

        //    if (result == false)
        //    {
        //        throw new System.ComponentModel.Win32Exception();
        //    }
        //    try
        //    {
        //        writerStdIn.Close();
        //        if (shStdInWrite.IsClosed == false)
        //        {
        //            shStdInWrite.Close();
        //        }

        //        string output;
        //        while (readerStdOut.EndOfStream == false)
        //        {
        //            output = readerStdOut.ReadLine();
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //    }

        //    CloseHandle(processInfo.hProcess);
        //    CloseHandle(processInfo.hThread);
        //    Process proc = Process.GetProcessById(processInfo.dwProcessId);
        //    return proc;
        //}
    }
}

