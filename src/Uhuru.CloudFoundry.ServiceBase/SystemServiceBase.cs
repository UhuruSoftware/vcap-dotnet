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
    using Uhuru.Configuration;
    using Uhuru.NatsClient;
    using Uhuru.Utilities;

    /// <summary>
    /// This is the service base for all Cloud Foundry system services.
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate", Justification = "Code more readable and easier to maintain.")]
    public abstract class SystemServiceBase : IDisposable
    {
        /// <summary>
        /// This is the NATS client used for communication with the rest of CLoud Foundry.
        /// </summary>
        protected IReactor nodeNats;

        /// <summary>
        /// The local IP that we use to publish stuff.
        /// </summary>
        protected string localIP;

        /// <summary>
        /// A dictionary containing orphaned services.
        /// </summary>
        protected Dictionary<string, object> orphanInstancesHash = new Dictionary<string, object>();

        /// <summary>
        /// A dictionary containing service bindings that are orphaned.
        /// </summary>
        protected Dictionary<string, object> orphanBindingHash = new Dictionary<string, object>();

        /// <summary>
        /// The VCAP component that we use to register ourselves to the Cloud Controller
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "vcap", Justification = "Spelling is correct.")]
        protected VCAPComponent vcapComponent;

        /// <summary>
        /// Starts the service using the specified options.
        /// </summary>
        /// <param name="options">The configuration options.</param>
        public virtual void Start(ServiceElement options)
        {
            if (options == null)
            {
                throw new ArgumentNullException("options");
            }

            this.localIP = NetworkInterface.GetLocalIPAddress(options.LocalRoute);

            Logger.Info(Strings.InitializingLogMessage, this.ServiceDescription());
            this.nodeNats = ReactorFactory.GetReactor(typeof(Reactor));
            this.nodeNats.OnError += new EventHandler<ReactorErrorEventArgs>(this.NatsErrorHandler);
            this.nodeNats.Start(new Uri(options.MBus));

            this.OnConnectNode();

            this.vcapComponent = new VCAPComponent();
            
            this.vcapComponent.Register(
                new Dictionary<string, object>
                {
                    { "nats", this.nodeNats },
                    { "type", this.ServiceDescription() },
                    { "host", this.localIP },
                    { "index", options.Index },
                    { "config", options },
                    { "statusPort", options.StatusPort }
                });

            int zInterval = options.ZInterval;
            TimerHelper.RecurringCall(
                zInterval,
                delegate
                {
                    this.UpdateVarz();
                });

            // give service a chance to wake up
            TimerHelper.DelayedCall(
                5 * 1000, 
                delegate
                {
                    this.UpdateVarz();
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
            if (this.nodeNats != null)
            {
                this.nodeNats.Close();
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
                if (this.nodeNats != null)
                {
                    this.nodeNats.Dispose();
                    this.nodeNats = null;
                }

                if (this.vcapComponent != null)
                {
                    this.vcapComponent.Dispose();
                    this.vcapComponent = null;
                }
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

            details["orphan_instances"] = this.orphanInstancesHash;
            details["orphan_bindings"] = this.orphanBindingHash;
            
            foreach (string key in details.Keys)
            {
                this.vcapComponent.Varz[key] = details[key];
            }
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
