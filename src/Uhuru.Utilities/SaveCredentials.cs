// -----------------------------------------------------------------------
// <copyright file="SaveCredentials.cs" company="Uhuru Software, Inc.">
// Copyright (c) 2011 Uhuru Software, Inc., All Rights Reserved
// </copyright>
// -----------------------------------------------------------------------

namespace Uhuru.Utilities
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Runtime.InteropServices;
    using System.Text;

    /// <summary>
    /// Utility class for adding credentials in credential manager.
    /// </summary>
    public static class SaveCredentials
    {
        /// <summary>
        /// Credential type.
        /// </summary>
        private enum CRED_TYPE : uint
        {
            /// <summary>
            /// Generic credential.
            /// </summary>
            CRED_TYPE_GENERIC = 1,

            /// <summary>
            /// Domain password.
            /// </summary>
            CRED_TYPE_DOMAIN_PASSWORD = 2,

            /// <summary>
            /// Domain certificate.
            /// </summary>
            CRED_TYPE_DOMAIN_CERTIFICATE = 3,

            /// <summary>
            /// Domain visible password.
            /// </summary>
            CRED_TYPE_DOMAIN_VISIBLE_PASSWORD = 4
        }

        /// <summary>
        /// Credential persistence.
        /// </summary>
        private enum CRED_PERSIST : uint
        {
            /// <summary>
            /// Session persistence.
            /// </summary>
            CRED_PERSIST_SESSION = 1,

            /// <summary>
            /// Local machine persistence.
            /// </summary>
            CRED_PERSIST_LOCAL_MACHINE = 2,

            /// <summary>
            /// Enterprise persistence.
            /// </summary>
            CRED_PERSIST_ENTERPRISE = 3
        }

        /// <summary>
        /// Adds the domain user credential.
        /// </summary>
        /// <param name="target">The target.</param>
        /// <param name="userName">Name of the user.</param>
        /// <param name="password">The password.</param>
        public static void AddDomainUserCredential(string target, string userName, string password)
        {
            Credential userCredential = new Credential();

            try
            {
                userCredential.TargetName = target;
                userCredential.Type = (uint)CRED_TYPE.CRED_TYPE_DOMAIN_PASSWORD;
                userCredential.UserName = userName;
                userCredential.AttributeCount = 0;
                userCredential.Persist = (uint)CRED_PERSIST.CRED_PERSIST_LOCAL_MACHINE;

                byte[] bpassword = Encoding.Unicode.GetBytes(password);
                userCredential.CredentialBlobSize = (uint)bpassword.Length;
                userCredential.CredentialBlob = Marshal.StringToCoTaskMemUni(password);

                if (!CredWrite(ref userCredential, (uint)0))
                {
                    throw new System.ComponentModel.Win32Exception(Marshal.GetLastWin32Error());
                }
            }
            finally
            {
                if (userCredential.CredentialBlob != null)
                {
                    Marshal.FreeCoTaskMem(userCredential.CredentialBlob);
                }
            }
        }

        /// <summary>
        /// Credentials the write.
        /// </summary>
        /// <param name="userCredential">The user credential.</param>
        /// <param name="flags">The flags.</param>
        /// <returns>True if the method executed successfully.</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1060:MovePInvokesToNativeMethodsClass", Justification = "Suppressed for simplicity."),
        DllImport("Advapi32.dll", SetLastError = true, EntryPoint = "CredWrite", CharSet = CharSet.Auto)]
        [return: System.Runtime.InteropServices.MarshalAsAttribute(System.Runtime.InteropServices.UnmanagedType.Bool)]
        private static extern bool CredWrite(ref Credential userCredential, uint flags);

        /// <summary>
        /// Credential structure.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1049:TypesThatOwnNativeResourcesShouldBeDisposable", Justification = "Unmanaged resources are properly disposed."),
        StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        private struct Credential
        {
            /// <summary>
            /// Flags field.
            /// </summary>
            public uint Flags;

            /// <summary>
            /// Type field.
            /// </summary>
            public uint Type;

            /// <summary>
            /// Target name.
            /// </summary>
            public string TargetName;

            /// <summary>
            /// Comment field.
            /// </summary>
            public string Comment;

            /// <summary>
            /// Last written.
            /// </summary>
            public System.Runtime.InteropServices.ComTypes.FILETIME LastWritten;

            /// <summary>
            /// Size of password.
            /// </summary>
            public uint CredentialBlobSize;

            /// <summary>
            /// The password.
            /// </summary>
            [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2006:UseSafeHandleToEncapsulateNativeResources", Justification = "The usage is straight-forward.")]
            public IntPtr CredentialBlob;

            /// <summary>
            /// Persistence type.
            /// </summary>
            public uint Persist;

            /// <summary>
            /// Attributes count.
            /// </summary>
            public uint AttributeCount;

            /// <summary>
            /// Credential attributes.
            /// </summary>
            public IntPtr CredAttribute;

            /// <summary>
            /// Target alias.
            /// </summary>
            public string TargetAlias;

            /// <summary>
            /// User name filed.
            /// </summary>
            public string UserName;
        }
    }
}
