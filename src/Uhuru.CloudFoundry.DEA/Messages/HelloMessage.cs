// -----------------------------------------------------------------------
// <copyright file="HelloMessage.cs" company="Uhuru Software, Inc.">
// Copyright (c) 2011 Uhuru Software, Inc., All Rights Reserved
// </copyright>
// -----------------------------------------------------------------------

namespace Uhuru.CloudFoundry.DEA.Messages
{
    using Uhuru.Utilities;
    using Uhuru.Utilities.Json;

    /// <summary>
    /// This class encapsulates a hello message.
    /// </summary>
    internal class HelloMessage : JsonConvertibleObject
    {
        /// <summary>
        /// Gets or sets the id of the DEA service.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "It is used for JSON (de)serialization."), 
        JsonName("id")]
        public string Id
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the version of the service.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "It is used for JSON (de)serialization."), 
        JsonName("version")]
        public decimal Version
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the host on which the service runs.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "It is used for JSON (de)serialization."), 
        JsonName("ip")]
        public string Host
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the file viewer port for the DEA service.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "It is used for JSON (de)serialization."), 
        JsonName("port")]
        public int FileViewerPort
        {
            get;
            set;
        }
    }
}
