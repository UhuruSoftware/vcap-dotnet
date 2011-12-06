// -----------------------------------------------------------------------
// <copyright file="ReactorErrorEventArgs.cs" company="Uhuru Software">
// Copyright (c) 2011 Uhuru Software, Inc., All Rights Reserved
// </copyright>
// -----------------------------------------------------------------------

namespace Uhuru.NatsClient
{
    using System;

    /// <summary>
    /// EventArgs class used when the NATS client raises an error event.
    /// </summary>
    public class ReactorErrorEventArgs : EventArgs
    {
        private string message;

        /// <summary>
        /// Gets or sets the error message.
        /// </summary>
        public string Message
        {
            get { return message; }
            set { message = value; }
        }

        /// <summary>
        /// Public constructor. Initializes the message property.
        /// </summary>
        /// <param name="message">Error message to be set.</param>
        public ReactorErrorEventArgs(string message)
        {
            this.message = message;
        }
    }
}
