namespace Uhuru.CloudFoundry.DEA
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Linq;
    using System.Runtime.InteropServices;
    using System.Text;
    using Uhuru.Utilities;
    using Uhuru.Utilities.WindowsJobObjects;

    public class ProcessPrison
    {
        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool CreateProcess(
            [In, Optional] string lpApplicationName,
            [In, Out, Optional] string lpCommandLine,
            [In, Optional] IntPtr lpProcessAttributes,
            [In, Optional] IntPtr lpThreadAttributes,
            [In] bool bInheritHandles,
            [In] ProcessCreationFlags dwCreationFlags,
            [In, Optional] string lpEnvironment,
            [In, Optional] string lpCurrentDirectory,
            [In] ref STARTUPINFO lpStartupInfo,
            [Out] out PROCESS_INFORMATION lpProcessInformation
            );

        [DllImport("Advapi32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool CreateProcessWithLogon(
            [In] string lpUsername,
            [In, Optional] string lpDomain,
            [In] string lpPassword,

            [In] LogonFlags dwLogonFlags,
            [In]  string lpApplicationName,
            [In, Out, Optional] string lpCommandLine,

            [In] ProcessCreationFlags dwCreationFlags,
            [In, Optional] string lpEnvironment,
            [In, Optional] string lpCurrentDirectory,
            [In] ref STARTUPINFO lpStartupInfo,
            [Out] out  PROCESS_INFORMATION lpProcessInfo
            );

        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        [return: MarshalAs(UnmanagedType.U4)]
        public static extern uint ResumeThread([In] IntPtr hThread);

        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        [return: MarshalAs(UnmanagedType.U4)]
        public static extern uint SuspendThread([In] IntPtr hThread);

        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool CloseHandle([In] IntPtr handle);

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        struct STARTUPINFO
        {
            public Int32 cb;
            public string lpReserved;
            public string lpDesktop;
            public string lpTitle;
            public Int32 dwX;
            public Int32 dwY;
            public Int32 dwXSize;
            public Int32 dwYSize;
            public Int32 dwXCountChars;
            public Int32 dwYCountChars;
            public Int32 dwFillAttribute;
            public Int32 dwFlags;
            public Int16 wShowWindow;
            public Int16 cbReserved2;
            public IntPtr lpReserved2;
            public IntPtr hStdInput;
            public IntPtr hStdOutput;
            public IntPtr hStdError;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        internal struct PROCESS_INFORMATION
        {
            public IntPtr hProcess;
            public IntPtr hThread;
            public int dwProcessId;
            public int dwThreadId;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        public struct SECURITY_ATTRIBUTES
        {
            public int nLength;
            public IntPtr lpSecurityDescriptor;
            public int bInheritHandle;
        }


        [Flags]
        public enum ProcessCreationFlags : uint
        {
            ZERO_FLAG = 0x00000000,
            CREATE_BREAKAWAY_FROM_JOB = 0x01000000,
            CREATE_DEFAULT_ERROR_MODE = 0x04000000,
            CREATE_NEW_CONSOLE = 0x00000010,
            CREATE_NEW_PROCESS_GROUP = 0x00000200,
            CREATE_NO_WINDOW = 0x08000000,
            CREATE_PROTECTED_PROCESS = 0x00040000,
            CREATE_PRESERVE_CODE_AUTHZ_LEVEL = 0x02000000,
            CREATE_SEPARATE_WOW_VDM = 0x00001000,
            CREATE_SHARED_WOW_VDM = 0x00001000,
            CREATE_SUSPENDED = 0x00000004,
            CREATE_UNICODE_ENVIRONMENT = 0x00000400,
            DEBUG_ONLY_THIS_PROCESS = 0x00000002,
            DEBUG_PROCESS = 0x00000001,
            DETACHED_PROCESS = 0x00000008,
            EXTENDED_STARTUPINFO_PRESENT = 0x00080000,
            INHERIT_PARENT_AFFINITY = 0x00010000
        }

        [Flags]
        enum LogonFlags
        {
            LOGON_WITH_PROFILE = 0x00000001,
            LOGON_NETCREDENTIALS_ONLY = 0x00000002
        }

        private ProcessPrisonCreateInfo createInfo;

        private JobObject jobObject;

        public string Id
        {
            get;
            private set;
        }

        public bool Created
        {
            get;
            private set;
        }

        public string WindowsUsername
        {
            get;
            private set;
        }

        public string WindowsUsernamePassword
        {
            get;
            private set;
        }

        public ProcessPrison()
        {
            this.Id = Guid.NewGuid().ToString();
        }

        public void Create()
        {
            Create(new ProcessPrisonCreateInfo());
        }

        public void Create(ProcessPrisonCreateInfo createInfo)
        {
            this.createInfo = createInfo;
            this.jobObject = new JobObject(this.Id);

            this.jobObject.ActiveProcessesLimit = this.createInfo.RunningProcessesLimit;
            this.jobObject.JobMemoryLimit = this.createInfo.TotalMemoryLimit;

            this.jobObject.KillProcessesOnJobClose = this.createInfo.TerminateContainerOnDispose;

            this.WindowsUsername = this.createInfo.WindowsUsername;
            this.WindowsUsernamePassword = this.createInfo.WindowsUsernamePassword;

            this.Created = true;
        }

        public void Destroy()
        {
            if (this.jobObject != null)
            {
                jobObject.TerminateProcesses(1);

                jobObject.Dispose();
                jobObject = null;
            }

            this.Created = false;
        }

        /// <summary>
        /// Runs a process in the current object container.
        /// Reference for starting a windows porcess in suspended mode:
        /// http://www.codeproject.com/Articles/230005/Launch-a-process-suspended
        /// </summary>
        /// <param name="executablePath"></param>
        public void RunProcess(string executablePath)
        {
            var runInfo = new ProcessPrisonRunInfo() { FileName = executablePath };
            this.RunProcess(runInfo);
        }

        public Process RunProcess(ProcessPrisonRunInfo runInfo)
        {
            if (!this.Created)
            {
                throw new InvalidOperationException("ProcessPrison has to be created first.");
            }

            var startupInfo = new STARTUPINFO();
            var processInfo = new PROCESS_INFORMATION();

            startupInfo.cb = Marshal.SizeOf(startupInfo.GetType());
            // startupInfo.dwFlags = 0x00000100;

            string env = BuildEnvironmentVariable(runInfo.EnvironmentVariables);

            var creationFlags = ProcessCreationFlags.ZERO_FLAG;

            creationFlags &= ~ProcessCreationFlags.CREATE_PRESERVE_CODE_AUTHZ_LEVEL;

            // Default creation flags for the CreateProcessWithLogonW API call
            creationFlags |=
                ProcessCreationFlags.CREATE_DEFAULT_ERROR_MODE | 
                // ProcessCreationFlags.CREATE_NEW_CONSOLE | 
                ProcessCreationFlags.CREATE_NEW_PROCESS_GROUP;

            // Just to be sure nothing is shared with other processes
            creationFlags |= ProcessCreationFlags.CREATE_SEPARATE_WOW_VDM;

            creationFlags |=
                ProcessCreationFlags.CREATE_SUSPENDED |
                ProcessCreationFlags.CREATE_UNICODE_ENVIRONMENT |
                ProcessCreationFlags.CREATE_NO_WINDOW;

            if (string.IsNullOrEmpty(this.WindowsUsername))
            {
                // Create the process in suspended mode to fence it with a Windows Job Object 
                // before it executes.
                bool ret = CreateProcess(
                    runInfo.FileName,
                    runInfo.Arguments,
                    IntPtr.Zero, IntPtr.Zero, false,
                    creationFlags,
                    env,
                    runInfo.WorkingDirectory,
                    ref startupInfo,
                    out processInfo
                    );

                if (!ret)
                {
                    throw new Win32Exception(Marshal.GetLastWin32Error());
                }
            }
            else
            {
                // Create the process in suspended mode to fence it with a Windows Job Object 
                // before it executes.
                bool ret = CreateProcessWithLogon(
                    this.WindowsUsername,
                    ".",
                    this.WindowsUsernamePassword,
                    LogonFlags.LOGON_WITH_PROFILE,
                    runInfo.FileName,
                    runInfo.Arguments,
                    creationFlags,
                    env,
                    runInfo.WorkingDirectory,
                    ref startupInfo,
                    out processInfo
                    );

                if (!ret)
                {
                    throw new Win32Exception(Marshal.GetLastWin32Error());
                }
            }

            var process = Process.GetProcessById(processInfo.dwProcessId);

            this.jobObject.AddProcess(process);

            uint ret2 = ResumeThread(processInfo.hThread);
            if (ret2 != 1)
            {
                throw new Win32Exception(Marshal.GetLastWin32Error());
            }

            CloseHandle(processInfo.hProcess);
            CloseHandle(processInfo.hThread);

            return process;
        }

        public Process[] GetRunningProcesses()
        {
            throw new NotImplementedException();
        }



        private static string BuildEnvironmentVariable(Dictionary<string, string> EvnironmantVariables)
        {
            string ret = null;
            if (EvnironmantVariables.Count > 0)
            {
                foreach (var EnvironmentVariable in EvnironmantVariables)
                {
                    if (EnvironmentVariable.Key.Contains('=') || EnvironmentVariable.Key.Contains('\0') || EnvironmentVariable.Value.Contains('\0'))
                    {
                        throw new ArgumentException("Invalid or restricted charachter", "EvnironmantVariables");
                    }

                    ret += EnvironmentVariable.Key + "=" + EnvironmentVariable.Value + '\0';
                }

                // See env format here: http://msdn.microsoft.com/en-us/library/windows/desktop/ms682425(v=vs.85).aspx
                ret += "\0\0";
            }
            return ret;
        }

    }
}
