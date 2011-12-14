// -----------------------------------------------------------------------
// <copyright file="CheckOrphanRequest.cs" company="Uhuru Software, Inc.">
// Copyright (c) 2011 Uhuru Software, Inc., All Rights Reserved
// </copyright>
// -----------------------------------------------------------------------

namespace Uhuru.CloudFoundry.ServiceBase
{
    using System.Collections.Generic;
    using Uhuru.Utilities;

    /// <summary>
    /// This class encapsulate a request message to check for oprhaned services.
    /// </summary>
    internal class CheckOrphanRequest : JsonConvertibleObject
    {
        /// <summary>
        /// Gets or sets the handles that contain information about which services to check.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "These are needed for JSON (de)serialization"), 
        JsonName("handles")]
        public Handle[] Handles
        {
            get;
            set;
        }
    }
}