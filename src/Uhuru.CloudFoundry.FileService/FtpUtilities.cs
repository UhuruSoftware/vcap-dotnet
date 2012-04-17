// -----------------------------------------------------------------------
// <copyright file="FtpUtilities.cs" company="Uhuru Software, Inc.">
// Copyright (c) 2011 Uhuru Software, Inc., All Rights Reserved
// </copyright>
// -----------------------------------------------------------------------

namespace Uhuru.CloudFoundry.FileService
{
    using System.Globalization;
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
        /// <param name="name">The name of the new ftp site.</param>
        /// <param name="directory">The target physical path of the virtual directory.</param>
        /// <param name="user">The Windows user that will have access to the virtual directory.</param>
        /// <returns>The port used for the new site.</returns>
        public static int CreateFtpSite(string name, string directory, string user)
        {
            int port = Uhuru.Utilities.NetworkInterface.GrabEphemeralPort();

            using (ServerManager serverManager = new ServerManager())
            {
                Configuration config = serverManager.GetApplicationHostConfiguration();

                ConfigurationSection sitesSection = config.GetSection("system.applicationHost/sites");

                ConfigurationElementCollection sitesCollection = sitesSection.GetCollection();

                ConfigurationElement siteElement = sitesCollection.CreateElement("site");
                siteElement["name"] = name;
                siteElement["id"] = port;

                ConfigurationElementCollection bindingsCollection = siteElement.GetCollection("bindings");

                ConfigurationElement bindingElement = bindingsCollection.CreateElement("binding");
                bindingElement["protocol"] = @"ftp";
                bindingElement["bindingInformation"] = string.Format(CultureInfo.InvariantCulture, @"*:{0}:", port);
                bindingsCollection.Add(bindingElement);

                ConfigurationElement ftpServerElement = siteElement.GetChildElement("ftpServer");

                ConfigurationElement securityElement = ftpServerElement.GetChildElement("security");

                ConfigurationElement authenticationElement = securityElement.GetChildElement("authentication");

                ConfigurationElement basicAuthenticationElement = authenticationElement.GetChildElement("basicAuthentication");
                basicAuthenticationElement["enabled"] = true;
        
                ConfigurationElementCollection siteCollection = siteElement.GetCollection();

                ConfigurationElement applicationElement = siteCollection.CreateElement("application");
                applicationElement["path"] = @"/";

                ConfigurationElementCollection applicationCollection = applicationElement.GetCollection();

                ConfigurationElement virtualDirectoryElement = applicationCollection.CreateElement("virtualDirectory");
                virtualDirectoryElement["path"] = @"/";
                virtualDirectoryElement["physicalPath"] = directory;
                applicationCollection.Add(virtualDirectoryElement);
                siteCollection.Add(applicationElement);
                sitesCollection.Add(siteElement);

                ConfigurationElementCollection authorization = config.GetSection("system.ftpServer/security/authorization", name).GetCollection();
                ConfigurationElement newAuthorization = authorization.CreateElement("add");
                newAuthorization["accessType"] = "Allow";
                newAuthorization["users"] = user;
                newAuthorization["permissions"] = "Read, Write";

                authorization.Add(newAuthorization);
                
                serverManager.CommitChanges();
            }

            Uhuru.Utilities.FirewallTools.OpenPort(port, string.Format(CultureInfo.InvariantCulture, "FTP_{0}", name));

            return port;
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
    }
}