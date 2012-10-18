// -----------------------------------------------------------------------
// <copyright file="ServiceConfiguration.cs" company="Uhuru Software, Inc.">
// Copyright (c) 2011 Uhuru Software, Inc., All Rights Reserved
// </copyright>
// -----------------------------------------------------------------------

namespace Uhuru.CloudFoundry.ServiceBase
{
    using Uhuru.Utilities.Json;

    /// <summary>
    /// TODO: Update summary.
    /// </summary>
    public class ServiceConfiguration : JsonConvertibleObject
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

        /// <summary>
        /// Gets or sets the service version.
        /// </summary>
        [JsonName("version")]
        public string Version
        {
            get;
            set;
        }
    }
}
