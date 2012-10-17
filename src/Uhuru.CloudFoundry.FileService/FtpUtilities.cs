// -----------------------------------------------------------------------
// <copyright file="FtpUtilities.cs" company="Uhuru Software, Inc.">
// Copyright (c) 2011 Uhuru Software, Inc., All Rights Reserved
// </copyright>
// -----------------------------------------------------------------------

namespace Uhuru.CloudFoundry.FileService
{
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using Microsoft.Web.Administration;

    /// <summary>
    /// This utilities class contains helper functions for managing FTP virtual directories
    /// </summary>
    public static class FtpUtilities
    {
        /// <summary>
        /// Creates an ftp virtual directory on the "fileService" ftp site.
        /// </summary>
        /// <param name="siteName">The name of the new ftp site.</param>
        /// <param name="directory">The target physical path of the virtual directory.</param>
        /// <param name="port">The port.</param>
        public static void CreateFtpSite(string siteName, string directory, int port)
        {
            using (ServerManager serverManager = new ServerManager())
            {
                Configuration config = serverManager.GetApplicationHostConfiguration();

                ConfigurationSection sitesSection = config.GetSection("system.applicationHost/sites");

                ConfigurationElementCollection sitesCollection = sitesSection.GetCollection();

                ConfigurationElement siteElement = sitesCollection.CreateElement("site");
                siteElement["name"] = siteName;
                siteElement["id"] = port;

                ConfigurationElementCollection bindingsCollection = siteElement.GetCollection("bindings");

                ConfigurationElement bindingElement = bindingsCollection.CreateElement("binding");
                bindingElement["protocol"] = @"ftp";
                bindingElement["bindingInformation"] = string.Format(CultureInfo.InvariantCulture, @"*:{0}:", port);
                bindingsCollection.Add(bindingElement);

                ConfigurationElement ftpServerElement = siteElement.GetChildElement("ftpServer");

                ConfigurationElement securityElement = ftpServerElement.GetChildElement("security");

                ConfigurationElement sslElement = securityElement.GetChildElement("ssl");
                sslElement["controlChannelPolicy"] = "SslAllow";
                sslElement["dataChannelPolicy"] = "SslAllow";
                
                ConfigurationElement authenticationElement = securityElement.GetChildElement("authentication");

                ConfigurationElement basicAuthenticationElement = authenticationElement.GetChildElement("basicAuthentication");
                basicAuthenticationElement["enabled"] = true;
        
                ConfigurationElementCollection siteCollection = siteElement.GetCollection();

                ConfigurationElement applicationElement = siteCollection.CreateElement("application");
                applicationElement["path"] = @"/";

                ConfigurationElementCollection applicationCollection = applicationElement.GetCollection();

                ConfigurationElement virtualDirectoryElement = applicationCollection.CreateElement("virtualDirectory");
                virtualDirectoryElement["path"] = @"/";
                virtualDirectoryElement["physicalPath"] = new DirectoryInfo(directory).FullName;
                applicationCollection.Add(virtualDirectoryElement);
                siteCollection.Add(applicationElement);
                sitesCollection.Add(siteElement);
                
                serverManager.CommitChanges();
            }

            Uhuru.Utilities.FirewallTools.OpenPort(port, string.Format(CultureInfo.InvariantCulture, "FTP_{0}", siteName));
        }

        /// <summary>
        /// Deletes an FTP virtual directory.
        /// </summary>
        /// <param name="name">The name of the virtual directory.</param>
        public static void DeleteFtpSite(string name)
        {
            using (ServerManager serverManager = new ServerManager())
            {
                Configuration config = serverManager.GetApplicationHostConfiguration();

                ConfigurationSection sitesSection = config.GetSection("system.applicationHost/sites");

                ConfigurationElementCollection sitesCollection = sitesSection.GetCollection();

                ConfigurationElement fileServiceSite = sitesCollection.FirstOrDefault(c => (string)c.Attributes["name"].Value == name);

                if (fileServiceSite != null)
                {
                    sitesCollection.Remove(fileServiceSite);
                }
                
                ConfigurationElementCollection bindingsCollection = fileServiceSite.GetCollection("bindings");

                ConfigurationElement bindingElement = bindingsCollection.CreateElement("binding");
                bindingElement["protocol"] = @"ftp";
                string bindingInformation = bindingElement["bindingInformation"] as string;

                if (!string.IsNullOrEmpty(bindingInformation))
                {
                    string[] bindingParts = bindingInformation.Split(':');
                    if (bindingParts.Length > 2)
                    {
                        int port = 0;
                        if (int.TryParse(bindingParts[1], out port))
                        {
                            Uhuru.Utilities.FirewallTools.ClosePort(port);
                        }
                    }
                }

                serverManager.CommitChanges();
            }
        }

        /// <summary>
        /// Verify if the ftp site exists.
        /// </summary>
        /// <param name="name">The name of the virtual directory.</param>
        /// <returns>True if the ftp site exists.</returns>
        public static bool Exists(string name)
        {
            using (ServerManager serverManager = new ServerManager())
            {
                Configuration config = serverManager.GetApplicationHostConfiguration();

                ConfigurationSection sitesSection = config.GetSection("system.applicationHost/sites");

                ConfigurationElementCollection sitesCollection = sitesSection.GetCollection();

                ConfigurationElement fileServiceSite = sitesCollection.FirstOrDefault(c => (string)c.Attributes["name"].Value == name);

                if (fileServiceSite == null)
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Adds the user access.
        /// </summary>
        /// <param name="siteName">Name of the site.</param>
        /// <param name="user">The user.</param>
        public static void AddUserAccess(string siteName, string user)
        {
            using (ServerManager serverManager = new ServerManager())
            {
                Configuration config = serverManager.GetApplicationHostConfiguration();

                ConfigurationElementCollection authorization = config.GetSection("system.ftpServer/security/authorization", siteName).GetCollection();
                ConfigurationElement newAuthorization = authorization.CreateElement("add");
                newAuthorization["accessType"] = "Allow";
                newAuthorization["users"] = user;
                newAuthorization["permissions"] = "Read, Write";

                authorization.Add(newAuthorization);

                serverManager.CommitChanges();
            }
        }

        /// <summary>
        /// Deletes the user access.
        /// </summary>
        /// <param name="siteName">Name of the site.</param>
        /// <param name="user">The user.</param>
        public static void DeleteUserAccess(string siteName, string user)
        {
            using (ServerManager serverManager = new ServerManager())
            {
                Configuration config = serverManager.GetApplicationHostConfiguration();

                ConfigurationElementCollection authorization = config.GetSection("system.ftpServer/security/authorization", siteName).GetCollection();

                ConfigurationElement userAccess = authorization.Where(a => (string)a["users"] == user).FirstOrDefault();

                authorization.Remove(userAccess);

                serverManager.CommitChanges();
            }
        }

        /// <summary>
        /// Adds the group access.
        /// </summary>
        /// <param name="siteName">Name of the site.</param>
        /// <param name="group">The group.</param>
        public static void AddGroupAccess(string siteName, string group)
        {
            using (ServerManager serverManager = new ServerManager())
            {
                Configuration config = serverManager.GetApplicationHostConfiguration();

                ConfigurationElementCollection authorization = config.GetSection("system.ftpServer/security/authorization", siteName).GetCollection();
                ConfigurationElement newAuthorization = authorization.CreateElement("add");
                newAuthorization["accessType"] = "Allow";
                newAuthorization["roles"] = group;
                newAuthorization["permissions"] = "Read, Write";

                authorization.Add(newAuthorization);

                serverManager.CommitChanges();
            }
        }

        /// <summary>
        /// Deletes the group access.
        /// </summary>
        /// <param name="siteName">Name of the site.</param>
        /// <param name="group">The group.</param>
        public static void DeleteGroupAccess(string siteName, string group)
        {
            using (ServerManager serverManager = new ServerManager())
            {
                Configuration config = serverManager.GetApplicationHostConfiguration();

                ConfigurationElementCollection authorization = config.GetSection("system.ftpServer/security/authorization", siteName).GetCollection();

                ConfigurationElement userAccess = authorization.Where(a => (string)a["roles"] == group).FirstOrDefault();

                authorization.Remove(userAccess);

                serverManager.CommitChanges();
            }
        }
    }
}