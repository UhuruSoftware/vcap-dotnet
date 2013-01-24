// -----------------------------------------------------------------------
// <copyright file="DeaAdvertiseMessage.cs" company="Uhuru Software, Inc.">
// Copyright (c) 2011 Uhuru Software, Inc., All Rights Reserved
// </copyright>
// -----------------------------------------------------------------------

namespace Uhuru.CloudFoundry.DEA.Messages
{
    using System.Collections.Generic;
    using Uhuru.Utilities;
    using Uhuru.Utilities.Json;

    /// <summary>
    /// This class is a representation of a DEA status message response.
    /// </summary>
    public class DeaAdvertiseMessage : JsonConvertibleObject
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
        /// Gets or sets the supported runtimes.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1002:DoNotExposeGenericLists", Justification = "Convention."),
        System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly", Justification = "Convention.")]
        [JsonName("runtimes")]
        public List<string> Runtimes
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the current available memory in MiB.
        /// </summary>
        [JsonName("available_memory")]
        public long AvailableMemory
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets a value indicating whether it accepts only production apps.
        /// </summary>
        [JsonName("prod")]
        public bool Prod
        {
            get;
            set;
        }
    }
}
