using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Globalization;

namespace CloudFoundry.Net.Nats.Exceptions
{
    public class NatsException : Exception
    {

        public NatsException(Client client, string message):
            base(String.Format(CultureInfo.InvariantCulture,
                "{0}\r\n\r\nConnected to {1}:{2}", message, client.URI.Host, client.URI.Port))
        {
            
        }
    }
}
