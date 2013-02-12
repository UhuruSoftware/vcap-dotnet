// -----------------------------------------------------------------------
// <copyright file="IDeaClient.cs" company="Uhuru Software, Inc.">
// Copyright (c) 2011 Uhuru Software, Inc., All Rights Reserved
// </copyright>
// -----------------------------------------------------------------------

namespace Uhuru.CloudFoundry.DEA.DirectoryServer
{
    using System;
    using System.Globalization;

    /// <summary>
    /// Interface for the DEA that gets called by the Directory server.
    /// </summary>
    public interface IDeaClient
    {
        /// <summary>
        /// Looks up the path in the DEA.
        /// </summary>
        /// <param name="path">The path to lookup.</param>
        /// <returns>A PathLookupResponse containing the response from the DEA.</returns>
        PathLookupResponse LookupPath(Uri path);
    }

    /// <summary>
    /// This class contains a DEAs response to a lookup path query from the Directory Server.
    /// </summary>
    public class PathLookupResponse
    {
        /// <summary>
        /// Gets or sets the local path looked up by the DEA.
        /// </summary>
        public string Path
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the error (if any) the DEA encountered while looking up the path.
        /// </summary>
        public PathLookupResponseError Error
        {
            get;
            set;
        }

        /// <summary>
        /// This class contains a DEAs error response to a lookup path query from the Directory Server.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1034:NestedTypesShouldNotBeVisible", Justification = "Class used in very specific cases only; keeping code simpler.")]
        public class PathLookupResponseError
        {
            /// <summary>
            /// Gets or sets the message.
            /// </summary>
            /// <value>
            /// The message.
            /// </value>
            public string Message
            {
                get;
                set;
            }

            /// <summary>
            /// Gets or sets the error code.
            /// </summary>
            /// <value>
            /// The error code.
            /// </value>
            public int ErrorCode
            {
                get;
                set;
            }

            /// <summary>
            /// Returns a <see cref="System.String"/> that represents this instance.
            /// </summary>
            /// <returns>
            /// A <see cref="System.String"/> that represents this instance.
            /// </returns>
            public override string ToString()
            {
                return string.Format(CultureInfo.InvariantCulture, "ErrorCode: {0}, Error: {1}", this.ErrorCode, this.Message);
            }
        }
    }
}
