using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Net.Sockets;
using System.Net;
using Uhuru.Utilities;
using Uhuru.NatsClient;
using System.Globalization;

namespace Uhuru.CloudFoundry.ServiceBase
{
    /// <summary>
    /// This is a class used to register vcap components to the Cloud Foundry controller.
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Vcap")]
    public class VcapComponent
    {
        Dictionary<string, object> discover = new Dictionary<string, object>();
        private MonitoringServer httpMonitoringServer;

        /// <summary>
        /// Gets or sets the varz information for the component.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public Dictionary<string, object> Varz
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the healthz information for the component.
        /// </summary>
        public string Healthz
        {
            get;
            set;
        }

        /// <summary>
        /// Gets the UUID of the component.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Uuid")]
        public string Uuid
        {
            get
            {
                return discover["uuid"].ToString();
            }
        }

        /// <summary>
        /// Registers a component using the specified options.
        /// </summary>
        /// <param name="options">The options for the component.</param>
        public void Register(Dictionary<string, object> options)
        {
            if (options == null)
            {
                throw new ArgumentNullException("options");
            }
            Varz = new Dictionary<string, object>();
            string uuid = Guid.NewGuid().ToString("N");
            object type = options["type"];
            object index = options.ContainsKey("index") ? options["index"] : null;

            if (index != null)
            {
                uuid = String.Format(CultureInfo.InvariantCulture, "{0}-{1}", index, uuid);
            }

            string host = options["host"].ToString();
            int port = NetworkInterface.GrabEphemeralPort();

            Reactor nats = (Reactor)options["nats"];

            string[] auth = new string[] 
            { 
                options.ContainsKey("user") ? options["user"].ToString() :String.Empty, 
                options.ContainsKey("password") ? options["password"].ToString() : String.Empty
            };

            // Discover message limited
            discover = new Dictionary<string, object>() {
              {"type", type},
              {"index", index},
              {"uuid", uuid},
              {"host", String.Format(CultureInfo.InvariantCulture, "{0}:{1}", host, port)},
              {"credentials", auth},
              {"start", RubyCompatibility.DateTimeToRubyString(DateTime.Now)}
            };

            // Varz is customizable
            Varz = new Dictionary<string, object>();
            foreach (string key in discover.Keys)
            {
                Varz[key] = discover[key];
            }

            Varz["num_cores"] = Environment.ProcessorCount;

            //TODO: vladi: make sure this is not required by anyone else
            //@varz[:config] = sanitize_config(opts[:config]) if opts[:config]

            Healthz = "ok\n";

            start_http_server(host, port, auth);

            // Listen for discovery requests
            nats.Subscribe("vcap.component.discover", delegate(string msg, string reply, string subject)
            {
                update_discover_uptime();
                nats.Publish(reply, null, discover.ToJson());
            });

            // Also announce ourselves on startup..
            nats.Publish("vcap.component.announce", null, msg: discover.ToJson());
        }

        private void update_discover_uptime()
        {
            TimeSpan span = DateTime.Now - (DateTime)discover["start"];
            discover["uptime"] = String.Format(CultureInfo.InvariantCulture, "{0}d:{1}h:{2}m:{3}s", span.Days, span.Hours, span.Minutes, span.Seconds);
        }

        private void start_http_server(string host, int port, string[] auth)
        {
            //TODO: vladi: port this again, this will most likely not work
            httpMonitoringServer = new MonitoringServer(port, host, auth[0], auth[1]);
            httpMonitoringServer.HealthzRequested += new EventHandler<HealthzRequestEventArgs>(httpMonitoringServer_HealthzRequested);
            httpMonitoringServer.Start();
        }

        void httpMonitoringServer_HealthzRequested(object sender, HealthzRequestEventArgs e)
        {
            e.HealthzMessage = Healthz;
        }
    }
}