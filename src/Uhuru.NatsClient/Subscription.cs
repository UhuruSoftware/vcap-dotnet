// -----------------------------------------------------------------------
// <copyright file="Subscription.cs" company="Uhuru Software, Inc.">
// Copyright (c) 2011 Uhuru Software, Inc., All Rights Reserved
// </copyright>
// -----------------------------------------------------------------------

namespace Uhuru.NatsClient
{
    using System.Timers;
    
    /// <summary>
    /// a class to hold subscription-related data
    /// </summary>
    public class Subscription
    {
        /// <summary>
        /// Gets or sets the subject of the message.
        /// </summary>
        public string Subject { get; set; }

        /// <summary>
        /// Gets or sets the callback method that will be called when a message is received.
        /// </summary>
        public SubscribeCallback Callback { get; set; }

        /// <summary>
        /// Gets or sets the numbet of received messages.
        /// </summary>
        public int Received { get; set; }

        /// <summary>
        /// Gets or sets the number of queued messages.
        /// </summary>
        public int Queue { get; set; }

        /// <summary>
        /// Gets or sets the max number of messages.
        /// </summary>
        public int Max { get; set; }

        /// <summary>
        /// Gets or sets the max amount of time to wait for a message.
        /// </summary>
        public Timer Timeout { get; set; }

        // public int Expected TODO:Mitza
        // { get; set; }
    }
}
