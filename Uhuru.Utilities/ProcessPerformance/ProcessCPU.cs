using System;
using System.Runtime.InteropServices;
using ComType = System.Runtime.InteropServices.ComTypes;

namespace Uhuru.Utilities.ProcessPerformance
{
    static class NativeMethods
    {
        // closes handles
        [DllImport("KERNEL32.DLL")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool CloseHandle(IntPtr Handle);

        // gets the process handle
        [DllImport("kernel32.dll")]
        public static extern IntPtr OpenProcess(uint DesiredAccess, [MarshalAs(UnmanagedType.Bool)] bool InheritHandle, uint ProcessId);

        // gets the process creation, exit, kernel and user time 
        [DllImport("kernel32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool GetProcessTimes(IntPtr ProcessHandle, out ComType.FILETIME CreationTime, out ComType.FILETIME ExitTime, out ComType.FILETIME KernelTime, out ComType.FILETIME UserTime);

        // some consts will need later
        public const int PROCESS_ENTRY_32_SIZE = 296;
        public const uint TH32CS_SNAPPROCESS = 0x00000002;
        public const uint PROCESS_ALL_ACCESS = 0x1F0FFF;

        public static readonly IntPtr PROCESS_HANDLE_ERROR = new IntPtr(-1);
    }
}
