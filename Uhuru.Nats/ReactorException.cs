using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Globalization;
using System.Runtime.Serialization;

namespace Uhuru.NatsClient
{
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
        /// Public constructor.
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
        /// Public constructor.
        /// </summary>
        public ReactorException()
        {
        }
    }
}
