// -----------------------------------------------------------------------
// <copyright file="VcapReactor.cs" company="Uhuru Software, Inc.">
// Copyright (c) 2011 Uhuru Software, Inc., All Rights Reserved
// </copyright>
// -----------------------------------------------------------------------

namespace Uhuru.CloudFoundry.DEA
{
    using System;
    using Uhuru.NatsClient;

    /// <summary>
    /// The reactor for the the common VCAP Component.
    /// </summary>
    public class VCAPReactor
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="VCAPReactor"/> class.
        /// </summary>
        public VCAPReactor()
        {
            this.NatsClient = ReactorFactory.GetReactor(typeof(Reactor));
            this.NatsClient.OnError += this.OnNatsError;
        }

        /// <summary>
        /// Occurs when the component.dicover message is received.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1009:DeclareEventHandlersCorrectly", Justification = "These are special events coming from the NATS client.")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1710:IdentifiersShouldHaveCorrectSuffix", Justification = "Code is more understandable this way.")]
        public event SubscribeCallback OnComponentDiscover;

        /// <summary>
        /// Occurs when NATS fails.
        /// </summary>
        public event EventHandler<ReactorErrorEventArgs> OnNatsError;

        /// <summary>
        /// Gets or sets the NATS client.
        /// </summary>
        public IReactor NatsClient
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the URI of the NATS client.
        /// </summary>
        public Uri Uri
        {
            get;
            set;
        }

        /// <summary>
        /// Starts this the reactor, by starting the NATS client and subscribing to the message bus.
        /// </summary>
        public virtual void Start()
        {
            this.NatsClient.Start(Uri);

            this.NatsClient.Subscribe(Strings.NatsSubjectVcapComponentDiscover, this.OnComponentDiscover);
        }

        /// <summary>
        /// Sends the VCAP component announce.
        /// </summary>
        /// <param name="message">The message.</param>
        public void SendVCAPComponentAnnounce(string message)
        {
            this.NatsClient.Publish(Strings.NatsSubjectVcapComponentAnnounce, null, message);
        }

        /// <summary>
        /// Send a reply to a NATS message.
        /// </summary>
        /// <param name="reply">The reply token.</param>
        /// <param name="message">The actual reply.</param>
        public void SendReply(string reply, string message)
        {
            this.NatsClient.Publish(reply, null, message);
        }
    }
}
