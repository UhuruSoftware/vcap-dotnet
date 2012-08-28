// -----------------------------------------------------------------------
// <copyright file="VHDUtilities.cs" company="Uhuru Software, Inc.">
// Copyright (c) 2011 Uhuru Software, Inc., All Rights Reserved
// </copyright>
// -----------------------------------------------------------------------

namespace Uhuru.CloudFoundry.FileService
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Text;
    using Uhuru.Utilities;

    /// <summary>
    /// Manage Windows VHDs.
    /// </summary>
    public static class VHDUtilities
    {
        /// <summary>
        /// Executes the diskpart script.
        /// </summary>
        /// <param name="script">The script.</param>
        /// <returns>Output script.</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2201:DoNotRaiseReservedExceptionTypes", Justification = "Code more readeble."),
        System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1303:Do not pass literals as localized parameters", MessageId = "Uhuru.Utilities.Logger.Info(System.String)", Justification = "Code more readeble."),
        System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1303:Do not pass literals as localized parameters", MessageId = "Uhuru.Utilities.Logger.Error(System.String)", Justification = "Code more readeble.")]
        public static string ExecuteDiskPartScript(string script)
        {
            string tempScriptFile = Path.GetTempFileName();
            File.WriteAllText(tempScriptFile, script);
            ProcessStartInfo psi = new ProcessStartInfo("diskpart", "/s " + "\"" + tempScriptFile + "\"");
            psi.UseShellExecute = false;
            psi.CreateNoWindow = true;
            psi.RedirectStandardError = true;
            psi.RedirectStandardOutput = true;

            var ps = Process.Start(psi);

            ps.WaitForExit();

            string output = ps.StandardError.ReadToEnd() + ps.StandardOutput.ReadToEnd();

            string message = string.Format(CultureInfo.InvariantCulture, "DISKPART exit code: {0}\n Execution time: {3}\n Output:\n{1}\n Script:\n{2}", ps.ExitCode, output, script, ps.ExitTime - ps.StartTime);

            if (ps.ExitCode != 0)
            {
                // Logger.Error(message);
                throw new Exception(message);
            }

            File.Delete(tempScriptFile);

            // Logger.Info(message);
            return output;
        }

        /// <summary>
        /// Creates a VHD on the specified path.
        /// </summary>
        /// <param name="path">The file path.</param>
        /// <param name="diskSizeMiB">The size MiB.</param>
        /// <param name="fixedSize">if set to <c>true</c> [fixed size].</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "Mi", Justification = "asdf")]
        public static void CreateVHD(string path, long diskSizeMiB, bool fixedSize)
        {
            string script =
                @"
                    create vdisk file=""{0}""  maximum={1} type={2}
                    attach vdisk
                    create partition primary 
                    format quick
                    detach vdisk
                ";

            script = string.Format(
                CultureInfo.InvariantCulture,
                script,
                path,
                diskSizeMiB,
                fixedSize ? "fixed" : "expandable");

            ExecuteDiskPartScript(script);
        }

        /// <summary>
        /// Mounts the VHD to the specified path.
        /// </summary>
        /// <param name="path">The file path to the VHD.</param>
        /// <param name="mountPath">The target mount path.</param>
        public static void MountVHD(string path, string mountPath)
        {
            Directory.CreateDirectory(mountPath);

            string script =
                @"
                    select vdisk file=""{0}""
                    attach vdisk
                ";

            script = string.Format(
                CultureInfo.InvariantCulture,
                script,
                path);

            ExecuteDiskPartScript(script);

            // Retry on `assign mount` because of this bug: http://social.technet.microsoft.com/Forums/en-US/w7itproinstall/thread/688379ec-79b1-4d2a-a866-a0eef4f3a93c/
            script =
                @"
                    select vdisk file=""{0}""
                    select partition 1
                    rescan
                    assign mount=""{1}""
                ";

            script = string.Format(
                CultureInfo.InvariantCulture,
                script,
                path,
                mountPath);

            int retryCount = 20;
            while (true)
            {
                try
                {
                    ExecuteDiskPartScript(script);
                }
                catch (Exception)
                {
                    if (retryCount > 0)
                    {
                        retryCount--;
                        continue;
                    }

                    throw;
                }

                break;
            }
        }

        /// <summary>
        /// Unmounts the VHD.
        /// </summary>
        /// <param name="path">The path to the VHD.</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Unmount", Justification = "asdf")]
        public static void UnmountVHD(string path)
        {
            string script =
                @"
                    select vdisk file=""{0}""
                    select partition 1
                    remove all dismount noerr
                    detach vdisk
                ";

            script = string.Format(
                CultureInfo.InvariantCulture,
                script,
                path);

            ExecuteDiskPartScript(script);
        }
    }
}
