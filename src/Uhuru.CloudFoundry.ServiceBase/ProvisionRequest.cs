// -----------------------------------------------------------------------
// <copyright file="ProvisionRequest.cs" company="Uhuru Software, Inc.">
// Copyright (c) 2011 Uhuru Software, Inc., All Rights Reserved
// </copyright>
// -----------------------------------------------------------------------

namespace Uhuru.CloudFoundry.ServiceBase
{
    using System.Collections.Generic;
    using Uhuru.Utilities;
    using Uhuru.Utilities.Json;

    /// <summary>
    /// This encapsulates a request message for provisioning a service.
    /// </summary>
    internal class ProvisionRequest : JsonConvertibleObject
    {
        /// <summary>
        /// Gets or sets the billing plan of the service that is to be provisioned.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "These are used for (de)serialization"), 
        JsonName("plan")]
        public ProvisionedServicePlanType Plan
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the credentials information for the service that is to be provisioned.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "These are used for (de)serialization"),
        JsonName("credentials")]
        public ServiceCredentials Credentials
        {
            get;
            set;
        }
    }
}