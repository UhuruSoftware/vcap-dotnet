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
        private string name;
        private string user;
        private string password;
        private int port;
        private string plan;
        private Dictionary<string, object> planOptions;
        private string host;
        private string instanceName;
        private string serviceLabel;
        private string[] serviceTags;

        /// <summary>
        /// Initializes a new instance of the <see cref="ApplicationService"/> class.
        /// </summary>
        /// <param name="name">The name of the provisioned service.</param>
        /// <param name="user">The user that was provisioned for the service.</param>
        /// <param name="password">The password for the user.</param>
        /// <param name="port">The port used to connect to the service.</param>
        /// <param name="plan">The billing plan.</param>
        /// <param name="planOptions">The billing plan options.</param>
        /// <param name="host">The host used to connect to the service.</param>
        /// <param name="instanceName">Name of the instance (e.g. database name).</param>
        /// <param name="serviceLabel">The service label (e.g. mssql-2008).</param>
        /// <param name="serviceTags">The service tags.</param>
        public ApplicationService(string name, string user, string password, int port,  string plan, Dictionary<string, object> planOptions, string host, string instanceName, string serviceLabel, string[] serviceTags)
        {
            this.name = name;
            this.user = user;
            this.password = password;
            this.port = port;
            this.plan = plan;
            this.planOptions = planOptions;
            this.host = host;
            this.instanceName = instanceName;
            this.serviceLabel = serviceLabel;
            this.serviceTags = serviceTags;
        }

        /// <summary>
        /// the name of this instance of the service
        /// </summary>
        public string Name
        {
            get
            {
                return name;
            }
        }

        /// <summary>
        /// the user to authenticate
        /// </summary>
        public string User
        {
            get
            {
                return user;
            }
        }

        /// <summary>
        /// the password of the user to authenticate
        /// </summary>
        public string Password
        {
            get
            {
                return password;
            }
        }

        /// <summary>
        /// the port where the service will be available
        /// </summary>
        public int Port
        {
            get
            {
                return port;
            }
        }

        /// <summary>
        /// the usage plan of the service
        /// </summary>
        public string Plan
        {
            get
            {
                return plan;
            }
        }

        /// <summary>
        /// details regarding the usage plan of the service
        /// </summary>
        public Dictionary<string, object> GetPlanOptions() 
        { 
            return planOptions;
        }

        /// <summary>
        /// the host where the service will be made available
        /// </summary>
        public string Host
        {
            get
            {
                return host;
            }
        }

        /// <summary>
        /// Name of the database/key value store/etc.
        /// </summary>
        public string InstanceName
        {
            get
            {
                return instanceName;
            }
        }

        /// <summary>
        /// Service type label (mssql-2008, mysql-5.1, etc.).
        /// </summary>
        public string ServiceLabel
        {
            get
            {
                return serviceLabel;
            }
        }

        /// <summary>
        /// Service type tags.
        /// </summary>
        public string[] ServiceTags
        {
            get
            {
                return serviceTags;
            }
        }

    }
}
