using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CFNet = CloudFoundry.Net;


namespace Uhuru.CloudFoundry.DEA
{
    public class DeaReactor : VcapReactor
    {

        public eventSubscribeCallback OnRouterStart;

        public eventSubscribeCallback OnHealthManagerStart;

        public eventSubscribeCallback OnDeaStart;

        public eventSubscribeCallback OnDeaStop;

        public eventSubscribeCallback OnDeaStatus;

        public eventSubscribeCallback OnDropletStatus;

        public eventSubscribeCallback OnDeaDiscover;

        public eventSubscribeCallback OnDeaFindDroplet;

        public eventSubscribeCallback OnDeaUpdate;

        

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
            NatsClient.Publish("dea.heartbeat", msg: message);
        }

        public void SendDeaStart(string message)
        {
            NatsClient.Publish("dea.start", msg: message);
        }

        public void SendDopletExited(string message)
        {
            NatsClient.Publish("droplet.exited", msg: message);
            Logger.debug(String.Format("Sent droplet.exited {0}", message));
        }

        public void SendRouterRegister(string message)
        {
            NatsClient.Publish("router.register", msg: message);
        }

        public void SendRouterUnregister(string message)
        {
            NatsClient.Publish("router.unregister", msg: message);
        }
    }
}
