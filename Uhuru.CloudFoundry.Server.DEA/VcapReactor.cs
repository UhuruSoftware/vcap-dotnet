using System;
using Uhuru.NatsClient;


namespace Uhuru.CloudFoundry.DEA
{
    public class VcapReactor
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1009:DeclareEventHandlersCorrectly")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1710:IdentifiersShouldHaveCorrectSuffix")]
        public event SubscribeCallback OnComponentDiscover;

        public event EventHandler<ReactorErrorEventArgs> OnNatsError;

        public Reactor NatsClient
        {
            get;
            set;
        }

        public Uri Uri
        {
            get;
            set;
        }

        public VcapReactor()
        {
            NatsClient = new Reactor();
            NatsClient.OnError += OnNatsError;
        }

        public virtual void Start()
        {
            NatsClient.Start(Uri);

            NatsClient.Subscribe(Strings.NatsSubjectVcapComponentDiscover, OnComponentDiscover);
        }

        public void SendVcapComponentAnnounce(string message)
        {
            NatsClient.Publish(Strings.NatsSubjectVcapComponentAnnounce, null, message);
        }

        public void SendReply(string reply, string message)
        {
            NatsClient.Publish(reply, null, message);
        }



    }
}
