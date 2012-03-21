// -----------------------------------------------------------------------
// <copyright file="SystemServiceBase.cs" company="Uhuru Software, Inc.">
// Copyright (c) 2011 Uhuru Software, Inc., All Rights Reserved
// </copyright>
// -----------------------------------------------------------------------

namespace Uhuru.CloudFoundry.ServiceBase
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using Uhuru.NatsClient;
    using Uhuru.Utilities;
    using Uhuru.Utilities.Json;

    /// <summary>
    /// This is the service base for all Cloud Foundry system services.
    /// </summary>
    public abstract class SystemServiceBase : IDisposable
    {
        /// <summary>
        /// This is the NATS client used for communication with the rest of CLoud Foundry.
        /// </summary>
        private IReactor nodeNats;

        /// <summary>
        /// The local IP that we use to publish stuff.
        /// </summary>
        private string localIP;

        /// <summary>
        /// A dictionary containing orphaned services.
        /// </summary>
        private Dictionary<string, object> orphanInstancesHash;

        /// <summary>
        /// A dictionary containing service bindings that are orphaned.
        /// </summary>
        private Dictionary<string, object> orphanBindingHash;

        /// <summary>
        /// The VCAP component that we use to register ourselves to the Cloud Controller
        /// </summary>
        private VCAPComponent vcapComponent;

        /// <summary>
        /// Gets the nats reactor used for communicating with the cloud controller.
        /// </summary>
        public IReactor NodeNats
        {
            get
            {
                return this.nodeNats;
            }
        }
        
        /// <summary>
        /// Gets or sets the orphan instances hash.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly", Justification = "This is needed for JSON (de)serialization")]
        protected Dictionary<string, object> OrphanInstancesHash
        {
            get
            {
                return this.orphanInstancesHash;
            }

            set
            {
                this.orphanInstancesHash = value;
            }
        }
        
        /// <summary>
        /// Gets or sets the orphan binding hash.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly", Justification = "This is needed for JSON (de)serialization")]
        protected Dictionary<string, object> OrphanBindingHash
        {
            get
            {
                return this.orphanBindingHash;
            }

            set
            {
                this.orphanBindingHash = value;
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

            this.localIP = NetworkInterface.GetLocalIPAddress();
            Logger.Info(Strings.InitializingLogMessage, this.ServiceDescription());
            this.OrphanInstancesHash = new Dictionary<string, object>();
            this.OrphanBindingHash = new Dictionary<string, object>();

            this.nodeNats = ReactorFactory.GetReactor(typeof(Reactor));
            this.nodeNats.OnError += new EventHandler<ReactorErrorEventArgs>(this.NatsErrorHandler);
            this.NodeNats.Start(options.Uri);

            this.OnConnectNode();

            this.vcapComponent = new VCAPComponent();

            this.vcapComponent.Register(
                new Dictionary<string, object>
                {
                    { "nats", this.NodeNats },
                    { "type", this.ServiceDescription() },
                    { "host", this.localIP },
                    { "index", options.Index },
                    { "config", options },
                    { "statusPort", options.StatusPort }
                });

            int zInterval = options.ZInterval;

            // give service a chance to wake up
            TimerHelper.DelayedCall(
                5000, 
                delegate
                {
                    this.UpdateVarz();
                });

            TimerHelper.RecurringCall(
                zInterval, 
                delegate
                {
                    this.UpdateVarz();
                });

            // give service a chance to wake up
            TimerHelper.DelayedCall(
                5000, 
                delegate
                {
                    this.UpdateHealthz();
                });

            TimerHelper.RecurringCall(
                zInterval, 
                delegate
                {
                    this.UpdateHealthz();
                });
        }

        /// <summary>
        /// Gets the service description.
        /// </summary>
        /// <returns>A string containing the service description.</returns>
        public string ServiceDescription()
        {
            return string.Format(CultureInfo.InvariantCulture, "{0}-{1}", this.ServiceName(), this.Flavor());
        }

        /// <summary>
        /// Implementation of IDisposable.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Stops this instance.
        /// </summary>
        public void Shutdown()
        {
            Logger.Info(Strings.ShuttingDownLogMessage, this.ServiceDescription());
            if (this.NodeNats != null)
            {
                this.NodeNats.Close();
            }
        }

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources
        /// </summary>
        /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                this.nodeNats.Close();
                this.nodeNats.Dispose();
                this.vcapComponent.Dispose();
            }
        }
                
        /// <summary>
        /// Called after the node is connected to NATS.
        /// </summary>
        protected abstract void OnConnectNode();
        
        /// <summary>
        /// Gets the flavor of the service. Only "Node" for the .net world.
        /// </summary>
        /// <returns>On windows, this always returns the value "Node"</returns>
        protected abstract string Flavor();

        /// <summary>
        /// Generates credentials for a new service instance that has to be provisioned.
        /// </summary>
        /// <returns>Service credentials - name, user and password.</returns>
        protected abstract ServiceCredentials GenerateCredentials();
        
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
        /// Updates the varz message.
        /// </summary>
        private void UpdateVarz()
        {
            Dictionary<string, object> details = this.VarzDetails();

            details["orphan_instances"] = this.OrphanInstancesHash;
            details["orphan_bindings"] = this.OrphanBindingHash;
            
            foreach (string key in details.Keys)
            {
                this.vcapComponent.Varz[key] = details[key];
            }
        }

        /// <summary>
        /// Updates the healthz message.
        /// </summary>
        private void UpdateHealthz()
        {
            this.vcapComponent.Healthz = JsonConvertibleObject.SerializeToJson(this.HealthzDetails());
        }

        /// <summary>
        /// NATS the error handler.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="args">The <see cref="Uhuru.NatsClient.ReactorErrorEventArgs"/> instance containing the error data.</param>
        private void NatsErrorHandler(object sender, ReactorErrorEventArgs args)
        {
            string errorThrown = args.Message == null ? string.Empty : args.Message;
            Logger.Fatal(Strings.ExitingNatsError, errorThrown);
            Environment.FailFast(string.Format(CultureInfo.InvariantCulture, Strings.NatsError, errorThrown), args.Exception);
        }
    }
}
