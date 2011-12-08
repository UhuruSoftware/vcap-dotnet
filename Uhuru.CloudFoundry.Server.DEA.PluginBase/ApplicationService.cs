// -----------------------------------------------------------------------
// <copyright file="ApplicationService.cs" company="Uhuru Software">
// Copyright (c) 2011 Uhuru Software, Inc., All Rights Reserved
// </copyright>
// -----------------------------------------------------------------------

using System;
using System.Collections.Generic;

namespace Uhuru.CloudFoundry.Server.DEA.PluginBase
{
    /// <summary>
    /// holds the data related to an application service
    /// </summary>
    public class ApplicationService : MarshalByRefObject
    {
        /// <summary>
        /// the name of this instance of the service
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// the user to authenticate
        /// </summary>
        public string User { get; set; }

        /// <summary>
        /// the password of the user to authenticate
        /// </summary>
        public string Password { get; set; }

        /// <summary>
        /// the port where the service will be available
        /// </summary>
        public int Port { get; set; }

        /// <summary>
        /// the usage plan of the service
        /// </summary>
        public string Plan { get; set; }

        /// <summary>
        /// details regarding the usage plan of the service
        /// </summary>
        public Dictionary<string, object> PlanOptions { get; set; }

        /// <summary>
        /// the host where the service will be made available
        /// </summary>
        public string Host { get; set; }

        /// <summary>
        /// Name of the database/key value store/etc.
        /// </summary>
        public string InstanceName { get; set; }

        /// <summary>
        /// Service type label (mssql-2008, mysql-5.1, etc.).
        /// </summary>
        public string ServiceLabel { get; set; }

        /// <summary>
        /// Service type tags.
        /// </summary>
        public string[] ServiceTags { get; set; }

    }
}
