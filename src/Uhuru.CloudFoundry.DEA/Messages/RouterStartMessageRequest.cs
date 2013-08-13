// -----------------------------------------------------------------------
// <copyright file="RouterStartMessageRequest.cs" company="Uhuru Software, Inc.">
// Copyright (c) 2011 Uhuru Software, Inc., All Rights Reserved
// </copyright>
// -----------------------------------------------------------------------

namespace Uhuru.CloudFoundry.DEA.Messages
{
    using System.Collections.Generic;
    using Uhuru.Utilities;
    using Uhuru.Utilities.Json;

    /// <summary>
    /// This class is a representation of a DEA status message response.
    /// </summary>
    public class RouterStartMessageRequest : JsonConvertibleObject
    {
        /// <summary>
        /// Gets or sets the minimumRegisterIntervalInSeconds.
        /// </summary>
        [JsonName("minimumRegisterIntervalInSeconds")]
        public int MinimumRegisterIntervalInSeconds
        {
            get;
            set;
        }
    }
}
