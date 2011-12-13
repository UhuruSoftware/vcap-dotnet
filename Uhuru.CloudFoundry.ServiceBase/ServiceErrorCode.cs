// -----------------------------------------------------------------------
// <copyright file="ServiceErrorCode.cs" company="Uhuru Software">
// Copyright (c) 2011 Uhuru Software, Inc., All Rights Reserved
// </copyright>
// -----------------------------------------------------------------------

namespace Uhuru.CloudFoundry.ServiceBase
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    /// <summary>
    /// This structure contains service error information.
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1815:OverrideEqualsAndOperatorEqualsOnValueTypes", Justification = "Instances of this type are never compared.")]
    public struct ServiceErrorCode
    {
        /// <summary>
        /// Gets or sets an error code.
        /// </summary>
        public int ErrorCode
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets an http error code for the service error.
        /// </summary>
        public HttpErrorCode HttpError
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the error message.
        /// </summary>
        public string Message
        {
            get;
            set;
        }
    }
}
