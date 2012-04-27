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
        public void Init(System.Web.HttpApplication context)
        {
            if (context == null)
            {
                return;
            }

            string path = Path.Combine(context.Server.MapPath("~"), @"..\uhurufs.tsv");
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

                    NetUse(remoteShare, remoteUser, remotePassword);
                }
            }
        }

        /// <summary>
        /// Calls net use with the specified share, user and password.
        /// </summary>
        /// <param name="remoteShare">The UNC path to the share.</param>
        /// <param name="remoteUser">Username used for authentication.</param>
        /// <param name="remotePassword">Password used for authentication.</param>
        private static void NetUse(string remoteShare, string remoteUser, string remotePassword)
        {
            try
            {
                ExecuteProcess(string.Format(CultureInfo.InvariantCulture, @"net use ""{0} {1}"" /USER:{2}", remoteShare, remotePassword, remoteUser));
            }
            catch
            {
                throw;
            }
        }

        /// <summary>
        /// Runs a process and waits for it to return.
        /// </summary>
        /// <param name="command">The command to execute.</param>
        private static void ExecuteProcess(string command)
        {
            using (Process process = Process.Start(command))
            {
                process.WaitForExit();
            }
        }
    }
}
