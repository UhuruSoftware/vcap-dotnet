// -----------------------------------------------------------------------
// <copyright file="ReactorErrorEventArgs.cs" company="Uhuru Software, Inc.">
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
        /// <summary>
        /// Error message
        /// </summary>
        private string message;

        /// <summary>
        /// Error message
        /// </summary>
        private Exception exception;

        /// <summary>
        /// Initializes a new instance of the ReactorErrorEventArgs class.
        /// </summary>
        /// <param name="message">Error message to be set.</param>
        public ReactorErrorEventArgs(string message)
        {
            this.message = message;
        }

        /// <summary>
        /// Initializes a new instance of the ReactorErrorEventArgs class.
        /// </summary>
        /// <param name="message">Error message to be set.</param>
        /// <param name="ex">The exception describing the error.</param>
        public ReactorErrorEventArgs(string message, Exception ex)
        {
            this.message = message;
            this.exception = ex;
        }

        /// <summary>
        /// Gets or sets the error message.
        /// </summary>
        public string Message
        {
            get { return this.message; }
            set { this.message = value; }
        }

        /// <summary>
        /// Gets or sets the exception.
        /// </summary>
        /// <value>
        /// The exception.
        /// </value>
        public Exception Exception
        {
            get { return this.exception; }
            set { this.exception = value; }
        }
    }
}
