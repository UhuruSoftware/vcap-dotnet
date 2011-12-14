// -----------------------------------------------------------------------
// <copyright file="ApplicationService.cs" company="Uhuru Software, Inc.">
// Copyright (c) 2011 Uhuru Software, Inc., All Rights Reserved
// </copyright>
// -----------------------------------------------------------------------

namespace Uhuru.CloudFoundry.Server.DEA.PluginBase
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// holds the data related to an application service
    /// </summary>
    public class ApplicationService : MarshalByRefObject
    {
        /// <summary>
        /// The name of the provisioned service.
        /// </summary>
        private string name;

        /// <summary>
        /// The user that was provisioned for the service.
        /// </summary>
        private string user;

        /// <summary>
        /// The password for the user.
        /// </summary>
        private string password;

        /// <summary>
        /// The port used to connect to the service.
        /// </summary>
        private int port;

        /// <summary>
        /// The billing plan.
        /// </summary>
        private string plan;

        /// <summary>
        /// The billing plan options.
        /// </summary>
        private Dictionary<string, object> planOptions;

        /// <summary>
        /// The host used to connect to the service.
        /// </summary>
        private string host;

        /// <summary>
        /// Name of the instance (e.g. database name).
        /// </summary>
        private string instanceName;

        /// <summary>
        /// The service label (e.g. mssql-2008).
        /// </summary>
        private string serviceLabel;

        /// <summary>
        /// The service tags.
        /// </summary>
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
        /// Gets the name of this instance of the service
        /// </summary>
        public string Name
        {
            get
            {
                return this.name;
            }
        }

        /// <summary>
        /// Gets the user to authenticate
        /// </summary>
        public string User
        {
            get
            {
                return this.user;
            }
        }

        /// <summary>
        /// Gets the password of the user to authenticate
        /// </summary>
        public string Password
        {
            get
            {
                return this.password;
            }
        }

        /// <summary>
        /// Gets the port where the service will be available
        /// </summary>
        public int Port
        {
            get
            {
                return this.port;
            }
        }

        /// <summary>
        /// Gets the usage plan of the service
        /// </summary>
        public string Plan
        {
            get
            {
                return this.plan;
            }
        }

        /// <summary>
        /// Gets the host where the service will be made available
        /// </summary>
        public string Host
        {
            get
            {
                return this.host;
            }
        }

        /// <summary>
        /// Gets the name of the database/key value store/etc.
        /// </summary>
        public string InstanceName
        {
            get
            {
                return this.instanceName;
            }
        }

        /// <summary>
        /// Gets the service type label (mssql-2008, mysql-5.1, etc.).
        /// </summary>
        public string ServiceLabel
        {
            get
            {
                return this.serviceLabel;
            }
        }

        /// <summary>
        /// Gets details regarding the usage plan of the service
        /// </summary>
        public Dictionary<string, object> PlanOptions
        {
            get
            {
                return this.planOptions;
            }
        }

        /// <summary>
        /// Service type tags.
        /// </summary>
        /// <returns>
        /// An array of service tags
        /// </returns>
        public string[] GetServiceTags()
        {
            return this.serviceTags;
        }
    }
}
