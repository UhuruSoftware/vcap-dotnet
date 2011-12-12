// -----------------------------------------------------------------------
// <copyright file="HealthzRequestEventArgs.cs" company="Uhuru Software, Inc.">
// Copyright (c) 2011 Uhuru Software, Inc., All Rights Reserved
// </copyright>
// -----------------------------------------------------------------------

namespace Uhuru.Utilities
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    /// <summary>
    /// This is an EventArgs class used by the Healthz and Varz server.
    /// When the server needs healthz information, it raises an event.
    /// The subscriber to that event sets the message of these args.
    /// </summary>
    public class HealthzRequestEventArgs : EventArgs
    {
        /// <summary>
        /// The health JSON message
        /// </summary>
        private string healthzMessage;

        /// <summary>
        /// Gets or sets the healthz message that will be served by the server.
        /// </summary>
        public string HealthzMessage
        {
            get { return this.healthzMessage; }
            set { this.healthzMessage = value; }
        }
    }
}
