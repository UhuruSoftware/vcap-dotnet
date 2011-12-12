// -----------------------------------------------------------------------
// <copyright file="VarzRequestEventArgs.cs" company="Uhuru Software, Inc.">
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
    /// When the server needs varz information, it raises an event.
    /// The subscriber to that event sets the message of these args.
    /// </summary>
    public class VarzRequestEventArgs : EventArgs
    {
        /// <summary>
        /// The varz message.
        /// </summary>
        private string varzMessage;

        /// <summary>
        /// Gets or sets the varz message that will be served by the server.
        /// </summary>
        public string VarzMessage
        {
            get { return this.varzMessage; }
            set { this.varzMessage = value; }
        }
    }
}
