using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CloudFoundry.Net.Nats;
using CloudFoundry.Net.DEA;
using Uhuru.CloudFoundry.Server;
using Uhuru.Utilities;


namespace Uhuru.CloudFoundry.Server.MsSqlNode.Base
{
    public abstract class Base : IDisposable
    {
        Options options;
        private string local_ip;
        Dictionary<string, object> orphan_ins_hash;
        Dictionary<string, object> orphan_binding_hash;
        protected Client node_nats;
        VcapComponent vcapComponent;

        public virtual void Start(Options options)
        {
            this.options = options;
            local_ip = NetworkInterface.GetLocalIPAddress();
            Logger.Info(String.Format("{0}: Initializing", service_description()));
            orphan_ins_hash = new Dictionary<string, object>();
            orphan_binding_hash = new Dictionary<string, object>();

            node_nats = new Client();
            node_nats.Start(options.Uri);
            
            on_connect_node();

            vcapComponent = new VcapComponent();

            vcapComponent.Register(
                new Dictionary<string, object>
                {
                    {"nats", node_nats},
                    {"type", service_description()},
                    {"host", local_ip},
                    {"index", options.Index},
                    {"config", options}
                });

            int z_interval = options.ZInterval;


            // give service a chance to wake up
            TimerHelper.DelayedCall(5000, delegate()
            {
                update_varz();
            });

            TimerHelper.RecurringCall(z_interval, delegate()
            {
                update_varz();
            });

            // give service a chance to wake up
            TimerHelper.DelayedCall(5000, delegate()
            {
                update_healthz();
            });

            TimerHelper.RecurringCall(z_interval, delegate()
            {
                update_healthz();
            });
        }

        public string service_description()
        {
            return String.Format("{0}-{1}", service_name(), flavor());
        }

        private void update_varz()
        {
            //TODO: vladi: implement this
            //vz = varz_details
            //vz[:orphan_instances] = @orphan_ins_hash
            //vz[:orphan_bindings] = @orphan_binding_hash
            //vz.each { |k,v|
            //  VCAP::Component.varz[k] = v
            //}
        }


        private void update_healthz()
        {
            //TODO: vladi: implement this
            //VCAP::Component.healthz = Yajl::Encoder.encode(healthz_details, :pretty => true, :terminator => "\n")
        }

        public void Shutdown()
        {
            Logger.Info(String.Format("{0}: Shutting down", service_description()));
            node_nats.Stop();
        }

        // Subclasses VCAP::Services::Base::{Node,Provisioner} implement the
        // following methods. (Note that actual service Provisioner or Node
        // implementations should NOT need to touch these!)

        // TODO on_connect_node should be on_connect_nats
        protected abstract void on_connect_node();
        protected abstract string flavor(); // "Provisioner" or "Node"
        protected abstract Dictionary<string, object> varz_details();
        protected abstract Dictionary<string, string> healthz_details();

        // Service Provisioner and Node classes must implement the following
        // method
        protected abstract string service_name();

        public void Dispose()
        {
            node_nats.Dispose();
            //Dispose(true);

            GC.SuppressFinalize(this);
        }
    }
}
