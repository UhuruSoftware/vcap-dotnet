// -----------------------------------------------------------------------
// <copyright file="Handle.cs" company="Uhuru Software, Inc.">
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
    using Uhuru.Utilities.Json;

    /// <summary>
    /// This is a class containing information about a provisioned service.
    /// </summary>
    public class Handle : JsonConvertibleObject
    {
        /// <summary>
        /// Gets or sets the service ID.
        /// </summary>
        [JsonName("service_id")]
        public string ServiceId
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the configuration.
        /// </summary>
        [JsonName("configuration")]
        public ServiceConfiguration Configuration
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the service credentials.
        /// </summary>
        [JsonName("credentials")]
        public ServiceCredentials Credentials
        {
            get;
            set;
        }

        /// <summary>
        /// Converts this instance to a Dictionary&lt;string, object&gt; that is ready to be serialized to a Ruby-compatible JSON.
        /// </summary>
        /// <returns>A Dictionary&lt;string, object&gt;</returns>
        public Dictionary<string, object> ToJson()
        {
            Dictionary<string, object> intermediateObject = this.ToJsonIntermediateObject();
            intermediateObject["configuration"] = this.Configuration.ToJsonIntermediateObject();
            return intermediateObject;
        }
    }
}