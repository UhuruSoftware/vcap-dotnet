// -----------------------------------------------------------------------
// <copyright file="ProcessCPU.cs" company="Uhuru Software">
// </copyright>
// -----------------------------------------------------------------------

namespace Uhuru.Utilities.ProcessPerformance
{
    using System;
    using System.Runtime.InteropServices;
    using ComType = System.Runtime.InteropServices.ComTypes;
    
    static class NativeMethods
    {
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
        /// <param name="DesiredAccess"></param>
        /// <param name="InheritHandle"></param>
        /// <param name="ProcessId"></param>
        /// <returns></returns>
        [DllImport("kernel32.dll")]
        public static extern IntPtr OpenProcess(uint DesiredAccess, [MarshalAs(UnmanagedType.Bool)] bool InheritHandle, uint ProcessId);

        /// <summary>
        /// gets the process creation, exit, kernel and user time 
        /// </summary>
        /// <param name="ProcessHandle"></param>
        /// <param name="CreationTime"></param>
        /// <param name="ExitTime"></param>
        /// <param name="KernelTime"></param>
        /// <param name="UserTime"></param>
        /// <returns></returns>
        [DllImport("kernel32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool GetProcessTimes(IntPtr ProcessHandle, out ComType.FILETIME CreationTime, out ComType.FILETIME ExitTime, out ComType.FILETIME KernelTime, out ComType.FILETIME UserTime);

        // some consts we'll need later
        public const int PROCESS_ENTRY_32_SIZE = 296;
        public const uint TH32CS_SNAPPROCESS = 0x00000002;
        public const uint PROCESS_ALL_ACCESS = 0x1F0FFF;

        public static readonly IntPtr PROCESS_HANDLE_ERROR = new IntPtr(-1);
    }
}
