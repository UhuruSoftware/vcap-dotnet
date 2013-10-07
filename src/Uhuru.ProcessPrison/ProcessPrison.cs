namespace Uhuru.Isolation
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.Specialized;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Linq;
    using System.Runtime.InteropServices;
    using System.Security.Cryptography;
    using System.Text;
    using System.Threading;
    using DiskQuotaTypeLibrary;
    using Microsoft.Win32;
    using Uhuru.Utilities;
    using Uhuru.Utilities.WindowsJobObjects;
    using System.Runtime.ConstrainedExecution;
    using System.IO;

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






        [Flags]
        public enum WINDOWS_STATION_ACCESS_MASK : uint
        {
            WINSTA_NONE = 0,

            WINSTA_ENUMDESKTOPS = 0x0001,
            WINSTA_READATTRIBUTES = 0x0002,
            WINSTA_ACCESSCLIPBOARD = 0x0004,
            WINSTA_CREATEDESKTOP = 0x0008,
            WINSTA_WRITEATTRIBUTES = 0x0010,
            WINSTA_ACCESSGLOBALATOMS = 0x0020,
            WINSTA_EXITWINDOWS = 0x0040,
            WINSTA_ENUMERATE = 0x0100,
            WINSTA_READSCREEN = 0x0200,
        }

        [DllImport("user32.dll", EntryPoint = "CreateWindowStation", CharSet = CharSet.Unicode, SetLastError = true)]
        public static extern IntPtr CreateWindowStation(
                        [MarshalAs(UnmanagedType.LPWStr)] string name,
                        [MarshalAs(UnmanagedType.U4)] int reserved,      // must be zero.
                        [MarshalAs(UnmanagedType.U4)] WINDOWS_STATION_ACCESS_MASK desiredAccess,
                        [MarshalAs(UnmanagedType.LPStruct)] SecurityAttributes attributes);


        [StructLayout(LayoutKind.Sequential)]
        public class SecurityAttributes
        {
            #region Struct members
            [MarshalAs(UnmanagedType.U4)]
            private int mStuctLength;

            private IntPtr mSecurityDescriptor;

            [MarshalAs(UnmanagedType.U4)]
            private bool mInheritHandle;
            #endregion

            public SecurityAttributes()
            {
                mStuctLength = Marshal.SizeOf(typeof(SecurityAttributes));
                mSecurityDescriptor = IntPtr.Zero;
            }

            public IntPtr SecurityDescriptor
            {
                get { return mSecurityDescriptor; }
                set { mSecurityDescriptor = value; }
            }

            public bool Inherit
            {
                get { return mInheritHandle; }
                set { mInheritHandle = value; }
            }
        }

        [DllImport("user32.dll", SetLastError = true)]
        public static extern bool SetProcessWindowStation(IntPtr hWinSta);

        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
        [DllImport("user32", CharSet = CharSet.Unicode, SetLastError = true)]
        public static extern IntPtr GetProcessWindowStation();


        // ms-help://MS.VSCC.v80/MS.MSDN.v80/MS.WIN32COM.v10.en/dllproc/base/createdesktop.htm
        [DllImport("user32.dll", EntryPoint = "CreateDesktop", CharSet = CharSet.Unicode, SetLastError = true)]
        public static extern IntPtr CreateDesktop(
                        [MarshalAs(UnmanagedType.LPWStr)] string desktopName,
                        [MarshalAs(UnmanagedType.LPWStr)] string device, // must be null.
                        [MarshalAs(UnmanagedType.LPWStr)] string deviceMode, // must be null,
                        [MarshalAs(UnmanagedType.U4)] int flags,  // use 0
                        [MarshalAs(UnmanagedType.U4)] ACCESS_MASK accessMask,
                        [MarshalAs(UnmanagedType.LPStruct)] SecurityAttributes attributes);


        [Flags]
        public enum ACCESS_MASK : uint
        {
            DELETE = 0x00010000,
            READ_CONTROL = 0x00020000,
            WRITE_DAC = 0x00040000,
            WRITE_OWNER = 0x00080000,
            SYNCHRONIZE = 0x00100000,

            STANDARD_RIGHTS_REQUIRED = 0x000f0000,

            STANDARD_RIGHTS_READ = 0x00020000,
            STANDARD_RIGHTS_WRITE = 0x00020000,
            STANDARD_RIGHTS_EXECUTE = 0x00020000,

            STANDARD_RIGHTS_ALL = 0x001f0000,

            SPECIFIC_RIGHTS_ALL = 0x0000ffff,

            ACCESS_SYSTEM_SECURITY = 0x01000000,

            MAXIMUM_ALLOWED = 0x02000000,

            GENERIC_READ = 0x80000000,
            GENERIC_WRITE = 0x40000000,
            GENERIC_EXECUTE = 0x20000000,
            GENERIC_ALL = 0x10000000,

            DESKTOP_READOBJECTS = 0x00000001,
            DESKTOP_CREATEWINDOW = 0x00000002,
            DESKTOP_CREATEMENU = 0x00000004,
            DESKTOP_HOOKCONTROL = 0x00000008,
            DESKTOP_JOURNALRECORD = 0x00000010,
            DESKTOP_JOURNALPLAYBACK = 0x00000020,
            DESKTOP_ENUMERATE = 0x00000040,
            DESKTOP_WRITEOBJECTS = 0x00000080,
            DESKTOP_SWITCHDESKTOP = 0x00000100,

            WINSTA_ENUMDESKTOPS = 0x00000001,
            WINSTA_READATTRIBUTES = 0x00000002,
            WINSTA_ACCESSCLIPBOARD = 0x00000004,
            WINSTA_CREATEDESKTOP = 0x00000008,
            WINSTA_WRITEATTRIBUTES = 0x00000010,
            WINSTA_ACCESSGLOBALATOMS = 0x00000020,
            WINSTA_EXITWINDOWS = 0x00000040,
            WINSTA_ENUMERATE = 0x00000100,
            WINSTA_READSCREEN = 0x00000200,

            WINSTA_ALL_ACCESS = 0x0000037f
        }


        private ProcessPrisonCreateInfo createInfo;

        [CLSCompliant(false)]
        private DIDiskQuotaUser userQuota;

        public JobObject jobObject
        {
            get;
            private set;
        }

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

        private string WindowsDomain
        {
            get;
            set;
        }

        public string WindowsPassword
        {
            get;
            private set;
        }

        public long DiskUsageBytes
        {
            get
            {
                if (userQuota == null) return -1;

                // Invalidate will update quota use
                this.userQuota.Invalidate();
                return (long)this.userQuota.QuotaUsed;
            }
        }


        /// <summary>
        /// Gets the private memory in bytes. It also counts the swapped memory. 
        /// This is the the type of memory enforced by the Job Object memory limit.
        /// Coresponds to Private Bytes in Process Hacker and Process Explorer, Commit Size in Task Manager, .
        /// </summary>
        public long PrivateVirtualMemoryUsageBytes
        {
            get
            {
                return this.jobObject.PrivateMemory;
            }
        }

        public ProcessPrison()
        {
            this.WindowsDomain = ".";
        }

        public void Create()
        {
            Create(new ProcessPrisonCreateInfo());
        }

        public void Create(ProcessPrisonCreateInfo createInfo)
        {
            if (createInfo.Id == null)
                this.Id = GenerateSecureGuid().ToString();
            else
                this.Id = createInfo.Id;


            this.createInfo = createInfo;
            this.jobObject = new JobObject(JobObjectNamespace() + this.Id);

            this.jobObject.ActiveProcessesLimit = this.createInfo.RunningProcessesLimit;
            this.jobObject.JobMemoryLimitBytes = this.createInfo.TotalPrivateMemoryLimitBytes;

            this.jobObject.KillProcessesOnJobClose = this.createInfo.KillProcessesrOnPrisonClose;


            if (this.createInfo.WindowsPassword == null)
                this.WindowsPassword = GenerateSecurePassword(40);
            else
                this.WindowsPassword = this.createInfo.WindowsPassword;


            this.WindowsUsername = CreateDecoratedUser(this.Id, this.WindowsPassword);


            if (this.createInfo.DiskQuotaBytes > -1)
            {
                if (string.IsNullOrEmpty(this.createInfo.DiskQuotaPath))
                {
                    // set this.createInfo.DiskQuotaPath to the output of GetUserProfileDirectory  
                    throw new NotImplementedException();
                }

                // Set the disk quota to 0 for all disks, exept disk quota path
                var volumesQuotas = DiskQuotaManager.GetDisksQuotaUser(this.WindowsUsername);
                foreach (var volumeQuota in volumesQuotas)
                {
                    volumeQuota.QuotaLimit = 0;
                }

                userQuota = DiskQuotaManager.GetDiskQuotaUser(DiskQuotaManager.GetVolumeRootFromPath(this.createInfo.DiskQuotaPath), this.WindowsUsername);
                userQuota.QuotaLimit = this.createInfo.DiskQuotaBytes;
            }

            if (this.createInfo.UrlPortAccess > 0)
            {
                UrlsAcl.AddPortAccess(this.createInfo.UrlPortAccess, this.WindowsUsername);
            }

            if (this.createInfo.NetworkOutboundRateLimitBitsPerSecond > 0)
            {
                NetworkQos.CreateOutboundThrottlePolicy(this.WindowsUsername, this.WindowsUsername, this.createInfo.NetworkOutboundRateLimitBitsPerSecond);

                if (this.createInfo.UrlPortAccess > 0)
                {
                    NetworkQos.CreateOutboundThrottlePolicy(this.createInfo.UrlPortAccess.ToString(), this.createInfo.UrlPortAccess, this.createInfo.NetworkOutboundRateLimitBitsPerSecond);
                }
            }

            this.Created = true;
        }

        public void Attach(ProcessPrisonCreateInfo createInfo)
        {
            if (createInfo.Id == null)
            {
                throw new ArgumentException("Id from createInfo is null", "createInfo");
            }

            if (createInfo.WindowsPassword == null)
            {
                throw new ArgumentException("WindowsPassword from createInfo is null", "createInfo");
            }

            this.Id = createInfo.Id;

            this.createInfo = createInfo;

            // The Job Object will disapear after a reboot or if all job's processes exit.
            // It is fine if it is created again with the same name id if the Job doesn't exist.

            try
            {
                // try only to attach and fail if it doesn't exist
                this.jobObject = JobObject.Attach(JobObjectNamespace() + this.Id);
            }
            catch (Win32Exception)
            {
                // try to create the job Id;
                this.jobObject = new JobObject(JobObjectNamespace() + this.Id);
            }


            this.WindowsPassword = this.createInfo.WindowsPassword;
            this.WindowsUsername = GenerateDecoratedUsername(this.Id);

            if (this.createInfo.DiskQuotaBytes > -1)
            {
                userQuota = DiskQuotaManager.GetDiskQuotaUser(DiskQuotaManager.GetVolumeRootFromPath(this.createInfo.DiskQuotaPath), this.WindowsUsername);
            }

            this.Created = true;
        }

        public void Destroy()
        {
            this.TerminateProcesses();

            if (this.createInfo.NetworkOutboundRateLimitBitsPerSecond > 0)
            {
                NetworkQos.RemoveOutboundThrottlePolicy(this.WindowsUsername);

                if (this.createInfo.UrlPortAccess > 0)
                {
                   NetworkQos.RemoveOutboundThrottlePolicy(this.createInfo.UrlPortAccess.ToString());
                }
            }
            
            if (this.createInfo.UrlPortAccess > 0)
            {
                UrlsAcl.RemovePortAccess(this.createInfo.UrlPortAccess);
            }

            UserImpersonator.DeleteUserProfile(this.WindowsUsername, "");
            WindowsUsersAndGroups.DeleteUser(this.WindowsUsername);

            if (this.jobObject != null)
            {
                jobObject.Dispose();
                jobObject = null;
            }

            this.Created = false;
        }

        public void TerminateProcesses()
        {
            if (this.jobObject != null)
            {
                jobObject.TerminateProcesses(1);
            }
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

                private static readonly object windowStationLock = new object();

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


            var creationFlags = ProcessCreationFlags.ZERO_FLAG;

            creationFlags &=
                ~ProcessCreationFlags.CREATE_PRESERVE_CODE_AUTHZ_LEVEL;

            creationFlags |=
                ProcessCreationFlags.CREATE_SEPARATE_WOW_VDM |

                ProcessCreationFlags.CREATE_DEFAULT_ERROR_MODE |
                ProcessCreationFlags.CREATE_NEW_PROCESS_GROUP |
                ProcessCreationFlags.CREATE_NO_WINDOW |
                ProcessCreationFlags.CREATE_SUSPENDED |
                ProcessCreationFlags.CREATE_UNICODE_ENVIRONMENT;

            if (runInfo.Interactive)
            {
                creationFlags |= ProcessCreationFlags.CREATE_NEW_CONSOLE;
                // startupInfo.lpDesktop = @"winsta0\default"; // set additional ACLs for the user to have access to winsta0/default
            }
            else
            {
                creationFlags |= ProcessCreationFlags.CREATE_NO_WINDOW;
                // http://support.microsoft.com/kb/165194
                // startupInfo.lpDesktop = this.Id + "\\" + "default";
                // TODO: isolate the Windows Station and Destop
            }





            if (string.IsNullOrEmpty(this.WindowsUsername))
            {
                throw new InvalidOperationException("This should not be run without a user");
            }
            else
            {

                SECURITY_ATTRIBUTES secAttributes = new SECURITY_ATTRIBUTES();
                secAttributes.nLength = Marshal.SizeOf(secAttributes);

                IntPtr windowStation = CreateWindowStation(this.WindowsUsername, 0, WINDOWS_STATION_ACCESS_MASK.WINSTA_NONE, null);
                IntPtr desktop = IntPtr.Zero;

                lock (windowStationLock)
                {
                    IntPtr currentWindowStation = GetProcessWindowStation();

                    try
                    {
                        bool setOk = SetProcessWindowStation(windowStation);

                        if (!setOk)
                        {
                            throw new Win32Exception(Marshal.GetLastWin32Error());
                        }

                        CreateDesktop(this.WindowsUsername, null, null, 0, ACCESS_MASK.DESKTOP_CREATEWINDOW, null);

                        var envMap = new Dictionary<string, string>();
                        envMap["Method"] = "CreateProcessWithLogonW";
                        envMap["Username"] = this.WindowsUsername;
                        envMap["Domain"] = ".";
                        envMap["Password"] = this.WindowsPassword;
                        envMap["LogonFlags"] = ((int)LogonFlags.LOGON_WITH_PROFILE).ToString();
                        envMap["CommandLine"] = '"' + runInfo.FileName + "\" " + runInfo.Arguments;
                        envMap["CreationFlags"] = ((int)creationFlags).ToString();
                        envMap["CurrentDirectory"] = runInfo.WorkingDirectory;
                        envMap["Desktop"] = string.Format(@"{0}\{0}", this.WindowsUsername);

                        string env = BuildEnvironmentVariable(envMap);

                        // startupInfo.lpDesktop = string.Format(@"{0}\{0}", this.WindowsUsername);

                        // Create the process in suspended mode to fence it with a Windows Job Object 
                        // before it executes.
                        // TODO: Use CreateProcessWithToken to prevent Windows for creating an unamed job object for the
                        // second created process.
                        // http://stackoverflow.com/questions/1287620/createprocesswithlogonw-and-assignprocesstojobobject

                        string processPath = Process.GetCurrentProcess().MainModule.FileName;
                        string processDirPath = Directory.GetParent(processPath).FullName;

                        bool ret = CreateProcess(
                            null,
                            processDirPath + @"\Uhuru.WindowsIsolation.CreateProcessDelegate.exe",
                            IntPtr.Zero,
                            IntPtr.Zero,
                            true,
                            creationFlags,
                            env,
                            null,
                            ref startupInfo,
                            out processInfo
                            );

                        if (!ret)
                        {
                            throw new Win32Exception(Marshal.GetLastWin32Error());
                        }
                    }
                    finally
                    {
                        SetProcessWindowStation(currentWindowStation);
                    }
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

        /// <summary>
        /// Sets an environment variable for the user.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="value"></param>
        public void SetUsersEnvironmentVariable(string name, string value)
        {
            if (!this.Created)
            {
                throw new InvalidOperationException("ProcessPrison has to be created first.");
            }

            if (string.IsNullOrEmpty(name) || name.Contains('='))
            {
                throw new ArgumentException("Invalid name", "name");
            }

            if (value == null)
            {
                throw new ArgumentException("Value is null", "value");
            }


            using (var impersonator = new UserImpersonator(this.WindowsUsername, this.WindowsDomain, this.WindowsPassword, true))
            {
                using (var registryHandle = impersonator.GetRegistryHandle())
                {
                    using (var registry = RegistryKey.FromHandle(registryHandle))
                    {
                        var envRegKey = registry.OpenSubKey("Environment", true);

                        envRegKey.SetValue(name, value, RegistryValueKind.String);
                    }
                }
            }
        }

        /// <summary>
        /// Sets an environment variable for the user.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="value"></param>
        public void SetUsersEnvironmentVariables(Dictionary<string, string> envVariables)
        {
            if (!this.Created)
            {
                throw new InvalidOperationException("ProcessPrison has to be created first.");
            }

            if (envVariables.Keys.Any(x => x.Contains('=')))
            {
                throw new ArgumentException("A name of an environment variable contains the invalid '=' characther", "envVariables");
            }

            if (envVariables.Keys.Any(x => string.IsNullOrEmpty(x)))
            {
                throw new ArgumentException("A name of an environment variable is null or empty", "envVariables");
            }

            //if (envVariables.Values.Any(x => x == null))
            //{
            //    throw new ArgumentException("A value of an environment variable is null", "envVariables");
            //}

            using (var impersonator = new UserImpersonator(this.WindowsUsername, this.WindowsDomain, this.WindowsPassword, true))
            {
                using (var registryHandle = impersonator.GetRegistryHandle())
                {
                    using (var registry = RegistryKey.FromHandle(registryHandle))
                    {
                        var envRegKey = registry.OpenSubKey("Environment", true);

                        foreach (var env in envVariables)
                        {
                            var value = env.Value == null ? string.Empty : env.Value;

                            envRegKey.SetValue(env.Key, value, RegistryValueKind.String);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Gets the environment variables for the user.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="value"></param>
        public IDictionary<string, string> GetUsersEnvironmentVariables()
        {
            if (!this.Created)
            {
                throw new InvalidOperationException("ProcessPrison has to be created first.");
            }

            var ret = new Dictionary<string, string>();

            using (var impersonator = new UserImpersonator(this.WindowsUsername, this.WindowsDomain, this.WindowsPassword, true))
            {
                using (var registryHandle = impersonator.GetRegistryHandle())
                {
                    using (var registry = RegistryKey.FromHandle(registryHandle))
                    {
                        var envRegKey = registry.OpenSubKey("Environment", true);

                        foreach (var key in envRegKey.GetValueNames())
                        {
                            ret[key] = (string)envRegKey.GetValue(key);
                        }
                    }
                }
            }

            return ret;
        }

        public Process[] GetRunningProcesses()
        {
            throw new NotImplementedException();
        }


        /// <summary>
        /// Formats a string with the env variables for CreateProcess Win API function.
        /// See env format here: http://msdn.microsoft.com/en-us/library/windows/desktop/ms682425(v=vs.85).aspx
        /// </summary>
        /// <param name="EvnironmantVariables"></param>
        /// <returns></returns>
        private static string BuildEnvironmentVariable(Dictionary<string, string> EvnironmantVariables)
        {
            string ret = null;
            if (EvnironmantVariables.Count > 0)
            {
                foreach (var EnvironmentVariable in EvnironmantVariables)
                {
                    var value = EnvironmentVariable.Value;
                    if (value == null) value = "";

                    if (EnvironmentVariable.Key.Contains('=') || EnvironmentVariable.Key.Contains('\0') || value.Contains('\0'))
                    {
                        throw new ArgumentException("Invalid or restricted charachter", "EvnironmantVariables");
                    }

                    ret += EnvironmentVariable.Key + "=" + value + '\0';
                }


                ret += "\0\0";
            }
            return ret;
        }


        public static string GenerateSecurePassword(int base64Length)
        {
            int bytesLength = (base64Length * 3) / 4 + 1;
            var rnd = new byte[bytesLength];

            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(rnd);
            }

            return System.Convert.ToBase64String(rnd).Substring(0, base64Length);
        }

        /// <summary>
        /// Generated a GUID with a cryptographically secure random number generator.
        /// </summary>
        /// <returns>Secure GUID.</returns>
        public static Guid GenerateSecureGuid()
        {
            var secureGuid = new byte[16];

            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(secureGuid);
            }

            return new Guid(secureGuid);
        }

        public static string GenerateDecoratedUsername(string id)
        {
            string decoration = "prison-";
            var windowsUsername = decoration + id;

            // Max local windows username is 20 chars.
            windowsUsername = windowsUsername.Substring(0, Math.Min(20, windowsUsername.Length));
            return windowsUsername;
        }

        public static string CreateDecoratedUser(string id, string password)
        {
            var windowsUsername = GenerateDecoratedUsername(id);

            WindowsUsersAndGroups.CreateUser(windowsUsername, password, GenrateUserDescription(id));

            return windowsUsername;
        }

        public static string GenrateUserDescription(string id)
        {
            return "Uhuru Process Prison " + id;
        }

        public static string GetIdFromUserDescription(string description)
        {
            if (!description.Contains("Uhuru Process Prison "))
            {
                throw new ArgumentException("description not valid", "description");
            }
            return description.Substring("Uhuru Process Prison ".Length);
        }

        private static string JobObjectNamespace()
        {
            return "";
        }

    }
}
