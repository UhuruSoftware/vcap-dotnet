// -----------------------------------------------------------------------
// <copyright file="DiscoverMessage.cs" company="Uhuru Software, Inc.">
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
    /// This class contains announcement information for a service.
    /// </summary>
    public class DiscoverMessage : JsonConvertibleObject
    {
        /// <summary>
        /// Gets or sets the service plan.
        /// </summary>
        [JsonName("plan")]
        public string Plan
        {
            get;
            set;
        }
    }
}