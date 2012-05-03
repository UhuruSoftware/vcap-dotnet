// -----------------------------------------------------------------------
// <copyright file="IISUhuruFSModule.cs" company="Uhuru Software, Inc.">
// Copyright (c) 2011 Uhuru Software, Inc., All Rights Reserved
// </copyright>
// -----------------------------------------------------------------------

namespace Uhuru.CloudFoundry.DEA.Plugins.IIS
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Globalization;
    using System.IO;
    using System.Text;
    using System.Web;

    /// <summary>
    /// Module used 
    /// </summary>
    public class IISUhuruFSModule : IHttpModule
    {
        /// <summary>
        /// Releases unmanaged and - optionally - managed resources
        /// </summary>
        public void Dispose()
        {
        }

        /// <summary>
        /// Method called when the IIS app initializes.
        /// </summary>
        /// <param name="context">The http context of the application.</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "Used for logging.")]
        public void Init(System.Web.HttpApplication context)
        {
            string appApth = System.Web.Hosting.HostingEnvironment.MapPath("~");
            string logFile = Path.Combine(appApth, @"..\logs\startup.log");

            string path = Path.Combine(appApth, @"..\uhurufs.tsv");
            if (File.Exists(path))
            {
                string[] lines = File.ReadAllLines(path);
                foreach (string line in lines)
                {
                    string[] parts = line.Split('\t');
                    if (parts.Length != 3)
                    {
                        continue;
                    }

                    string remoteShare = parts[0];
                    string remoteUser = parts[1];
                    string remotePassword = parts[2];

                    File.AppendAllText(logFile, string.Format(CultureInfo.InvariantCulture, "Connecting to \"{0}\" with username \"{1}\"... ", remoteShare, remoteUser));
                    try
                    {
                        NetUse(remoteShare, remoteUser, remotePassword);
                    }
                    catch (Exception e)
                    {
                        File.AppendAllText(logFile, string.Format(CultureInfo.InvariantCulture, "Error: \"{0}\".\n", e.Message));
                        throw;
                    }

                    File.AppendAllText(logFile, string.Format(CultureInfo.InvariantCulture, "Done.\n"));
                }
            }
        }

        /// <summary>
        /// Calls net use with the specified share, user and password.
        /// </summary>
        /// <param name="remoteShare">The UNC path to the share.</param>
        /// <param name="remoteUser">Username used for authentication.</param>
        /// <param name="remotePassword">Password used for authentication.</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2201:DoNotRaiseReservedExceptionTypes", Justification = "Enough for this purpose")]
        private static void NetUse(string remoteShare, string remoteUser, string remotePassword)
        {
            int retCode = ExecuteProcess("net", string.Format(CultureInfo.InvariantCulture, @"use ""{0}"" ""{1}"" /USER:""{2}""", remoteShare, remotePassword, remoteUser));
            if (retCode != 0)
            {
                throw new Exception("Net command exit code is different from 0");
            }
        }

        /// <summary>
        /// Runs a process and waits for it to return.
        /// </summary>
        /// <param name="processFile">The command to execute.</param>
        /// <param name="processArguments">The arguments.</param>
        /// <returns>
        /// Process return code
        /// </returns>
        private static int ExecuteProcess(string processFile, string processArguments)
        {
            ProcessStartInfo pi = new ProcessStartInfo(processFile, processArguments);
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
