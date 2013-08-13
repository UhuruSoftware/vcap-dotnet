// -----------------------------------------------------------------------
// <copyright file="DeaReactor.cs" company="Uhuru Software, Inc.">
// Copyright (c) 2011 Uhuru Software, Inc., All Rights Reserved
// </copyright>
// -----------------------------------------------------------------------

namespace Uhuru.CloudFoundry.DEA
{
    using System.Globalization;
    using Uhuru.NatsClient;
    using Uhuru.Utilities;

    /// <summary>
    /// The reactor for the DEA. It is basically a wrapper for the NATS client. It inherits the common VCAP reactor which belongs to the VcapComponent.
    /// </summary>
    public class DeaReactor : VCAPReactor
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DeaReactor"/> class.
        /// </summary>
        public DeaReactor()
        {
        }

        /// <summary>
        /// Occurs when router.start message is received on the message bus.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1710:IdentifiersShouldHaveCorrectSuffix", Justification = "Suitable for this context.")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1009:DeclareEventHandlersCorrectly", Justification = "Suitable for this context.")]
        public event SubscribeCallback OnRouterStart;

        /// <summary>
        /// Occurs when healthmanager.start message is received on the message bus.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1710:IdentifiersShouldHaveCorrectSuffix", Justification = "Suitable for this context.")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1009:DeclareEventHandlersCorrectly", Justification = "Suitable for this context.")]
        public event SubscribeCallback OnHealthManagerStart;

        /// <summary>
        /// Occurs when dea.{vcapguid}.start message is received on the message bus.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1710:IdentifiersShouldHaveCorrectSuffix", Justification = "Suitable for this context.")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1009:DeclareEventHandlersCorrectly", Justification = "Suitable for this context.")]
        public event SubscribeCallback OnDeaStart;

        /// <summary>
        /// Occurs when dea.stop message is received on the message bus.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1710:IdentifiersShouldHaveCorrectSuffix", Justification = "Suitable for this context.")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1009:DeclareEventHandlersCorrectly", Justification = "Suitable for this context.")]
        public event SubscribeCallback OnDeaStop;

        /// <summary>
        /// Occurs when the dea.status message is received on the message bus.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1710:IdentifiersShouldHaveCorrectSuffix", Justification = "Suitable for this context.")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1009:DeclareEventHandlersCorrectly", Justification = "Suitable for this context.")]
        public event SubscribeCallback OnDeaStatus;

        /// <summary>
        /// Occurs when the droplet.status message is received on the message bus.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1710:IdentifiersShouldHaveCorrectSuffix", Justification = "Suitable for this context.")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1009:DeclareEventHandlersCorrectly", Justification = "Suitable for this context.")]
        public event SubscribeCallback OnDropletStatus;

        /// <summary>
        /// Occurs when dea.find.droplet message is received on the message bus.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1710:IdentifiersShouldHaveCorrectSuffix", Justification = "Suitable for this context.")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1009:DeclareEventHandlersCorrectly", Justification = "Suitable for this context.")]
        public event SubscribeCallback OnDeaFindDroplet;

        /// <summary>
        /// Occurs when dea.update message is received on the message bus.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1710:IdentifiersShouldHaveCorrectSuffix", Justification = "Suitable for this context.")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1009:DeclareEventHandlersCorrectly", Justification = "Suitable for this context.")]
        public event SubscribeCallback OnDeaUpdate;

        /// <summary>
        /// Occurs when dea.locate message is received on the message bus.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1710:IdentifiersShouldHaveCorrectSuffix", Justification = "Suitable for this context.")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1009:DeclareEventHandlersCorrectly", Justification = "Suitable for this context.")]
        public event SubscribeCallback OnDeaLocate;

        /// <summary>
        /// Gets or sets the UUID of the vcap component.
        /// </summary>
        public string UUID
        {
            get;
            set;
        }

        /// <summary>
        /// Runs the Dea Reactor. This function is not blocking.
        /// </summary>
        public override void Start()
        {
            base.Start();

            NatsClient.Subscribe("dea.status", this.OnDeaStatus);
            NatsClient.Subscribe("droplet.status", this.OnDropletStatus);
            
            NatsClient.Subscribe("dea.find.droplet", this.OnDeaFindDroplet);
            NatsClient.Subscribe("dea.update", this.OnDeaUpdate);
            NatsClient.Subscribe("dea.locate", this.OnDeaLocate);

            NatsClient.Subscribe("dea.stop", this.OnDeaStop);
            NatsClient.Subscribe(string.Format(CultureInfo.InvariantCulture, "dea.{0}.start", this.UUID), this.OnDeaStart);

            NatsClient.Subscribe("router.start", this.OnRouterStart);
            NatsClient.Subscribe("healthmanager.start", this.OnHealthManagerStart);
        }

        /// <summary>
        /// Sends the DEA heartbeat to the message bus.
        /// </summary>
        /// <param name="message">The message.</param>
        public void SendDeaHeartbeat(string message)
        {
            NatsClient.Publish("dea.heartbeat", null, message);
        }

        /// <summary>
        /// Sends the DEA start to the message bus.
        /// </summary>
        /// <param name="message">The message.</param>
        public void SendDeaStart(string message)
        {
            NatsClient.Publish("dea.start", null, message);
        }

        /// <summary>
        /// Sends the droplet exited to the message bus.
        /// </summary>
        /// <param name="message">The message.</param>
        public void SendDropletExited(string message)
        {
            NatsClient.Publish("droplet.exited", null, message);
            Logger.Debug(Strings.SentDropletExited, message);
        }

        /// <summary>
        /// Sends the router register to the message bus.
        /// </summary>
        /// <param name="message">The message.</param>
        public void SendRouterRegister(string message)
        {
            NatsClient.Publish("router.register", null, message);
        }

        /// <summary>
        /// Sends the router unregister to the message bus.
        /// </summary>
        /// <param name="message">The message.</param>
        public void SendRouterUnregister(string message)
        {
            NatsClient.Publish("router.unregister", null, message);
        }

        /// <summary>
        /// Sends the DEA advertise message for centralized apps placement.
        /// </summary>
        /// <param name="message">The message.</param>
        public void SendDeaAdvertise(string message)
        {
            NatsClient.Publish("dea.advertise", null, message);
        }

        /// <summary>
        /// Sends greeting message to routers.
        /// </summary>
        /// <param name="callback">Callback on reponse.</param>
        public void SendRouterGreetings(SubscribeCallback callback)
        {
            NatsClient.Request("router.greet", null, callback, "{}");
        }
    }
}
