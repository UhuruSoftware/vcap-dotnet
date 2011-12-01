using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Uhuru.NatsClient
{
    public class ReactorEventArgs : EventArgs
    {
        private string message;

        public string Message
        {
            get { return message; }
            set { message = value; }
        }

        public ReactorEventArgs(string message)
        {
            this.message = message;
        }
    }
}
