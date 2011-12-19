// -----------------------------------------------------------------------
// <copyright file="NativeMethods.cs" company="Uhuru Software, Inc.">
// Copyright (c) 2011 Uhuru Software, Inc., All Rights Reserved
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
    internal static class NativeMethods
    {
        /// <summary>
        /// This tells OpenProcess that we want all possible access rights for a process object.
        /// </summary>
        public const int ProcessAllAccess = 0x1F0FFF;

        /// <summary>
        /// This is an IntPtr that looks like an error response. We use it for comparison.
        /// </summary>
        public static readonly IntPtr ProcessHandleError = new IntPtr(-1);

        /// <summary>
        /// This is an IntPtr that looks like an error response. We use it for comparison.
        /// </summary>
        public static readonly IntPtr ProcessHandleZero = new IntPtr(0);

        /// <summary>
        /// Closes handles.
        /// </summary>
        /// <param name="handle">The handle to close.</param>
        /// <returns>A boolean value indicating whether the operation was successful.</returns>
        [DllImport("KERNEL32.DLL")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool CloseHandle(IntPtr handle);

        /// <summary>
        /// gets the process handle
        /// </summary>
        /// <param name="desiredAccess">the access level we want to obtain</param>
        /// <param name="inheritHandle">if this is true, child processes will inherit this handle</param>
        /// <param name="processId">the process ID</param>
        /// <returns>a process handle</returns>
        [DllImport("kernel32.dll")]
        public static extern IntPtr OpenProcess(uint desiredAccess, [MarshalAs(UnmanagedType.Bool)] bool inheritHandle, uint processId);

        /// <summary>
        /// gets the process creation, exit, kernel and user time 
        /// </summary>
        /// <param name="processHandle">the handle of the process</param>
        /// <param name="creationTime">the creation time</param>
        /// <param name="exitTime">the exit time</param>
        /// <param name="kernelTime">the kernel time</param>
        /// <param name="userTime">the user time</param>
        /// <returns>True if the operation was successful</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1021:AvoidOutParameters", Justification = "No other way to do this."), 
        DllImport("kernel32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool GetProcessTimes(IntPtr processHandle, out ComType.FILETIME creationTime, out ComType.FILETIME exitTime, out ComType.FILETIME kernelTime, out ComType.FILETIME userTime);
    }
}
