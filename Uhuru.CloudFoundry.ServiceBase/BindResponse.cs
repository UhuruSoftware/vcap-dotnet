// -----------------------------------------------------------------------
// <copyright file="BindResponse.cs" company="Uhuru Software">
// Copyright (c) 2011 Uhuru Software, Inc., All Rights Reserved
// </copyright>
// -----------------------------------------------------------------------

namespace Uhuru.CloudFoundry.ServiceBase
{
    using System.Collections.Generic;
    using Uhuru.Utilities;

    /// <summary>
    /// This class encapsulates a response message to a <see cref="BindRequest"/>.
    /// </summary>
    internal class BindResponse : MessageWithSuccessStatus
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
        /// Gets or sets the credentials that have been generated for the bind request.
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