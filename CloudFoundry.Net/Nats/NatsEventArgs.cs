using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CloudFoundry.Net.Nats
{
    public class NatsEventArgs:EventArgs
    {
        private string message;

        public string Message
        {
            get { return message; }
            set { message = value; }
        }

        public NatsEventArgs(string message)
        {
            this.message = message;
        }

    }
}
