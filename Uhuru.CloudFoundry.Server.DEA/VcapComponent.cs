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
            
            ConstructReactor();

            Uuid = Guid.NewGuid().ToString("N");

            // Initialize Index from config file
            if (Index != null)
            {
                Uuid = String.Format(CultureInfo.InvariantCulture, "{0}-{1}", Index, Uuid);
            }

            Host = NetworkInterface.GetLocalIPAddress(UhuruSection.GetSection().DEA.LocalRoute);
            VcapReactor.Uri = new Uri(UhuruSection.GetSection().DEA.MessageBus);

            // http server port
            Port = NetworkInterface.GrabEphemeralPort();

            Authentication = new string[] { Credentials.GenerateCredential(), Credentials.GenerateCredential() };
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

            Discover = new Dictionary<string, object>() {
              {"type", ComponentType},
              {"index", Index},
              {"uuid", Uuid},
              {"host", String.Format(CultureInfo.InvariantCulture, "{0}:{1}", Host, Port)},
              {"credentials", Authentication},
              {"start", RubyCompatibility.DateTimeToRubyString(StartedAt = DateTime.Now)}
            };
            
            // Varz is customizable
            Varz = new Dictionary<string, object>();
            foreach (string key in Discover.Keys)
            {
                Varz[key] = Discover[key];
            }

            Varz["num_cores"] = Environment.ProcessorCount;

            Healthz = "ok\n";

            // Listen for discovery requests
            VcapReactor.OnComponentDiscover += delegate(string msg, string reply, string subject)
            {
                UpdateDiscoverUptime();
                VcapReactor.SendReply(reply, JsonConvertibleObject.SerializeToJson(Discover));
            };

            StartHttpServer();

            // Also announce ourselves on startup..
            VcapReactor.SendVcapComponentAnnounce(JsonConvertibleObject.SerializeToJson(Discover)); 
        }

        private void UpdateDiscoverUptime()
        {
            TimeSpan span = DateTime.Now - StartedAt;
            Discover["uptime"] = String.Format(CultureInfo.InvariantCulture, Strings.DaysHoursMinutesSecondsDateTimeFormat, span.Days, span.Hours, span.Minutes, span.Seconds);
        }

        private void StartHttpServer()
        {
            MonitoringServer = new MonitoringServer(Port, Host, Authentication[0], Authentication[1]);

            MonitoringServer.VarzRequested += delegate(object sender, VarzRequestEventArgs response)
            {
                try
                {
                    VarzLock.ExitWriteLock();
                    response.VarzMessage = JsonConvertibleObject.SerializeToJson(Varz);
                }
                finally
                { 
                    VarzLock.ExitReadLock();
                }
            };

            MonitoringServer.HealthzRequested += delegate(object sender, HealthzRequestEventArgs response)
            {
                response.HealthzMessage = Healthz;
            };

            MonitoringServer.Start();
        }

        ~VcapComponent()
        {
            MonitoringServer.Stop();
        }
    }
}
