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

        protected string Type
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
        
        protected Dictionary<string, object> discover = new Dictionary<string, object>();

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

        public ReaderWriterLockSlim VarzLock = new ReaderWriterLockSlim();
        public Dictionary<string, object> Varz = new Dictionary<string, object>();

        public string Healthz
        {
            get;
            set;
        }

        protected VcapReactor vcapReactor
        {
            get;
            set;
        }

        public VcapComponent()
        {
            ConstructReactor();

            Uuid = Guid.NewGuid().ToString("N");

            //Initialize Index from config file

            if (Index != null)
            {
                Uuid = String.Format("{0}-{1}", Index, Uuid);
            }


            Host = NetworkInterface.GetLocalIPAddress(UhuruSection.GetSection().DEA.LocalRoute);
            vcapReactor.Uri = new Uri(UhuruSection.GetSection().DEA.MessageBus);

            //http server port
            Port = NetworkInterface.GrabEphemeralPort();

            Authentication = new string[]{ "", "" };
        }

        protected virtual void ConstructReactor()
        {
            if (vcapReactor == null)
            {
                vcapReactor = new VcapReactor();
            }
        }

        public virtual void Run()
        {
            vcapReactor.Start();

            discover = new Dictionary<string, object>() {
              {"type", Type},
              {"index", Index},
              {"uuid", Uuid},
              {"host", String.Format(CultureInfo.InvariantCulture, "{0}:{1}", Host, Port)},
              {"credentials", Authentication},
              {"start", RubyCompatibility.DateTimeToRubyString(StartedAt = DateTime.Now)}
            };
            
            // Varz is customizable
            Varz = new Dictionary<string, object>();
            foreach (string key in discover.Keys)
            {
                Varz[key] = discover[key];
            }

            Varz["num_cores"] = Environment.ProcessorCount;

            Healthz = "ok\n";

            // Listen for discovery requests
            vcapReactor.OnComponentDiscover += delegate(string msg, string reply, string subject)
            {
                UpdateDiscoverUptime();
                vcapReactor.SendReply(reply, JsonConvertibleObject.SerializeToJson(discover));
            };

            StartHttpServer();

            // Also announce ourselves on startup..
            vcapReactor.SendVcapComponentAnnounce(JsonConvertibleObject.SerializeToJson(discover)); 
        }

        private void UpdateDiscoverUptime()
        {
            TimeSpan span = DateTime.Now - StartedAt;
            discover["uptime"] = String.Format(CultureInfo.InvariantCulture, Strings.DaysHoursMinutesSecondsDateTimeFormat, span.Days, span.Hours, span.Minutes, span.Seconds);
        }

        private void StartHttpServer()
        {
            //TODO: vladi: port this again, this will most likely not work
            //Cassini.Server http_server = new Cassini.Server(Port, Host, "");

            File.WriteAllText("healthz", Healthz);
            File.WriteAllText("varz", JsonConvertibleObject.SerializeToJson(Varz));

            //http_server.Start();
        }
    }
}
