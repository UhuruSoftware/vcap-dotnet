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
        public ReaderWriterLockSlim VarzLock { get; set; }

        public Dictionary<string, object> Varz { get; set; }

        protected Dictionary<string, object> Discover { get; set; }

        protected DateTime StartedAt
        {
            get;
            set;
        }

        protected string Uuid
        {
            get;
            set;
        }

        protected string ComponentType
        {
            get;
            set;
        }

        protected string Index
        {
            get;
            set;
        }

        protected Uri NatsUri
        {
            get;
            set;
        }
        
        protected string Host
        {
            get;
            set;
        }

        protected int Port
        {
            get;
            set;
        }

        protected string[] Authentication
        {
            get;
            set;
        }

        public string Healthz
        {
            get;
            set;
        }

        protected VcapReactor VcapReactor
        {
            get;
            set;
        }

        protected MonitoringServer MonitoringServer
        {
            get;
            set;
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

        protected virtual void ConstructReactor()
        {
            if (VcapReactor == null)
            {
                VcapReactor = new VcapReactor();
            }
        }

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

        private void UpdateDiscoverUptime()
        {
            TimeSpan span = DateTime.Now - this.StartedAt;
            this.Discover["uptime"] = string.Format(CultureInfo.InvariantCulture, Strings.DaysHoursMinutesSecondsDateTimeFormat, span.Days, span.Hours, span.Minutes, span.Seconds);
        }

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

        ~VcapComponent()
        {
            MonitoringServer.Stop();
        }
    }
}
