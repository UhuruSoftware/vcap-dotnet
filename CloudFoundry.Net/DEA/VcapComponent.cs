using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cassini;
using System.IO;
using System.Net.Sockets;
using System.Net;
using Uhuru.Utilities;

namespace CloudFoundry.Net.DEA
{
    public class VcapComponent
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

        public void Register(Dictionary<string, object> opts)
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

            Nats.Client nats = (Nats.Client)opts["nats"];

            string[] auth = new string[] 
            { 
                opts.ContainsKey("user") ? opts["user"].ToString() :String.Empty, 
                opts.ContainsKey("password") ? opts["password"].ToString() : String.Empty
            };

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
            Server http_server = new Server(port, host, "");

            File.WriteAllText("healthz", healthz);
            File.WriteAllText("varz", varz.ToJson());

            http_server.Start();
        }

        //returns the ip used by the OS to connect to the RouteIPAddress. Pointing to a interface address will return that address
        public static string GetLocalIpAddress(string RouteIPAddress = "198.41.0.4")
        {
            UdpClient udpClient = new UdpClient();
            udpClient.Connect(RouteIPAddress, 1);
            IPEndPoint ep = (IPEndPoint)udpClient.Client.LocalEndPoint;
            udpClient.Close();
            return ep.Address.ToString();
        }

        public static int GetEphemeralPort()
        {
            TcpListener socket = new TcpListener(IPAddress.Any, 0);
            socket.Start();
            int port = ((IPEndPoint)socket.LocalEndpoint).Port;
            socket.Stop();
            return port;
        }

    }
}