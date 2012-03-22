// -----------------------------------------------------------------------
// <copyright file="VcapComponent.cs" company="Uhuru Software, Inc.">
// Copyright (c) 2011 Uhuru Software, Inc., All Rights Reserved
// </copyright>
// -----------------------------------------------------------------------

namespace Uhuru.CloudFoundry.ServiceBase
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Globalization;
    using Uhuru.NatsClient;
    using Uhuru.Utilities;
    using Uhuru.Utilities.Json;

    /// <summary>
    /// This is a class used to register vcap components to the Cloud Foundry controller.
    /// </summary>
    public class VCAPComponent : IDisposable
    {
        /// <summary>
        /// Discover information for the node.
        /// </summary>
        private Dictionary<string, object> discover = new Dictionary<string, object>();

        /// <summary>
        /// The http monitoring server that server varz and healthz information.
        /// </summary>
        private MonitoringServer httpMonitoringServer;

        /// <summary>
        /// Gets or sets the varz information for the component.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly", Justification = "Outside objects need to be able to set a new instance of this property")]
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
        public string UUID
        {
            get
            {
                return this.discover["uuid"].ToString();
            }
        }

        /// <summary>
        /// Gets or sets the cpu performance.
        /// </summary>
        /// <value>
        /// The cpu performance.
        /// </value>
        protected PerformanceCounter CpuPerformance
        {
            get;
            set;
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

            int port = options.ContainsKey("statusPort") ? (int)options["statusPort"] : 0;
            if (port < 1)
            {
                port = NetworkInterface.GrabEphemeralPort();
            }

            Reactor nats = (Reactor)options["nats"];

            //// string[] auth = new string[] 
            //// { 
            ////     options.ContainsKey("user") ? options["user"].ToString() : string.Empty, 
            ////     options.ContainsKey("password") ? options["password"].ToString() : string.Empty
            //// };

            string[] auth = new string[] { Credentials.GenerateCredential(32), Credentials.GenerateCredential(32) };

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

            // todo: change this to a more accurate method
            // consider:
            // PerformanceCounter upTime = new PerformanceCounter("System", "System Up Time");
            // upTime.NextValue();
            // TimeSpan ts2 = TimeSpan.FromSeconds(upTime.NextValue());
            // Console.WriteLine("{0}d {1}h {2}m {3}s", ts2.Days, ts2.Hours, ts2.Minutes, ts2.Seconds);
            this.Varz["system_start"] = RubyCompatibility.DateTimeToRubyString(DateTime.Now.AddMilliseconds(-Environment.TickCount));

            this.CpuPerformance = new PerformanceCounter();
            this.CpuPerformance.CategoryName = "Processor Information";
            this.CpuPerformance.CounterName = "% Processor Time";
            this.CpuPerformance.InstanceName = "_Total";
            this.CpuPerformance.NextValue();

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

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources
        /// </summary>
        /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (this.httpMonitoringServer != null)
                {
                    this.httpMonitoringServer.Stop();
                    this.httpMonitoringServer.Dispose();
                }
            }
        }

        /// <summary>
        /// Updates the discovery uptime.
        /// </summary>
        private void UpdateDiscoverUptime()
        {
            TimeSpan span = DateTime.Now - RubyCompatibility.DateTimeFromRubyString((string)this.discover["start"]);
            this.discover["uptime"] = string.Format(CultureInfo.InvariantCulture, "{0}d:{1}h:{2}m:{3}s", span.Days, span.Hours, span.Minutes, span.Seconds);
        }

        /// <summary>
        /// Updates the varz structure with uptime, cpu, memory usage ....
        /// </summary>
        private void UpdateVarz()
        {
            TimeSpan span = DateTime.Now - RubyCompatibility.DateTimeFromRubyString((string)this.discover["start"]);
            this.Varz["uptime"] = string.Format(CultureInfo.InvariantCulture, "{0}d:{1}h:{2}m:{3}s", span.Days, span.Hours, span.Minutes, span.Seconds);

            float cpu = ((float)Process.GetCurrentProcess().TotalProcessorTime.Ticks / span.Ticks) * 100;

            // trim it to one decimal precision
            cpu = float.Parse(cpu.ToString("F1", CultureInfo.CurrentCulture), CultureInfo.CurrentCulture);

            this.Varz["cpu"] = cpu;
            this.Varz["mem"] = Process.GetCurrentProcess().WorkingSet64 / 1024;

            // extra uhuru information
            this.Varz["cpu_time"] = Process.GetCurrentProcess().TotalProcessorTime;

            // this is the cpu percentage for the time span between the Nextvalue calls;
            this.Varz["system_cpu"] = this.CpuPerformance.NextValue();
            this.Varz["system_cpu_ticks"] = this.CpuPerformance.RawValue;

            // todo: add memory usage here
            // consider:
            // PerformanceCounter ramCounter;
            // ramCounter = new PerformanceCounter("Memory", "Available MBytes");
            // ramCounter.NextValue();
            this.Varz["system_mem"] = null;
        }

        /// <summary>
        /// Starts the HTTP server used for monitoring.
        /// </summary>
        /// <param name="host">The host that publishes the monitoring server.</param>
        /// <param name="port">The port on which the server will listen.</param>
        /// <param name="auth">The user and password used for basic http authentication.</param>
        private void StartHttpServer(string host, int port, string[] auth)
        {
            // TODO: vladi: port this again, this will most likely not work
            this.httpMonitoringServer = new MonitoringServer(port, host, auth[0], auth[1]);
            this.httpMonitoringServer.HealthzRequested += new EventHandler<HealthzRequestEventArgs>(this.HttpMonitoringServer_HealthzRequested);
            this.httpMonitoringServer.VarzRequested += new EventHandler<VarzRequestEventArgs>(this.HttpMonitoringServer_VarzRequested);
            this.httpMonitoringServer.Start();
        }

        /// <summary>
        /// Handles the VarzRequested event of the httpMonitoringServer control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="Uhuru.Utilities.VarzRequestEventArgs"/> instance containing the event data.</param>
        private void HttpMonitoringServer_VarzRequested(object sender, VarzRequestEventArgs e)
        {
            this.UpdateVarz();
            e.VarzMessage = JsonConvertibleObject.SerializeToJson(this.Varz);
        }

        /// <summary>
        /// Handles the HealthzRequested event of the HttpMonitoringServer control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="Uhuru.Utilities.HealthzRequestEventArgs"/> instance containing the event data.</param>
        private void HttpMonitoringServer_HealthzRequested(object sender, HealthzRequestEventArgs e)
        {
            e.HealthzMessage = this.Healthz;
        }
    }
}