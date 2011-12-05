using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Uhuru.NatsClient;


namespace Uhuru.CloudFoundry.DEA
{
    public class DeaReactor : VcapReactor
    {

        public event SubscribeCallback OnRouterStart;

        public event SubscribeCallback OnHealthManagerStart;

        public event SubscribeCallback OnDeaStart;

        public event SubscribeCallback OnDeaStop;

        public event SubscribeCallback OnDeaStatus;

        public event SubscribeCallback OnDropletStatus;

        public event SubscribeCallback OnDeaDiscover;

        public event SubscribeCallback OnDeaFindDroplet;

        public event SubscribeCallback OnDeaUpdate;

        

        public DeaReactor()
        {

        }

        public string Uuid
        {
            get;
            set;
        }

        //Runs the DeaReactor in a blocking mode
        public override void Start()
        {
            base.Start();
            

            NatsClient.Subscribe("dea.status", OnDeaStatus);
            NatsClient.Subscribe("droplet.status", OnDropletStatus);
            NatsClient.Subscribe("dea.discover", OnDeaDiscover);
            NatsClient.Subscribe("dea.find.droplet", OnDeaFindDroplet);
            NatsClient.Subscribe("dea.update", OnDeaUpdate);

            NatsClient.Subscribe("dea.stop", OnDeaStop);
            NatsClient.Subscribe(String.Format("dea.{0}.start", Uuid), OnDeaStart);

            NatsClient.Subscribe("router.start", OnRouterStart);
            NatsClient.Subscribe("healthmanager.start", OnHealthManagerStart);
        }

        public void SendDeaHeartbeat(string message)
        {
            NatsClient.Publish("dea.heartbeat", null, message);
        }

        public void SendDeaStart(string message)
        {
            NatsClient.Publish("dea.start", null,  message);
        }

        public void SendDopletExited(string message)
        {
            NatsClient.Publish("droplet.exited", null, message);
            Logger.debug(String.Format("Sent droplet.exited {0}", message));
        }

        public void SendRouterRegister(string message)
        {
            NatsClient.Publish("router.register", null, message);
        }

        public void SendRouterUnregister(string message)
        {
            NatsClient.Publish("router.unregister", null, message);
        }
    }
}
