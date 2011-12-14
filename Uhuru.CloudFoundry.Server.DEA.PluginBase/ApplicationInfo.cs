// -----------------------------------------------------------------------
// <copyright file="ApplicationInfo.cs" company="Uhuru Software, Inc.">
// Copyright (c) 2011 Uhuru Software, Inc., All Rights Reserved
// </copyright>
// -----------------------------------------------------------------------

namespace Uhuru.CloudFoundry.Server.DEA.PluginBase
{
    using System;
    
    /// <summary>
    /// a class holding the basic information of an application
    /// </summary>
    public class ApplicationInfo : MarshalByRefObject
    {
        /// <summary>
        /// Gets or sets the id of the current application instance
        /// </summary>
        public string InstanceId { get; set; }

        /// <summary>
        /// Gets or sets the ip where the app is to be found
        /// </summary>
        public string LocalIP { get; set; }

        /// <summary>
        /// Gets or sets the port where the app is to be found
        /// </summary>
        public int Port { get; set; }

        /// <summary>
        /// Gets or sets the name of the application
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the physical path of the app
        /// </summary>
        public string Path { get; set; }

        /// <summary>
        /// Gets or sets the username to authenticate
        /// </summary>
        public string WindowsUserName { get; set; }

        /// <summary>
        /// Gets or sets the password of the user to authenticate
        /// </summary>
        public string WindowsPassword { get; set; }
    }
}
