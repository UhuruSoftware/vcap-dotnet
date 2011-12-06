// -----------------------------------------------------------------------
// <copyright file="ReactorException.cs" company="Uhuru Software">
// Copyright (c) 2011 Uhuru Software, Inc., All Rights Reserved
// </copyright>
// -----------------------------------------------------------------------

namespace Uhuru.NatsClient
{
    using System;
    using System.Globalization;
    using System.Runtime.Serialization;
    
    /// <summary>
    /// This is an exception type that can be raised by the NATS client reactor.
    /// </summary>
    [Serializable]
    public class ReactorException : Exception
    {
        /// <summary>
        /// Public constructor inherited from the Exception class.
        /// </summary>
        /// <param name="message">Exception message.</param>
        /// <param name="exception">Inner exception</param>
        public ReactorException(string message, Exception exception)
            : base(message, exception)
        {
        }

        /// <summary>
        /// Public constructor inherited from the Exception class.
        /// </summary>
        /// <param name="message">Exception message.</param>
        public ReactorException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the ReactorException class.
        /// </summary>
        /// <param name="uri">The uri the NATS client is connected to.</param>
        /// <param name="message">Exception message.</param>
        public ReactorException(Uri uri, string message) :
            base(uri == null ? null : String.Format(CultureInfo.InvariantCulture, "{0}\r\n\r\nConnected to {1}:{2}",
            message, uri.Host, uri.Port))
        {

        }

        /// <summary>
        /// Constructor required by the [Serializable] attribute.
        /// </summary>
        /// <param name="serializationInfo">Serialization info.</param>
        /// <param name="streamingContext">Streaming context.</param>
        protected ReactorException(SerializationInfo serializationInfo, StreamingContext streamingContext)
            : base(serializationInfo, streamingContext)
        {
        }

        /// <summary>
        /// Initializes a new instance of the ReactorException class.
        /// </summary>
        public ReactorException()
        {
        }
    }
}
