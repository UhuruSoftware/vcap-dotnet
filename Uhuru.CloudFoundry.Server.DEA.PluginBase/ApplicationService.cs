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
        public string name;
        public string user;
        public string password;
        public int port;
        public string plan;
        public Dictionary<string, object> planOptions;
        public string host;
        public string instanceName;
        public string serviceLabel;
        public string[] serviceTags;

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
