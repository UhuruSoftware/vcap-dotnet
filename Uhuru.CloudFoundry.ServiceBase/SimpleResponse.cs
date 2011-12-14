// -----------------------------------------------------------------------
// <copyright file="SimpleResponse.cs" company="Uhuru Software, Inc.">
// Copyright (c) 2011 Uhuru Software, Inc., All Rights Reserved
// </copyright>
// -----------------------------------------------------------------------

namespace Uhuru.CloudFoundry.ServiceBase
{
    using System.Collections.Generic;
    using Uhuru.Utilities;

    /// <summary>
    /// This class encapsulates a simple response message, that only contains the status of the requested operation.
    /// </summary>
    internal class SimpleResponse : MessageWithSuccessStatus
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