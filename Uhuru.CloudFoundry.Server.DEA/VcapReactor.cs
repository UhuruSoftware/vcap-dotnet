using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Uhuru.NatsClient;


namespace Uhuru.CloudFoundry.DEA
{
    public class VcapReactor
    {
        public event SubscribeCallback OnComponentDiscover;

        public event EventHandler<ReactorErrorEventArgs> OnNatsError;

        public Reactor NatsClient
        {
            get;
            set;
        }

        public Uri Uri
        {
            get
            {
                return NatsClient.ServerUri;
            }
        }

        public VcapReactor()
        {
            NatsClient = new Reactor();
            NatsClient.OnError += OnNatsError;
        }

        public virtual void Start()
        {
            NatsClient.Start(Uri);

            NatsClient.Subscribe("vcap.component.discover", OnComponentDiscover);
        }

        public void SendVcapComponentAnnounce(string message)
        {
            NatsClient.Publish("vcap.component.announce", null, message);
        }

        public void SendReply(string reply, string message)
        {
            NatsClient.Publish(reply, msg: message);
        }



    }
}
