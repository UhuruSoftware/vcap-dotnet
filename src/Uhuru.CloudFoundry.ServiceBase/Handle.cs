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
    public class Handle
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
        /// Gets or sets the service credentials.
        /// </summary>
        [JsonName("credentials")]
        public ServiceCredentials Credentials
        {
            get;
            set;
        }
    }
}