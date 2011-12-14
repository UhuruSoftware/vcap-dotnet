// -----------------------------------------------------------------------
// <copyright file="MessageWithSuccessStatus.cs" company="Uhuru Software, Inc.">
// Copyright (c) 2011 Uhuru Software, Inc., All Rights Reserved
// </copyright>
// -----------------------------------------------------------------------

namespace Uhuru.CloudFoundry.ServiceBase
{
    using System.Collections.Generic;
    using Uhuru.Utilities;

    /// <summary>
    /// This is a base class for a message that is received through NATS and that has a success status.
    /// </summary>
    internal abstract class MessageWithSuccessStatus : JsonConvertibleObject
    {
        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="MessageWithSuccessStatus"/> represents a successful operation.
        /// </summary>
        public abstract bool Success
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the error that occured during an operation.
        /// </summary>
        public abstract Dictionary<string, object> Error
        {
            get;
            set;
        }
    }
}