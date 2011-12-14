// -----------------------------------------------------------------------
// <copyright file="UnprovisionRequest.cs" company="Uhuru Software, Inc.">
// Copyright (c) 2011 Uhuru Software, Inc., All Rights Reserved
// </copyright>
// -----------------------------------------------------------------------

namespace Uhuru.CloudFoundry.ServiceBase
{
    using System.Collections.Generic;
    using Uhuru.Utilities;

    /// <summary>
    /// This class encapsulate a request message to unprovision a service.
    /// </summary>
    internal class UnprovisionRequest : JsonConvertibleObject
    {
        /// <summary>
        /// Gets or sets the name of the service to be unprovisioned.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "These are used for (de)serialization"),
        JsonName("name")]
        public string Name
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the bindings of the service that have to be unprovisioned.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "These are used for (de)serialization"),
        JsonName("bindings")]
        public ServiceCredentials[] Bindings
        {
            get;
            set;
        }
    }
}