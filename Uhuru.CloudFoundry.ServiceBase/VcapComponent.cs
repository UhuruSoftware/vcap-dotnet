// -----------------------------------------------------------------------
// <copyright file="VcapComponent.cs" company="Uhuru Software">
// Copyright (c) 2011 Uhuru Software, Inc., All Rights Reserved
// </copyright>
// -----------------------------------------------------------------------

namespace Uhuru.CloudFoundry.ServiceBase
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using Uhuru.NatsClient;
    using Uhuru.Utilities;

    /// <summary>
    /// This is a class used to register vcap components to the Cloud Foundry controller.
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Vcap")]
    public class VcapComponent
    {
        private Dictionary<string, object> discover = new Dictionary<string, object>();
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
                return this.discover["uuid"].ToString();
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

            this.Varz = new Dictionary<string, object>();
            string uuid = Guid.NewGuid().ToString("N");
            object type = options["type"];
            object index = options.ContainsKey("index") ? options["index"] : null;

            if (index != null)
            {
                uuid = string.Format(CultureInfo.InvariantCulture, "{0}-{1}", index, uuid);
            }

            string host = options["host"].ToString();
            int port = NetworkInterface.GrabEphemeralPort();

            Reactor nats = (Reactor)options["nats"];

            string[] auth = new string[] 
            { 
                options.ContainsKey("user") ? options["user"].ToString() : string.Empty, 
                options.ContainsKey("password") ? options["password"].ToString() : string.Empty
            };

            // Discover message limited
            this.discover = new Dictionary<string, object>() 
            {
              { "type", type },
              { "index", index },
              { "uuid", uuid },
              { "host", string.Format(CultureInfo.InvariantCulture, "{0}:{1}", host, port) },
              { "credentials", auth },
              { "start", RubyCompatibility.DateTimeToRubyString(DateTime.Now) }
            };

            // Varz is customizable
            this.Varz = new Dictionary<string, object>();
            foreach (string key in this.discover.Keys)
            {
                this.Varz[key] = this.discover[key];
            }

            this.Varz["num_cores"] = Environment.ProcessorCount;

            this.Healthz = "ok\n";

            this.StartHttpServer(host, port, auth);

            // Listen for discovery requests
            nats.Subscribe(
                "vcap.component.discover", 
                delegate(string msg, string reply, string subject)
                {
                    this.UpdateDiscoverUptime();
                    nats.Publish(reply, null, JsonConvertibleObject.SerializeToJson(this.discover));
                });

            // Also announce ourselves on startup..
            nats.Publish("vcap.component.announce", null, msg: JsonConvertibleObject.SerializeToJson(this.discover));
        }

        private void UpdateDiscoverUptime()
        {
            TimeSpan span = DateTime.Now - (DateTime)this.discover["start"];
            this.discover["uptime"] = string.Format(CultureInfo.InvariantCulture, "{0}d:{1}h:{2}m:{3}s", span.Days, span.Hours, span.Minutes, span.Seconds);
        }

        private void StartHttpServer(string host, int port, string[] auth)
        {
            // TODO: vladi: port this again, this will most likely not work
            this.httpMonitoringServer = new MonitoringServer(port, host, auth[0], auth[1]);
            this.httpMonitoringServer.HealthzRequested += new EventHandler<HealthzRequestEventArgs>(this.HttpMonitoringServer_HealthzRequested);
            this.httpMonitoringServer.Start();
        }

        private void HttpMonitoringServer_HealthzRequested(object sender, HealthzRequestEventArgs e)
        {
            e.HealthzMessage = this.Healthz;
        }
    }
}