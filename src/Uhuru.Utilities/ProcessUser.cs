// -----------------------------------------------------------------------
// <copyright file="ProcessUser.cs" company="Uhuru Software, Inc.">
// Copyright (c) 2011 Uhuru Software, Inc., All Rights Reserved
// </copyright>
// -----------------------------------------------------------------------

namespace Uhuru.Utilities
{
    using System;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.Runtime.InteropServices;
    using System.Security.Principal;

    /// <summary>
    /// Windows User information about a Process.
    /// </summary>
    public static class ProcessUser
    {
        /// <summary>
        /// Gets the WindowsIdentity associated with the process.
        /// </summary>
        /// <param name="process">The process.</param>
        /// <returns>Windows Identity of the Process</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "Appropiate."), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "Appropiate.")]
        public static WindowsIdentity GetProcessWindowsIdentity(Process process)
        {
            IntPtr tokenHandle = IntPtr.Zero;
            WindowsIdentity wi = null;
            try
            {
                tokenHandle = GetProcessTokenHandle(process);
                wi = new WindowsIdentity(tokenHandle);
            }
            catch
            {
                if (wi != null)
                {
                    wi.Dispose();
                }

                return null;
            }
            finally
            {
                if (tokenHandle != IntPtr.Zero)
                {
                    NativeMethods.CloseHandle(tokenHandle);
                }
            }

            return wi;
        }

        /// <summary>
        /// Gets the Windows user name associated with the process.
        /// </summary>
        /// <param name="process">The process.</param>
        /// <returns>Windows User of the Process</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "Is is desirable to catch all exceptions.")]
        public static string GetProcessUser(Process process)
        {
            try
            {
                return GetProcessWindowsIdentity(process).Name.Split(new char[] { '\\' }, 2)[1];
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Gets the Access Token Handle associated with the process.
        /// </summary>
        /// <param name="process">The process.</param>
        /// <returns>Access Token Handle of the Process</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2122:DoNotIndirectlyExposeMethodsWithLinkDemands", Justification = "Appropriate.")]
        private static IntPtr GetProcessTokenHandle(Process process)
        {
            IntPtr tokenHandle;
            NativeMethods.OpenProcessToken(process.Handle, NativeMethods.TOKEN_QUERY, out tokenHandle);
            return tokenHandle;
        }

        /// <summary>
        /// The Windows API functions.
        /// </summary>
        [SuppressMessage("StyleCop.CSharp.NamingRules", "SA1600:ElementsMustBeDocumented", Justification = "WinAPI elements.")]
        [SuppressMessage("StyleCop.CSharp.NamingRules", "SA1305:FieldNamesMustNotUseHungarianNotation", Justification = "Preserve WinAPI naming.")]
        internal static class NativeMethods
        {
            /// <summary>
            /// Windows API constant.
            /// </summary>
            public const uint STANDARD_RIGHTS_REQUIRED = 0x000F0000;

            /// <summary>
            /// Windows API constant.
            /// </summary>
            public const uint STANDARD_RIGHTS_READ = 0x00020000;

            /// <summary>
            /// Windows API constant.
            /// </summary>
            public const uint TOKEN_ASSIGN_PRIMARY = 0x0001;

            /// <summary>
            /// Windows API constant.
            /// </summary>
            public const uint TOKEN_DUPLICATE = 0x0002;

            /// <summary>
            /// Windows API constant.
            /// </summary>
            public const uint TOKEN_IMPERSONATE = 0x0004;

            /// <summary>
            /// Windows API constant.
            /// </summary>
            public const uint TOKEN_QUERY = 0x0008;

            /// <summary>
            /// Windows API constant.
            /// </summary>
            public const uint TOKEN_QUERY_SOURCE = 0x0010;

            /// <summary>
            /// Windows API constant.
            /// </summary>
            public const uint TOKEN_ADJUST_PRIVILEGES = 0x0020;

            /// <summary>
            /// Windows API constant.
            /// </summary>
            public const uint TOKEN_ADJUST_GROUPS = 0x0040;

            /// <summary>
            /// Windows API constant.
            /// </summary>
            public const uint TOKEN_ADJUST_DEFAULT = 0x0080;

            /// <summary>
            /// Windows API constant.
            /// </summary>
            public const uint TOKEN_ADJUST_SESSIONID = 0x0100;

            /// <summary>
            /// Windows API constant.
            /// </summary>
            public const uint TOKEN_READ = STANDARD_RIGHTS_READ | TOKEN_QUERY;

            /// <summary>
            /// Windows API constant.
            /// </summary>
            public const uint TOKEN_ALL_ACCESS =
                STANDARD_RIGHTS_REQUIRED | TOKEN_ASSIGN_PRIMARY |
                TOKEN_DUPLICATE | TOKEN_IMPERSONATE | TOKEN_QUERY | TOKEN_QUERY_SOURCE |
                TOKEN_ADJUST_PRIVILEGES | TOKEN_ADJUST_GROUPS | TOKEN_ADJUST_DEFAULT |
                TOKEN_ADJUST_SESSIONID;

            [DllImport("advapi32", SetLastError = true)]
            [return: MarshalAs(UnmanagedType.Bool)]
            public static extern bool OpenProcessToken(IntPtr ProcessHandle, uint DesiredAccess, out IntPtr TokenHandle);

            [DllImport("kernel32.dll", SetLastError = true)]
            [return: MarshalAs(UnmanagedType.Bool)]
            public static extern bool CloseHandle(IntPtr handle);
        }
    }
}
