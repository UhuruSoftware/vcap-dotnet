// -----------------------------------------------------------------------
// <copyright file="CheckOrphanResponse.cs" company="Uhuru Software, Inc.">
// Copyright (c) 2011 Uhuru Software, Inc., All Rights Reserved
// </copyright>
// -----------------------------------------------------------------------

namespace Uhuru.CloudFoundry.ServiceBase
{
    using System.Collections.Generic;
    using Uhuru.Utilities;

    /// <summary>
    /// This clas encapsulates a response message to a <see cref="CheckOrphanRequest"/>
    /// </summary>
    internal class CheckOrphanResponse : MessageWithSuccessStatus
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

        /// <summary>
        /// Gets or sets a hash for orphan instances;
        /// Key: the id of the node with orphans
        /// Value: orphan instances list
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "These are needed for JSON (de)serialization"), 
        JsonName("orphan_instances")]
        public Dictionary<string, object> OrphanInstances
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets a hash for orphan bindings;
        /// Key: the id of the node with orphans
        /// Value: orphan bindings list
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "These are needed for JSON (de)serialization"), 
        JsonName("orphan_bindings")]
        public Dictionary<string, object> OrphanBindings
        {
            get;
            set;
        }
    }
}