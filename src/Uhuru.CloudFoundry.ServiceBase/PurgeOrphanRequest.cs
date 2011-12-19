// -----------------------------------------------------------------------
// <copyright file="PurgeOrphanRequest.cs" company="Uhuru Software, Inc.">
// Copyright (c) 2011 Uhuru Software, Inc., All Rights Reserved
// </copyright>
// -----------------------------------------------------------------------

namespace Uhuru.CloudFoundry.ServiceBase
{
    using System.Collections.Generic;
    using Uhuru.Utilities;

    /// <summary>
    /// This class encapsulates a request message to purge orphaned services.
    /// </summary>
    internal class PurgeOrphanRequest : JsonConvertibleObject
    {
        /// <summary>
        /// Gets or sets a list of orphan instances names.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "These are needed for JSON (de)serialization"), 
        JsonName("orphan_ins_list")]
        public string[] OrphanInsList
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets a list of orphan bindings credentials.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "These are needed for JSON (de)serialization"), 
        JsonName("orphan_binding_list")]
        public ServiceCredentials[] OrphanBindingList
        {
            get;
            set;
        }
    }
}