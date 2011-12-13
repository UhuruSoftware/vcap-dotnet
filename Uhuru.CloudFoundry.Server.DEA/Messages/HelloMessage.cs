// -----------------------------------------------------------------------
// <copyright file="HelloMessage.cs" company="Uhuru Software, Inc.">
// Copyright (c) 2011 Uhuru Software, Inc., All Rights Reserved
// </copyright>
// -----------------------------------------------------------------------

namespace Uhuru.CloudFoundry.DEA
{
    using Uhuru.Utilities;

    /// <summary>
    /// This class encapsulates a hello message.
    /// </summary>
    class HelloMessage : JsonConvertibleObject
    {
        /// <summary>
        /// Gets or sets the id of the DEA service.
        /// </summary>
        [JsonName("id")]
        public string Id
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the version of the service.
        /// </summary>
        [JsonName("version")]
        public decimal Version
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the host on which the service runs.
        /// </summary>
        [JsonName("ip")]
        public string Host
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the file viewer port for the DEA service.
        /// </summary>
        [JsonName("port")]
        public int FileViewerPort
        {
            get;
            set;
        }
    }
}
