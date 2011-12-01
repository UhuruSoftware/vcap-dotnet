using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json.Linq;
using System.Net;

namespace CloudFoundry.Net.HttpHelpers
{
    class RestJsonHelper
    {
        private HttpRequestHelper httpHelper = null;

        public RestJsonHelper(HttpRequestHelper httpHelper)
        {
            this.httpHelper = httpHelper;
        }

        public JObject GetObject(string url)
        {
            HttpResponse response = httpHelper.Get(url, "application/json");
            if (response.Status == HttpStatusCode.OK)
            {
                return JObject.Parse(response.Body);
            }
            else
            {
                return null;
            }
        }

        public JArray GetArray(string url)
        {
            HttpResponse response = httpHelper.Get(url, "application/json");
            if (response.Status == HttpStatusCode.OK)
            {
                return JArray.Parse(response.Body);
            }
            else
            {
                return new JArray();
            }
        }

        public HttpResponse Post(string url, string payload)
        {
            return httpHelper.Post(url, payload, "application/json");
        }

        public HttpResponse Put(string url, string payload)
        {
            return httpHelper.Put(url, payload, "application/json");
        }

    }
}
