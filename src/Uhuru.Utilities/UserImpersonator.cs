// -----------------------------------------------------------------------
// <copyright file="UserImpersonator.cs" company="Uhuru Software, Inc.">
// Copyright (c) 2011 Uhuru Software, Inc., All Rights Reserved
// </copyright>
// Code from http://www.codeproject.com/Articles/10090/A-small-C-Class-for-impersonating-a-User
// -----------------------------------------------------------------------

namespace Uhuru.Utilities
{
    using System;
    using System.ComponentModel;
    using System.Runtime.InteropServices;
    using System.Security.Principal;

    /// <summary>
    /// Impersonation of a user. Allows to execute code under another
    /// user context.
    /// Please note that the account that instantiates the Impersonator class
    /// needs to have the 'Act as part of operating system' privilege set.
    /// </summary>
    /// <remarks>
    /// This class is based on the information in the Microsoft knowledge base
    /// article http://support.microsoft.com/default.aspx?scid=kb;en-us;Q306158
    /// Encapsulate an instance into a using-directive like e.g.:
    /// </remarks>
    public sealed class UserImpersonator : IDisposable
    {
        /// <summary>
        /// Interactive logon.
        /// </summary>
        private const int Logon32LogonInteractive = 2;

        /// <summary>
        /// Default logon provider.
        /// </summary>
        private const int Logon32ProviderDefault = 0;

        /// <summary>
        /// Impersonation context.
        /// </summary>
        private WindowsImpersonationContext impersonationContext = null;

        /// <summary>
        /// Initializes a new instance of the UserImpersonator class.
        /// Starts the impersonation with the given credentials.
        /// Please note that the account that instantiates the Impersonator class
        /// needs to have the 'Act as part of operating system' privilege set.
        /// </summary>
        /// <param name="userName">The name of the user to act as.</param>
        /// <param name="domainName">The domain name of the user to act as.</param>
        /// <param name="password">The password of the user to act as.</param>
        /// <param name="loadUserProfile">if set to <c>true</c> [load user profile].</param>
        public UserImpersonator(string userName, string domainName, string password, bool loadUserProfile)
        {
            this.ImpersonateValidUser(userName, domainName, password, loadUserProfile);
        }

        /// <summary>
        /// Disposes the current object.
        /// </summary>
        public void Dispose()
        {
            this.UndoImpersonation();
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1060:MovePInvokesToNativeMethodsClass", Justification = "Keeping everything in one file for clarity.")]
        [DllImport("advapi32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        private static extern int LogonUser(string userName, string domain, string password, int logonType, int logonProvider, ref IntPtr token);

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1060:MovePInvokesToNativeMethodsClass", Justification = "Keeping everything in one file for clarity.")]
        [DllImport("advapi32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern int DuplicateToken(IntPtr token, int impersonationLevel, ref IntPtr newToken);

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1060:MovePInvokesToNativeMethodsClass", Justification = "Keeping everything in one file for clarity.")]
        [DllImport("advapi32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool RevertToSelf();

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1060:MovePInvokesToNativeMethodsClass", Justification = "Keeping everything in one file for clarity.")]
        [DllImport("kernel32.dll", CharSet = CharSet.Auto)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool CloseHandle(IntPtr handle);

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1060:MovePInvokesToNativeMethodsClass", Justification = "Keeping everything in one file for clarity.")]
        [System.Runtime.InteropServices.DllImportAttribute("userenv.dll", EntryPoint = "LoadUserProfile", SetLastError = true, CharSet = CharSet.Auto)]
        [return: System.Runtime.InteropServices.MarshalAsAttribute(System.Runtime.InteropServices.UnmanagedType.Bool)]
        private static extern bool LoadUserProfile([System.Runtime.InteropServices.InAttribute()] System.IntPtr token, ref ProfileInfo profileInfo);

        /// <summary>
        /// Does the actual impersonation.
        /// </summary>
        /// <param name="userName">The name of the user to act as.</param>
        /// <param name="domain">The domain name of the user to act as.</param>
        /// <param name="password">The password of the user to act as.</param>
        /// <param name="loadUserProfile">if set to <c>true</c> [load user profile].</param>
        private void ImpersonateValidUser(string userName, string domain, string password, bool loadUserProfile)
        {
            WindowsIdentity tempWindowsIdentity = null;
            IntPtr token = IntPtr.Zero;
            IntPtr tokenDuplicate = IntPtr.Zero;

            ProfileInfo profileInfo = new ProfileInfo();

            profileInfo.Size = Marshal.SizeOf(profileInfo.GetType());
            profileInfo.Flags = 0x1;
            profileInfo.UserName = userName;
            profileInfo.DefaultPath = null;
            profileInfo.PolicyPath = null;
            profileInfo.ProfilePath = null;
            profileInfo.ServerName = domain;

            try
            {
                if (!RevertToSelf())
                {
                    throw new Win32Exception(Marshal.GetLastWin32Error());
                }

                if (LogonUser(userName, domain, password, Logon32LogonInteractive, Logon32ProviderDefault, ref token) == 0)
                {
                    throw new Win32Exception(Marshal.GetLastWin32Error());
                }

                if (DuplicateToken(token, 2, ref tokenDuplicate) == 0)
                {
                    throw new Win32Exception(Marshal.GetLastWin32Error());
                }

                if (loadUserProfile && !LoadUserProfile(tokenDuplicate, ref profileInfo))
                {
                    throw new Win32Exception(Marshal.GetLastWin32Error());
                }

                using (tempWindowsIdentity = new WindowsIdentity(tokenDuplicate))
                {
                    this.impersonationContext = tempWindowsIdentity.Impersonate();
                }
            }
            finally
            {
                if (token != IntPtr.Zero)
                {
                    CloseHandle(token);
                }

                if (tokenDuplicate != IntPtr.Zero)
                {
                    CloseHandle(tokenDuplicate);
                }
            }
        }

        /// <summary>
        /// Reverts the impersonation.
        /// </summary>
        private void UndoImpersonation()
        {
            if (this.impersonationContext != null)
            {
                this.impersonationContext.Undo();
            }
        }

        /// <summary>
        /// Profile Info structure.
        /// </summary>
        [System.Runtime.InteropServices.StructLayoutAttribute(System.Runtime.InteropServices.LayoutKind.Sequential)]
        private struct ProfileInfo
        {
            /// <summary>
            /// Structure filed.
            /// </summary>
            public int Size;

            /// <summary>
            /// Structure filed.
            /// </summary>
            public int Flags;

            /// <summary>
            /// Structure filed.
            /// </summary>
            [System.Runtime.InteropServices.MarshalAsAttribute(System.Runtime.InteropServices.UnmanagedType.LPTStr)]
            public string UserName;

            /// <summary>
            /// Structure filed.
            /// </summary>
            [System.Runtime.InteropServices.MarshalAsAttribute(System.Runtime.InteropServices.UnmanagedType.LPTStr)]
            public string ProfilePath;

            /// <summary>
            /// Structure filed.
            /// </summary>
            [System.Runtime.InteropServices.MarshalAsAttribute(System.Runtime.InteropServices.UnmanagedType.LPTStr)]
            public string DefaultPath;

            /// <summary>
            /// Structure filed.
            /// </summary>
            [System.Runtime.InteropServices.MarshalAsAttribute(System.Runtime.InteropServices.UnmanagedType.LPTStr)]
            public string ServerName;

            /// <summary>
            /// Policy path filed.
            /// </summary>
            [System.Runtime.InteropServices.MarshalAsAttribute(System.Runtime.InteropServices.UnmanagedType.LPTStr)]
            public string PolicyPath;

            /// <summary>
            /// Profile field.
            /// </summary>
            public System.IntPtr Profile;
        }
    }
}