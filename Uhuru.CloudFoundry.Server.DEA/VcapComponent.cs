using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.Net;
using System.IO;
using Uhuru.Utilities;
using CFNet=CloudFoundry.Net;

namespace Uhuru.CloudFoundry.Server.DEA
{
    class VcapComponent
    {
        Dictionary<string, object> discover = new Dictionary<string, object>();
        public Dictionary<string, object> varz = new Dictionary<string, object>();
        private string healthz;

        public string Uuid
        {
            get
            {
                return discover["uuid"].ToString();
            }
        }

        public void Register(Dictionary<string, object> opts, CFNet.Nats.Client nats)
        {
            string uuid = Guid.NewGuid().ToString("N");
            object type = opts["type"];
            object index = opts.ContainsKey("index") ? opts["index"] : null;

            if (index != null)
            {
                uuid = String.Format("{0}-{1}", index, uuid);
            }

            string host = opts["host"].ToString();
            int port = opts.ContainsKey("port") ? Convert.ToInt32(opts["port"]) : 0;

            //Nats.Client nats = (Nats.Client)opts["nats"];

            string[] auth = new string[] { opts["user"].ToString(), opts["password"].ToString() };

            // Discover message limited
            discover = new Dictionary<string, object>() {
              {"type", type},
              {"index", index},
              {"uuid", uuid},
              {"host", String.Format("{0}:{1}", host, port)},
              {"credentials", auth},
              {"start", Utils.DateTimeToRubyString(DateTime.Now)}
            };

            // Varz is customizable
            varz = new Dictionary<string, object>();
            foreach (string key in discover.Keys)
            {
                varz[key] = discover[key];
            }

            varz["num_cores"] = Environment.ProcessorCount;

            //TODO: vladi: make sure this is not required by anyone else
            //@varz[:config] = sanitize_config(opts[:config]) if opts[:config]

            healthz = "ok\n";

            start_http_server(host, port, auth);

            // Listen for discovery requests
            nats.Subscribe("vcap.component.discover", delegate(string msg, string reply, string subject)
            {
                update_discover_uptime();
                nats.Publish(reply, msg: discover.ToJson());
            });

            // Also announce ourselves on startup..
            nats.Publish("vcap.component.announce", msg: discover.ToJson());
        }

        private void update_discover_uptime()
        {
            TimeSpan span = DateTime.Now - (DateTime)discover["start"];
            discover["uptime"] = String.Format("{0}d:{1}h:{2}m:{3}s", span.Days, span.Hours, span.Minutes, span.Seconds);
        }

        private void start_http_server(string host, int port, string[] auth)
        {
            //TODO: vladi: port this again, this will most likely not work
            MonitoringServer http_server = new MonitoringServer(port, host, auth[0], auth[1]);

            http_server.Start();
        }


    }
}
