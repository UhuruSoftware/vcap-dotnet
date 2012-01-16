// -----------------------------------------------------------------------
// <copyright file="TunnelPackage.cs" company="Uhuru Software, Inc.">
// Copyright (c) 2011 Uhuru Software, Inc., All Rights Reserved
// </copyright>
// -----------------------------------------------------------------------

namespace Uhuru.Utilities
{
    using System.IO;
    using System.Xml;

    /// <summary>
    /// TODO: Update summary.
    /// </summary>
    public static class TunnelPackage
    {
        /// <summary>
        /// Prepares a HttpTunnel Service
        /// </summary>
        /// <param name="sourceDir">The source dir.</param>
        /// <param name="destinationDir">The destination dir.</param>
        /// <param name="remotePort">The remote port.</param>
        /// <param name="remoteHost">The remote host.</param>
        /// <param name="remoteProtocol">The remote protocol.</param>
        public static void Create(string sourceDir, string destinationDir, string remotePort, string remoteHost, string remoteProtocol)
        {
            CopyAll(new DirectoryInfo(sourceDir), new DirectoryInfo(destinationDir));
            string configFile = Path.Combine(destinationDir, "web.config");

            XmlDocument doc = new XmlDocument();
            doc.Load(configFile);
            XmlNode appSettingsNode = doc.SelectSingleNode("configuration/appSettings");

            foreach (XmlNode childNode in appSettingsNode)
            {
                switch (childNode.Attributes["key"].Value)
                {
                    case "destinationIp":
                        childNode.Attributes["value"].Value = remoteHost;
                        break;
                    case "destinationPort":
                        childNode.Attributes["value"].Value = remotePort;
                        break;
                    case "protocol":
                        childNode.Attributes["value"].Value = remoteProtocol;
                        break;
                    default:
                        break;
                }
            }

            doc.Save(configFile);
        }

        /// <summary>
        /// Copies all files and subfolders from one folder to another
        /// </summary>
        /// <param name="source">The source directory info</param>
        /// <param name="target">The target directory info</param>
        private static void CopyAll(DirectoryInfo source, DirectoryInfo target)
        {
            if (Directory.Exists(target.FullName) == false)
            {
                Directory.CreateDirectory(target.FullName);
            }

            foreach (FileInfo fi in source.GetFiles())
            {
                fi.CopyTo(Path.Combine(target.ToString(), fi.Name), true);
            }

            foreach (DirectoryInfo sourceSubDir in source.GetDirectories())
            {
                DirectoryInfo nextTargetSubDir =
                    target.CreateSubdirectory(sourceSubDir.Name);
                CopyAll(sourceSubDir, nextTargetSubDir);
            }
        }
    }
}
