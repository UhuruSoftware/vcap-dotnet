// -----------------------------------------------------------------------
// <copyright file="ServiceCredentials.cs" company="Uhuru Software, Inc.">
// Copyright (c) 2011 Uhuru Software, Inc., All Rights Reserved
// </copyright>
// -----------------------------------------------------------------------

namespace Uhuru.CloudFoundry.ServiceBase
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Runtime.Serialization;
    using System.Xml.Serialization;
    using Uhuru.Utilities;

    /// <summary>
    /// This class contains information about service credentials.
    /// </summary>
    public class ServiceCredentials : JsonConvertibleObject
    {
        /// <summary>
        /// Binding options for the service credentials settings.
        /// </summary>
        private Dictionary<string, object> bindOptions = new Dictionary<string, object>();

        /// <summary>
        /// Gets or sets the Node ID.
        /// </summary>
        [JsonName("node_id")]
        public string NodeId
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the provisioned service name.
        /// </summary>
        [JsonName("name")]
        public string Name
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the username.
        /// </summary>
        [JsonName("username")]
        public string UserName
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the username.
        /// </summary>
        [JsonName("user")]
        public string User
        { 
            get; 
            set; 
        }

        /// <summary>
        /// Gets or sets the password.
        /// </summary>
        [JsonName("password")]
        public string Password
        { 
            get; 
            set; 
        }

        /// <summary>
        /// Gets or sets the hostname of the provisioned service.
        /// </summary>
        [JsonName("hostname")]
        public string HostName
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the port of the provisioned service.
        /// </summary>
        [JsonName("port")]
        public int Port
        { 
            get; 
            set; 
        }

        /// <summary>
        /// Gets or sets the bind options for the provisioned service.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly", Justification = "The setter is used by the json convertible object"), JsonName("bind_opts")]
        public Dictionary<string, object> BindOptions
        { 
            get
            {
                return this.bindOptions;
            }

            set
            {
                this.bindOptions = value;
            }
        }
    }
}