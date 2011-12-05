// -----------------------------------------------------------------------
// <copyright file="ProcessCPU.cs" company="Uhuru Software">
// </copyright>
// -----------------------------------------------------------------------

namespace Uhuru.Utilities.ProcessPerformance
{
    using System;
    using System.Runtime.InteropServices;
    using ComType = System.Runtime.InteropServices.ComTypes;
    
    /// <summary>
    /// exposes a few WinAPI methods for working with processes
    /// </summary>
    public static class NativeMethods
    {
        // some consts we'll need later
        public const int ProcessEntry32Size = 296;
        public const uint TH32CSSnapProcess = 0x00000002;
        public const uint ProcessAllAccess = 0x1F0FFF;

        public static readonly IntPtr ProcessHandleError = new IntPtr(-1);
        
        /// <summary>
        /// closes handles
        /// </summary>
        /// <param name="Handle"></param>
        /// <returns></returns>
        [DllImport("KERNEL32.DLL")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool CloseHandle(IntPtr Handle);

        /// <summary>
        /// gets the process handle
        /// </summary>
        /// <param name="DesiredAccess">the access level we want to obtain</param>
        /// <param name="InheritHandle"></param>
        /// <param name="ProcessId">the process ID</param>
        /// <returns>a process handle</returns>
        [DllImport("kernel32.dll")]
        public static extern IntPtr OpenProcess(uint DesiredAccess, [MarshalAs(UnmanagedType.Bool)] bool InheritHandle, uint ProcessId);

        /// <summary>
        /// gets the process creation, exit, kernel and user time 
        /// </summary>
        /// <param name="ProcessHandle">the handle of the process</param>
        /// <param name="CreationTime">the creation time</param>
        /// <param name="ExitTime">the exit time</param>
        /// <param name="KernelTime">the kernel time</param>
        /// <param name="UserTime">the user time</param>
        /// <returns></returns>
        [DllImport("kernel32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool GetProcessTimes(IntPtr ProcessHandle, out ComType.FILETIME CreationTime, out ComType.FILETIME ExitTime, out ComType.FILETIME KernelTime, out ComType.FILETIME UserTime);
    }
}
