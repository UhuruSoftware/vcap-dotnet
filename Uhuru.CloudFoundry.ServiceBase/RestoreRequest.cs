// -----------------------------------------------------------------------
// <copyright file="RestoreRequest.cs" company="Uhuru Software">
// Copyright (c) 2011 Uhuru Software, Inc., All Rights Reserved
// </copyright>
// -----------------------------------------------------------------------

namespace Uhuru.CloudFoundry.ServiceBase
{
    using System.Collections.Generic;
    using Uhuru.Utilities;

    /// <summary>
    /// This class encapsulates a request message to restore a service.
    /// </summary>
    internal class RestoreRequest : JsonConvertibleObject
    {
        /// <summary>
        /// Gets or sets the instance id of the service that is to be restored.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "These are needed for JSON (de)serialization"), 
        JsonName("instance_id")]
        public string InstanceId
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the backup path to the resource that will be used to restore the service instance.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "These are needed for JSON (de)serialization"), 
        JsonName("backup_path")]
        public string BackupPath
        {
            get;
            set;
        }
    }
}