﻿// -----------------------------------------------------------------------
// <copyright file="ProvisionResponse.cs" company="Uhuru Software, Inc.">
// Copyright (c) 2011 Uhuru Software, Inc., All Rights Reserved
// </copyright>
// -----------------------------------------------------------------------

namespace Uhuru.CloudFoundry.ServiceBase
{
    using System.Collections.Generic;
    using Uhuru.Utilities;
    using Uhuru.Utilities.Json;

    /// <summary>
    /// This class encapsulates a response message to a <see cref="ProvisionRequest"/>.
    /// </summary>
    internal class ProvisionResponse : MessageWithSuccessStatus
    {
        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="MessageWithSuccessStatus"/> represents a successful operation.
        /// </summary>
        [JsonName("success")]
        public override bool Success
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the credentials that have been provisioned.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "These are used for (de)serialization"), 
        JsonName("credentials")]
        public ServiceCredentials Credentials
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the error that occured during an operation.
        /// </summary>
        [JsonName("error")]
        public override Dictionary<string, object> Error
        {
            get;
            set;
        }
    }
}