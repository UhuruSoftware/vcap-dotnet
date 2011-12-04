using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Uhuru.Utilities;
using System.Globalization;
using Uhuru.NatsClient;


namespace Uhuru.CloudFoundry.ServiceBase
{
    public abstract class SystemServiceBase : IDisposable
    {
        private Reactor nodeNats;

        private string localIP;
        VcapComponent vcapComponent;

        public Reactor NodeNats
        {
            get
            {
            return nodeNats;
            }
            }

        public virtual void Start(Options options)
        {
            if (options == null)
            {
                throw new ArgumentNullException("options");
            }

            localIP = NetworkInterface.GetLocalIPAddress();
            Logger.Info(Strings.InitializingLogMessage, ServiceDescription());
      
            nodeNats = new Reactor();
            NodeNats.Start(new Uri(options.Uri));
            
            OnConnectNode();

            vcapComponent = new VcapComponent();

            vcapComponent.Register(
                new Dictionary<string, object>
                {
                    {"nats", NodeNats},
                    {"type", ServiceDescription()},
                    {"host", localIP},
                    {"index", options.Index},
                    {"config", options}
                });

            int z_interval = options.ZInterval;


            // give service a chance to wake up
            TimerHelper.DelayedCall(5000, delegate()
            {
                UpdateVarz();
            });

            TimerHelper.RecurringCall(z_interval, delegate()
            {
                UpdateVarz();
            });

            // give service a chance to wake up
            TimerHelper.DelayedCall(5000, delegate()
            {
                UpdateHealthz();
            });

            TimerHelper.RecurringCall(z_interval, delegate()
            {
                UpdateHealthz();
            });
        }

        public string ServiceDescription()
        {
            return String.Format(CultureInfo.InvariantCulture, "{0}-{1}", ServiceName(), Flavor());
        }

        private void UpdateVarz()
        {
            //TODO: vladi: implement this
            //vz = varz_details
            //vz[:orphan_instances] = @orphan_ins_hash
            //vz[:orphan_bindings] = @orphan_binding_hash
            //vz.each { |k,v|
            //  VCAP::Component.varz[k] = v
            //}
        }


        private void UpdateHealthz()
        {
            //TODO: vladi: implement this
            //VCAP::Component.healthz = Yajl::Encoder.encode(healthz_details, :pretty => true, :terminator => "\n")
        }

        public void Shutdown()
        {
           Logger.Info(Strings.ShuttingDownLogMessage, ServiceDescription());
            NodeNats.Stop();
        }

        // Subclasses VCAP::Services::Base::{Node,Provisioner} implement the
        // following methods. (Note that actual service Provisioner or Node
        // implementations should NOT need to touch these!)

        // TODO on_connect_node should be on_connect_nats
        protected abstract void OnConnectNode();
        protected abstract string Flavor(); // "Provisioner" or "Node"
        protected abstract Dictionary<string, object> VarzDetails();
        protected abstract Dictionary<string, string> HealthzDetails();

        // Service Provisioner and Node classes must implement the following method
        protected abstract string ServiceName();

        public void Dispose()
        {
            NodeNats.Dispose();
            //Dispose(true);

            GC.SuppressFinalize(this);
        }
    }
}
