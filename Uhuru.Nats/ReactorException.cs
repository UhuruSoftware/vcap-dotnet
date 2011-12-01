using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Globalization;
using System.Runtime.Serialization;

namespace Uhuru.NatsClient
{
    [Serializable]
    public class ReactorException : Exception
    {
       
        public ReactorException(string message, Exception exception) : base (message,exception)
        {
        }

        public ReactorException(string message) : base (message)
        {
        }

        public ReactorException(Uri uri, string message) :
            base(uri == null ? null : String.Format(CultureInfo.InvariantCulture,"{0}\r\n\r\nConnected to {1}:{2}", 
            message, uri.Host, uri.Port))
        {
            
        }
        protected ReactorException(SerializationInfo serializationInfo, StreamingContext streamingContext)
            : base(serializationInfo, streamingContext)
        {
            
        }

        public ReactorException()
        {
        }
    }
}
