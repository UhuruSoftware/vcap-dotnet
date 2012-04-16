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
        public UserImpersonator(string userName, string domainName, string password)
        {
            this.ImpersonateValidUser(userName, domainName, password);
        }

        /// <summary>
        /// Disposes the current object.
        /// </summary>
        public void Dispose()
        {
            this.UndoImpersonation();
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1060:MovePInvokesToNativeMethodsClass", Justification = "Keeping everything in one file for clarity."), 
        DllImport("advapi32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        private static extern int LogonUser(string userName, string domain, string password, int logonType, int logonProvider, ref IntPtr token);

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1060:MovePInvokesToNativeMethodsClass", Justification = "Keeping everything in one file for clarity."), 
        DllImport("advapi32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern int DuplicateToken(IntPtr token, int impersonationLevel, ref IntPtr newToken);

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1060:MovePInvokesToNativeMethodsClass", Justification = "Keeping everything in one file for clarity."), 
        DllImport("advapi32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool RevertToSelf();

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1060:MovePInvokesToNativeMethodsClass", Justification = "Keeping everything in one file for clarity."), 
        DllImport("kernel32.dll", CharSet = CharSet.Auto)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool CloseHandle(IntPtr handle);

        /// <summary>
        /// Does the actual impersonation.
        /// </summary>
        /// <param name="userName">The name of the user to act as.</param>
        /// <param name="domain">The domain name of the user to act as.</param>
        /// <param name="password">The password of the user to act as.</param>
        private void ImpersonateValidUser(string userName, string domain, string password)
        {
            WindowsIdentity tempWindowsIdentity = null;
            IntPtr token = IntPtr.Zero;
            IntPtr tokenDuplicate = IntPtr.Zero;

            try
            {
                if (!RevertToSelf())
                {
                    throw new Win32Exception(Marshal.GetLastWin32Error());
                }
                else
                {
                    if (LogonUser(userName, domain, password, Logon32LogonInteractive, Logon32ProviderDefault, ref token) == 0)
                    {
                        throw new Win32Exception(Marshal.GetLastWin32Error());
                    }
                    else
                    {
                        if (DuplicateToken(token, 2, ref tokenDuplicate) == 0)
                        {
                            throw new Win32Exception(Marshal.GetLastWin32Error());
                        }
                        else
                        {
                            using (tempWindowsIdentity = new WindowsIdentity(tokenDuplicate))
                            {
                                this.impersonationContext = tempWindowsIdentity.Impersonate();
                            }
                        }
                    }
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
    }
}