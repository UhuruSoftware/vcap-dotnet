// -----------------------------------------------------------------------
// <copyright file="Subscription.cs" company="Uhuru Software">
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
        public string Subject { get; set; }
        public SubscribeCallback Callback { get; set; }
        public int Received { get; set; }
        public int Queue { get; set; }
        public int Max { get; set; }
        public Timer Timeout { get; set; }

        // public int Expected TODO:Mitza
        // { get; set; }
    }
}
