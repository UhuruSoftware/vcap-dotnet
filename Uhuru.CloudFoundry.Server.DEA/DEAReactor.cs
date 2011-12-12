// -----------------------------------------------------------------------
// <copyright file="DeaReactor.cs" company="Uhuru Software">
// Copyright (c) 2011 Uhuru Software, Inc., All Rights Reserved
// </copyright>
// -----------------------------------------------------------------------

namespace Uhuru.CloudFoundry.DEA
{
    using System;
    using System.Globalization;
    using Uhuru.NatsClient;
    using Uhuru.Utilities;

    public class DeaReactor : VcapReactor
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1710:IdentifiersShouldHaveCorrectSuffix")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1009:DeclareEventHandlersCorrectly")]
        public event SubscribeCallback OnRouterStart;

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1710:IdentifiersShouldHaveCorrectSuffix")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1009:DeclareEventHandlersCorrectly")]
        public event SubscribeCallback OnHealthManagerStart;

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1710:IdentifiersShouldHaveCorrectSuffix")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1009:DeclareEventHandlersCorrectly")]
        public event SubscribeCallback OnDeaStart;

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1710:IdentifiersShouldHaveCorrectSuffix")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1009:DeclareEventHandlersCorrectly")]
        public event SubscribeCallback OnDeaStop;

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1710:IdentifiersShouldHaveCorrectSuffix")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1009:DeclareEventHandlersCorrectly")]
        public event SubscribeCallback OnDeaStatus;

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1710:IdentifiersShouldHaveCorrectSuffix")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1009:DeclareEventHandlersCorrectly")]
        public event SubscribeCallback OnDropletStatus;

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1710:IdentifiersShouldHaveCorrectSuffix")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1009:DeclareEventHandlersCorrectly")]
        public event SubscribeCallback OnDeaDiscover;

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1710:IdentifiersShouldHaveCorrectSuffix")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1009:DeclareEventHandlersCorrectly")]
        public event SubscribeCallback OnDeaFindDroplet;

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1710:IdentifiersShouldHaveCorrectSuffix")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1009:DeclareEventHandlersCorrectly")]
        public event SubscribeCallback OnDeaUpdate;

        /// <summary>
        /// Initializes a new instance of the <see cref="DeaReactor"/> class.
        /// </summary>
        public DeaReactor()
        {
        }

        /// <summary>
        /// Gets or sets the UUID of the vcap component.
        /// </summary>
        public string Uuid
        {
            get;
            set;
        }

        /// <summary>
        /// Runs the Dea Reacot. This function is not blocking.
        /// </summary>
        public override void Start()
        {
            base.Start();
            
            NatsClient.Subscribe("dea.status", OnDeaStatus);
            NatsClient.Subscribe("droplet.status", OnDropletStatus);
            NatsClient.Subscribe("dea.discover", OnDeaDiscover);
            NatsClient.Subscribe("dea.find.droplet", OnDeaFindDroplet);
            NatsClient.Subscribe("dea.update", OnDeaUpdate);

            NatsClient.Subscribe("dea.stop", OnDeaStop);
            NatsClient.Subscribe(String.Format(CultureInfo.InvariantCulture, Strings.NatsMessageDeaStart, Uuid), OnDeaStart);

            NatsClient.Subscribe("router.start", OnRouterStart);
            NatsClient.Subscribe("healthmanager.start", OnHealthManagerStart);
        }


        /// <summary>
        /// Sends the dea heartbeat to the message bus.
        /// </summary>
        /// <param name="message">The message.</param>
        public void SendDeaHeartbeat(string message)
        {
            NatsClient.Publish("dea.heartbeat", null, message);
        }

        /// <summary>
        /// Sends the dea start to the message bus.
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
    }
}
