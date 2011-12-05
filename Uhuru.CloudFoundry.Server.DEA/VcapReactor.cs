using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CFNet = CloudFoundry.Net;


namespace Uhuru.CloudFoundry.DEA
{
    public class VcapReactor
    {
        public event CFNet.Nats.SubscribeCallback OnComponentDiscover;

        public event EventHandler<CFNet.Nats.NatsEventArgs> OnNatsError;

        public CFNet.Nats.Client NatsClient
        {
            get;
            set;
        }

        public Uri Uri
        {
            get
            {
                return NatsClient.URI;
            }
            set
            {
                NatsClient.URI = value;
            }
        }

        public VcapReactor()
        {
            NatsClient = new CFNet.Nats.Client();
            NatsClient.OnError += OnNatsError;
        }

        public virtual void Start()
        {
            NatsClient.Start();

            NatsClient.Subscribe("vcap.component.discover", OnComponentDiscover);
        }

        public void SendVcapComponentAnnounce(string message)
        {
            NatsClient.Publish("vcap.component.announce", msg: message);
        }

        public void SendReply(string reply, string message)
        {
            NatsClient.Publish(reply, msg: message);
        }



    }
}
