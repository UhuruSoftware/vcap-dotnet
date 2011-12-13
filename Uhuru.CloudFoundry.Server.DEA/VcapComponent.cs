// -----------------------------------------------------------------------
// <copyright file="VcapComponent.cs" company="Uhuru Software">
// Copyright (c) 2011 Uhuru Software, Inc., All Rights Reserved
// </copyright>
// -----------------------------------------------------------------------

namespace Uhuru.CloudFoundry.DEA
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using System.Threading;
    using Uhuru.Configuration;
    using Uhuru.Utilities;
    
    public class VcapComponent
    {
        /// <summary>
        /// Gets or sets the varz lock for the varz property.
        /// </summary>
        public ReaderWriterLockSlim VarzLock { get; set; }

        /// <summary>
        /// Gets or sets the varz value to be published.
        /// </summary>
        public Dictionary<string, object> Varz { get; set; }

        /// <summary>
        /// Gets or sets the discover.
        /// </summary>
        protected Dictionary<string, object> Discover { get; set; }

        /// <summary>
        /// Gets or sets the timestamp the component started.
        /// </summary>
        protected DateTime StartedAt
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the UUID of the VCAP component.
        /// </summary>
        protected string Uuid
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the type of the VCAP component (DEA, CloudControlser, Node, Healthmanager, etc.).
        /// </summary>
        protected string ComponentType
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the index.
        /// </summary>
        protected string Index
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the NATS server URI.
        /// </summary>
        protected Uri NatsUri
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the host address of the VCAP component to be announced.
        /// </summary>
        protected string Host
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the monitoring server port.
        /// </summary>
        protected int Port
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the monitoring server authentication.
        /// </summary>
        protected string[] Authentication
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the healthz value to be published.
        /// </summary>
        public string Healthz
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the VCAP reactor.
        /// </summary
        protected VcapReactor VcapReactor
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the monitoring server used for healthz and varz.
        /// </summary>
        protected MonitoringServer MonitoringServer
        {
            get;
            set;
        }

        /// <summary>
        /// Constructs the reactor for the VCAP component.
        /// </summary>
        protected virtual void ConstructReactor()
        {
            if (VcapReactor == null)
            {
                VcapReactor = new VcapReactor();
            }
        }

        /// <summary>
        /// Initializes a new instance of the VcapComponent class.
        /// </summary>
        public VcapComponent()
        {
            this.VarzLock = new ReaderWriterLockSlim();
            this.Varz = new Dictionary<string, object>();
            this.Discover = new Dictionary<string, object>();

            this.ConstructReactor();

            this.Uuid = Guid.NewGuid().ToString("N");

            // Initialize Index from config file
            if (this.Index != null)
            {
                this.Uuid = string.Format(CultureInfo.InvariantCulture, "{0}-{1}", this.Index, this.Uuid);
            }

            this.Host = NetworkInterface.GetLocalIPAddress(UhuruSection.GetSection().DEA.LocalRoute);
            VcapReactor.Uri = new Uri(UhuruSection.GetSection().DEA.MessageBus);

            // http server port
            this.Port = NetworkInterface.GrabEphemeralPort();

            this.Authentication = new string[] { Credentials.GenerateCredential(), Credentials.GenerateCredential() };
        }

        /// <summary>
        /// Runs this the VCAP component. This method is non-blocking.
        /// </summary>
        public virtual void Run()
        {
            VcapReactor.Start();

            this.Discover = new Dictionary<string, object>() 
            {
              { "type", this.ComponentType },
              { "index", this.Index },
              { "uuid", this.Uuid },
              { "host", string.Format(CultureInfo.InvariantCulture, "{0}:{1}", this.Host, this.Port) },
              { "credentials", this.Authentication },
              { "start", RubyCompatibility.DateTimeToRubyString(this.StartedAt = DateTime.Now) }
            };
            
            // Varz is customizable
            this.Varz = new Dictionary<string, object>();
            foreach (string key in this.Discover.Keys)
            {
                this.Varz[key] = this.Discover[key];
            }

            this.Varz["num_cores"] = Environment.ProcessorCount;

            this.Healthz = "ok\n";

            // Listen for discovery requests
            VcapReactor.OnComponentDiscover += delegate(string msg, string reply, string subject)
            {
                this.UpdateDiscoverUptime();
                VcapReactor.SendReply(reply, JsonConvertibleObject.SerializeToJson(this.Discover));
            };

            this.StartHttpServer();

            // Also announce ourselves on startup..
            VcapReactor.SendVcapComponentAnnounce(JsonConvertibleObject.SerializeToJson(this.Discover)); 
        }

        /// <summary>
        /// Updates the discover uptime.
        /// </summary>
        private void UpdateDiscoverUptime()
        {
            TimeSpan span = DateTime.Now - this.StartedAt;
            this.Discover["uptime"] = string.Format(CultureInfo.InvariantCulture, Strings.DaysHoursMinutesSecondsDateTimeFormat, span.Days, span.Hours, span.Minutes, span.Seconds);
        }

        /// <summary>
        /// Starts the HTTP server used for healthz and varz.
        /// </summary>
        private void StartHttpServer()
        {
            MonitoringServer = new MonitoringServer(this.Port, this.Host, this.Authentication[0], this.Authentication[1]);

            MonitoringServer.VarzRequested += delegate(object sender, VarzRequestEventArgs response)
            {
                try
                {
                    this.VarzLock.ExitWriteLock();
                    response.VarzMessage = JsonConvertibleObject.SerializeToJson(this.Varz);
                }
                finally
                {
                    this.VarzLock.ExitReadLock();
                }
            };

            MonitoringServer.HealthzRequested += delegate(object sender, HealthzRequestEventArgs response)
            {
                response.HealthzMessage = this.Healthz;
            };

            MonitoringServer.Start();
        }

        /// <summary>
        /// Releases unmanaged resources and performs other cleanup operations before the
        /// <see cref="VcapComponent"/> is reclaimed by garbage collection.
        /// </summary>
        ~VcapComponent()
        {
            MonitoringServer.Stop();
        }
    }
}
