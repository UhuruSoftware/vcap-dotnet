// -----------------------------------------------------------------------
// <copyright file="BindRequest.cs" company="Uhuru Software">
// Copyright (c) 2011 Uhuru Software, Inc., All Rights Reserved
// </copyright>
// -----------------------------------------------------------------------

namespace Uhuru.CloudFoundry.ServiceBase
{
    using System.Collections.Generic;
    using Uhuru.Utilities;

    /// <summary>
    /// This class encapsulates a request message to bind a service to an app.
    /// </summary>
    internal class BindRequest : JsonConvertibleObject
    {
        /// <summary>
        /// Gets or sets the name of the service to be bound.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "These are used for (de)serialization"),
        JsonName("name")]
        public string Name
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the binding options.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "These are used for (de)serialization"),
        JsonName("bind_opts")]
        public Dictionary<string, object> BindOptions
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the credentials that will be used to bind the service to the app.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "These are used for (de)serialization"), 
        JsonName("credentials")]
        public ServiceCredentials Credentials
        {
            get;
            set;
        }
    }
}