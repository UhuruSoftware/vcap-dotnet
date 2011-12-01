using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;

namespace CloudFoundry.Net.HttpHelpers
{
    class HttpResponse
    {
        public HttpStatusCode Status;
        public string Body = String.Empty;
        public WebHeaderCollection Headers = null;
    }
}
