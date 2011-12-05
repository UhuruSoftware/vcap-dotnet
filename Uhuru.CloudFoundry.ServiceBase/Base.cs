using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Uhuru.Utilities;
using System.Globalization;
using Uhuru.NatsClient;


namespace Uhuru.CloudFoundry.ServiceBase
{
    /// <summary>
    /// This is the service base for all Cloud Foundry system services.
    /// </summary>
    public abstract class SystemServiceBase : IDisposable
    {
        private Reactor nodeNats;

        private Options configurationOptions;
        private string localIP;
        private Dictionary<string, object> orphanInsHash;
        private Dictionary<string, object> orphanBindingHash;
        private VcapComponent vcapComponent;

        /// <summary>
        /// Gets or sets the orphan instances hash.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        protected Dictionary<string, object> OrphanInstancesHash
        {
            get
            {
                return orphanInsHash;
            }
            set
            {
                orphanInsHash = value;
            }
        }

        /// <summary>
        /// Gets or sets the orphan binding hash.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        protected Dictionary<string, object> OrphanBindingHash
        {
            get
            {
                return orphanBindingHash;
            }
            set
            {
                orphanBindingHash = value;
            }
        }


        /// <summary>
        /// Gets the nats reactor used for communicating with the cloud controller.
        /// </summary>
        public Reactor NodeNats
        {
            get
            {
                return nodeNats;
            }
        }


        /// <summary>
        /// Starts the service using the specified options.
        /// </summary>
        /// <param name="options">The configuration options.</param>
        public virtual void Start(Options options)
        {
            if (options == null)
            {
                throw new ArgumentNullException("options");
            }

            this.configurationOptions = options;
            localIP = NetworkInterface.GetLocalIPAddress();
            Logger.Info(Strings.InitializingLogMessage, ServiceDescription());
            OrphanInstancesHash = new Dictionary<string, object>();
            OrphanBindingHash = new Dictionary<string, object>();

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

        /// <summary>
        /// Gets the service description.
        /// </summary>
        /// <returns>A string containing the service description.</returns>
        public string ServiceDescription()
        {
            return String.Format(CultureInfo.InvariantCulture, "{0}-{1}", ServiceName(), Flavor());
        }

        private void UpdateVarz()
        {
            Dictionary<string, object> details = VarzDetails();

            details["orphan_instances"] = OrphanInstancesHash;
            details["orphan_bindings"] = OrphanBindingHash;

            vcapComponent.Varz = details;
        }


        private void UpdateHealthz()
        {
            vcapComponent.Healthz = JsonConvertibleObject.SerializeToJson(HealthzDetails());
        }

        /// <summary>
        /// Stops this instance.
        /// </summary>
        public void Shutdown()
        {
           Logger.Info(Strings.ShuttingDownLogMessage, ServiceDescription());
            NodeNats.Stop();
        }


        
        /// <summary>
        /// Called after the node is connected to NATS.
        /// </summary>
        protected abstract void OnConnectNode();
        /// <summary>
        /// Gets the flavor of the service. Only "Node" for the .net world.
        /// </summary>
        /// <returns>"Node"</returns>
        protected abstract string Flavor();
        /// <summary>
        /// Gets the varz details for this service.
        /// </summary>
        /// <returns>A dictionary containing varz variables.</returns>
        protected abstract Dictionary<string, object> VarzDetails();
        /// <summary>
        /// Gets the healthz details for this service.
        /// </summary>
        /// <returns>A dictionary containing healthz details.</returns>
        protected abstract Dictionary<string, string> HealthzDetails();
        /// <summary>
        /// Gets the service name.
        /// </summary>
        /// <returns>A tring containing the service name.</returns>
        protected abstract string ServiceName();

        /// <summary>
        /// Implementation of IDisposable.
        /// </summary>
        public void Dispose()
        {
            NodeNats.Dispose();
            //Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
