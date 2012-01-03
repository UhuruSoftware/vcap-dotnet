// -----------------------------------------------------------------------
// <copyright file="UnbindRequest.cs" company="Uhuru Software, Inc.">
// Copyright (c) 2011 Uhuru Software, Inc., All Rights Reserved
// </copyright>
// -----------------------------------------------------------------------

namespace Uhuru.CloudFoundry.ServiceBase
{
    using System.Collections.Generic;
    using Uhuru.Utilities;
    using Uhuru.Utilities.Json;

    /// <summary>
    /// This class encapsulated a request to unbind a service from an app.
    /// </summary>
    internal class UnbindRequest : JsonConvertibleObject
    {
        /// <summary>
        /// Gets or sets the credentials that have to be unbound.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "These are needed for JSON (de)serialization"),
        JsonName("credentials")]
        public ServiceCredentials Credentials
        {
            get;
            set;
        }
    }
}