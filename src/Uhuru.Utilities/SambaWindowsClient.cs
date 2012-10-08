// -----------------------------------------------------------------------
// <copyright file="SambaWindowsClient.cs" company="Uhuru Software, Inc.">
// Copyright (c) 2011 Uhuru Software, Inc., All Rights Reserved
// </copyright>
// -----------------------------------------------------------------------

namespace Uhuru.Utilities
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Runtime.InteropServices;
    using System.Text;

    /// <summary>
    /// This class contains helper functions for mounting a share as a local directory.
    /// </summary>
    public static class SambaWindowsClient
    {
        /// <summary>
        /// Creates the file symbolic link.
        /// </summary>
        /// <param name="sourceFileName">Name of the source file.</param>
        /// <param name="targetFileName">Name of the target file.</param>
        public static void CreateFileSymbolicLink(string sourceFileName, string targetFileName)
        {
            if (CreateSymbolicLink(sourceFileName, targetFileName, 0) == 0)
            {
                throw new Win32Exception(Marshal.GetLastWin32Error());
            }
        }

        /// <summary>
        /// Creates the directory symbolic link.
        /// </summary>
        /// <param name="sourceDirectoryName">Name of the source directory.</param>
        /// <param name="targetDirectoryName">Name of the target directory.</param>
        public static void CreateDirectorySymbolicLink(string sourceDirectoryName, string targetDirectoryName)
        {
            if (CreateSymbolicLink(sourceDirectoryName, targetDirectoryName, 1) == 0)
            {
                throw new Win32Exception(Marshal.GetLastWin32Error());
            }
        }

        /// <summary>
        /// Mounts a remote share as a local directory.
        /// </summary>
        /// <param name="remotePath">The remote path.</param>
        /// <param name="remoteUser">A username used for authentication to the share.</param>
        /// <param name="remotePassword">A password used for authentication to the share.</param>
        public static void Mount(string remotePath, string remoteUser, string remotePassword)
        {
            ExecuteCommand(string.Format(CultureInfo.InvariantCulture, @"net use ""{0}"" ""{1}"" /USER:""{2}"" /yes", remotePath, remotePassword, remoteUser));
        }

        /// <summary>
        /// Make a link.
        /// </summary>
        /// <param name="remotePath">The remote path.</param>
        /// <param name="localPath">The local path that will be the mount point.</param>
        public static void LinkDirectory(string remotePath, string localPath)
        {
            // mklink creates the directory if doesn't exist
            try
            {
                Directory.Delete(localPath, true);
            }
            catch (DirectoryNotFoundException)
            {
            }

            // ExecuteCommand(string.Format(CultureInfo.InvariantCulture, @"mklink /d ""{0}"" ""{1}""", localPath, remotePath)) == 0;
            CreateDirectorySymbolicLink(localPath, remotePath);
        }

        /// <summary>
        /// Un-mounts a local path.
        /// </summary>
        /// <param name="remotePath">The remote path.</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Unmount", Justification = "Word is added to dictionary, but the warning is still shown.")]
        public static void Unmount(string remotePath)
        {
            ExecuteCommand(string.Format(CultureInfo.InvariantCulture, @"net use ""{0}"" /delete /yes", remotePath));
            //// ExecuteProcess("rmdir", string.Format(CultureInfo.InvariantCulture, @"/q ""{0}""", localPath));
        }

        /// <summary>
        /// Un-mounts a local path.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Unmount", Justification = "Word is added to dictionary, but the warning is still shown.")]
        public static void UnmountAll()
        {
            ExecuteCommand(@"net use  * /delete /yes");
        }

        /// <summary>
        /// Creates the symbolic link.
        /// </summary>
        /// <param name="symlinkFileName">Name of the symlink file.</param>
        /// <param name="targetFileName">Name of the target file.</param>
        /// <param name="flags">The flags. 0 for files and 1 for directories.</param>
        /// <returns>
        /// Returns 1 if successful.
        /// </returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1060:MovePInvokesToNativeMethodsClass", Justification = "Improves clarity."),
        DllImport("kernel32.dll", EntryPoint = "CreateSymbolicLinkW", CharSet = CharSet.Unicode, SetLastError = true)]
        private static extern int CreateSymbolicLink(string symlinkFileName, string targetFileName, int flags);

        /// <summary>
        /// Runs a process and waits for it to return.
        /// </summary>
        /// <param name="command">The command.</param>
        /// <returns>
        /// Process return code
        /// </returns>
        private static int ExecuteCommand(string command)
        {
            ProcessStartInfo pi = new ProcessStartInfo("cmd", "/c " + command);
            pi.CreateNoWindow = true;
            pi.UseShellExecute = false;
            pi.LoadUserProfile = false;
            pi.WorkingDirectory = "\\";

            using (Process process = Process.Start(pi))
            {
                process.WaitForExit();
                return process.ExitCode;
            }
        }
    }
}
