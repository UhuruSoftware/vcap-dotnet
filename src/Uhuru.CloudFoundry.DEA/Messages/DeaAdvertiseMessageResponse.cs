// -----------------------------------------------------------------------
// <copyright file="DeaAdvertiseMessageResponse.cs" company="Uhuru Software, Inc.">
// Copyright (c) 2011 Uhuru Software, Inc., All Rights Reserved
// </copyright>
// -----------------------------------------------------------------------

namespace Uhuru.CloudFoundry.DEA.Messages
{
    using Uhuru.Utilities;
    using Uhuru.Utilities.Json;

    /// <summary>
    /// This class is a representation of a DEA advertise message response.
    /// </summary>
    public class DeaAdvertiseMessageResponse : JsonConvertibleObject
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
        /// Gets or sets a list of runtimes supported by the DEA service.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "Suitable for this context."), 
        JsonName("runtimes")]
        public string[] Runtimes
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the ammount of memory available on the DEA.
        /// </summary>
        [JsonName("available_memory")]
        public long AvailableMemory
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets a value indicating whether the DEA only runs production apps.
        /// </summary>
        [JsonName("prod")]
        public bool OnlyProductionApps
        {
            get;
            set;
        }
    }
}
