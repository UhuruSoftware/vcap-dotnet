using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.Net;
//using Cassini;
using System.IO;
using Uhuru.Utilities;
using CFNet=CloudFoundry.Net;
using Uhuru.CloudFoundry.DEA.Configuration;
using System.Threading;

namespace Uhuru.CloudFoundry.DEA
{
    public class VcapComponent
    {
        

        protected DateTime StartedAt;

        protected string Uuid;
        protected string Type;
        protected string Index;
        protected Uri NatsUri;

        protected Dictionary<string, object> discover = new Dictionary<string, object>();


        protected string Host;
        protected int Port;
        protected string[] Authentication;


        public ReaderWriterLockSlim VarzLock = new ReaderWriterLockSlim();
        public Dictionary<string, object> Varz = new Dictionary<string, object>();
        public string Healthz;


        protected VcapReactor vcapReactor;

        public VcapComponent()
        {
            ConstructReactor();

            Uuid = Guid.NewGuid().ToString("N");

            //Initialize Index from config file

            if (Index != null)
            {
                Uuid = String.Format("{0}-{1}", Index, Uuid);
            }

            Host = Utils.GetLocalIpAddress(UhuruSection.GetSection().DEA.LocalRoute);
            vcapReactor.Uri = new Uri(UhuruSection.GetSection().DEA.MessageBus);

            //http server port
            Port = Utils.GetEphemeralPort();

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
              {"host", String.Format("{0}:{1}", Host, Port)},
              {"credentials", Authentication},
              {"start", Utils.DateTimeToRubyString(StartedAt = DateTime.Now)}
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
                vcapReactor.SendReply(reply, discover.ToJson());
            };

            StartHttpServer();

            // Also announce ourselves on startup..
            vcapReactor.SendVcapComponentAnnounce(discover.ToJson()); 
        }


        private void UpdateDiscoverUptime()
        {
            TimeSpan span = DateTime.Now - StartedAt;
            discover["uptime"] = String.Format("{0}d:{1}h:{2}m:{3}s", span.Days, span.Hours, span.Minutes, span.Seconds);
        }

        private void StartHttpServer()
        {
            //TODO: vladi: port this again, this will most likely not work
            //Cassini.Server http_server = new Cassini.Server(Port, Host, "");

            File.WriteAllText("healthz", Healthz);
            File.WriteAllText("varz", Varz.ToJson());

            //http_server.Start();
        }


    }
}
