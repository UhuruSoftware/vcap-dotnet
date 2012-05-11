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
            ExecuteCommand(string.Format(CultureInfo.InvariantCulture, @"net use ""{0}"" ""{1}"" /USER:""{2}""", remotePath, remotePassword, remoteUser));
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
            ExecuteCommand(string.Format(CultureInfo.InvariantCulture, @"net use ""{0}"" /delete", remotePath));
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
        /// Persists a resource on a mounted share, and then links it.
        /// This method will make sure the folder and file structure remains the same on the local file system, while also persisting data on a share.
        /// </summary>
        /// <param name="instancePath">The directory considered to be the "root" of the resources that have to be persisted.</param>
        /// <param name="persistentItem">The directory or file that has to be persisted.</param>
        /// <param name="mountPath">The mounted directory that points to a share.</param>
        public static void Link(string instancePath, string persistentItem, string mountPath)
        {
            if (string.IsNullOrEmpty(instancePath))
            {
                throw new ArgumentNullException("instancePath");
            }

            if (string.IsNullOrEmpty(persistentItem))
            {
                throw new ArgumentNullException("instancePath");
            }

            if (string.IsNullOrEmpty(mountPath))
            {
                throw new ArgumentNullException("instancePath");
            }

            string mountItem = Path.Combine(mountPath, persistentItem);
            string instanceItem = Path.Combine(instancePath, persistentItem);

            if (Directory.Exists(mountItem) || Directory.Exists(instanceItem))
            {
                Directory.CreateDirectory(mountItem);
                Directory.CreateDirectory(instanceItem);

                CopyFolderRecursively(instanceItem, mountItem);

                try
                {
                    Directory.Delete(instanceItem, true);
                }
                catch (DirectoryNotFoundException)
                {
                }

                // ExecuteCommand("mklink" + " /d " + instanceItem + " " + mountItem);
                CreateDirectorySymbolicLink(instanceItem, mountItem);
            }

            if (File.Exists(mountItem) || File.Exists(instanceItem))
            {
                Directory.CreateDirectory(new DirectoryInfo(mountItem).Parent.FullName);
                Directory.CreateDirectory(new DirectoryInfo(instanceItem).Parent.FullName);

                try
                {
                    File.Copy(instanceItem, mountItem);
                }
                catch (IOException)
                {
                }

                try
                {
                    File.Delete(instanceItem);
                }
                catch (DirectoryNotFoundException)
                {
                }

                // ExecuteCommand("mklink" + " " + instanceItem + " " + mountItem);
                CreateFileSymbolicLink(instanceItem, mountItem);
            }
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

        /// <summary>
        /// Copies a directory recursively, without overwriting.
        /// </summary>
        /// <param name="source">Source folder to copy.</param>
        /// <param name="destination">Destination folder.</param>
        private static void CopyFolderRecursively(string source, string destination)
        {
            if (!Directory.Exists(destination))
            {
                Directory.CreateDirectory(destination);
            }

            string[] files = Directory.GetFiles(source);

            foreach (string file in files)
            {
                string name = Path.GetFileName(file);
                string dest = Path.Combine(destination, name);

                try
                {
                    File.Copy(file, dest, false);
                }
                catch (IOException)
                {
                }
            }

            string[] folders = Directory.GetDirectories(source);

            foreach (string folder in folders)
            {
                string name = Path.GetFileName(folder);
                string dest = Path.Combine(destination, name);
                CopyFolderRecursively(folder, dest);
            }
        }
    }
}
