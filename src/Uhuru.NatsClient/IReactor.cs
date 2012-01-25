// -----------------------------------------------------------------------
// <copyright file="IReactor.cs" company="Uhuru Software, Inc.">
// Copyright (c) 2011 Uhuru Software, Inc., All Rights Reserved
// </copyright>
// -----------------------------------------------------------------------

namespace Uhuru.NatsClient
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Interface for the NATS client
    /// </summary>
    public interface IReactor : IDisposable
    {
        /// <summary>
        /// an event raised on connection
        /// </summary>
        event EventHandler<ReactorErrorEventArgs> OnConnect;

        /// <summary>
        /// This event is raised when an error message is received from the NATS server.
        /// </summary>
        event EventHandler<ReactorErrorEventArgs> OnError;

        /// <summary>
        /// Gets or sets a value indicating whether the connection is pedantic
        /// </summary>
        bool Pedantic { get; set; }

        /// <summary>
        /// Gets or sets the reconnect attempts if the tcp connection is lost
        /// </summary>
        int ReconnectAttempts { get; set; }

        /// <summary>
        /// Gets or sets the time between reconnect attempts
        /// </summary>
        int ReconnectTime { get; set; }

        /// <summary>
        /// Gets information about the server
        /// </summary>
        Dictionary<string, object> ServerInfo { get; }

        /// <summary>
        /// Gets the uri of the NAT Server
        /// </summary>
        Uri ServerUri { get; }

        /// <summary>
        /// Gets the connection status
        /// </summary>
        ConnectionStatus Status { get; }

        /// <summary>
        /// Gets or sets a value indicating whether the connection is verbose
        /// </summary>
        bool Verbose { get; set; }

        /// <summary>
        /// Call this method to attempt to reconnect the client to the NATS server.
        /// </summary>
        void AttemptReconnect();

        /// <summary>
        /// Publish a message to a given subject, with optional reply subject and completion block. 
        /// </summary>
        /// <param name="subject">the subject to publish to</param>
        void Publish(string subject);

        /// <summary>
        /// Publish a message to a given subject, with optional reply subject and completion block. 
        /// </summary>
        /// <param name="subject">the subject to publish to</param>
        /// <param name="callback">server callback</param>
        void Publish(string subject, SimpleCallback callback);

        /// <summary>
        /// Publish a message to a given subject, with optional reply subject and completion block. 
        /// </summary>
        /// <param name="subject">the subject to publish to</param>
        /// <param name="callback">server callback</param>
        /// <param name="msg">the message to publish</param>
        void Publish(string subject, SimpleCallback callback, string msg);

        /// <summary>
        /// Publish a message to a given subject, with optional reply subject and completion block. 
        /// </summary>
        /// <param name="subject">the subject to publish to</param>
        /// <param name="callback">server callback</param>
        /// <param name="msg">the message to publish</param>
        /// <param name="optReply">replay subject</param>
        void Publish(string subject, SimpleCallback callback, string msg, string optReply);

        /// <summary>
        /// Send a request.
        /// </summary>
        /// <param name="subject">The subject of the request</param>
        /// <returns>returns the subscription id</returns>
        int Request(string subject);

        /// <summary>
        /// Send a request and have the response delivered to the supplied callback
        /// </summary>
        /// <param name="subject">the subject </param>
        /// <param name="opts">additional options</param>
        /// <param name="callback">the callback for the response</param>
        /// <param name="data">data for the request</param>
        /// <returns>returns the subscription id</returns>
        int Request(string subject, System.Collections.Generic.Dictionary<string, object> opts, SubscribeCallback callback, string data);

        /// <summary>
        /// Create a client connection to the server
        /// </summary>
        /// <param name="uri">the uri of the NATS server</param>
        void Start(string uri);

        /// <summary>
        /// Close the client connection. 
        /// </summary>
        void Close();

        /// <summary>
        /// Create a client connection to the serve
        /// </summary>
        /// <param name="uri">the uri of the NATS server</param>
        void Start(Uri uri);

        /// <summary>
        /// Subscribes to the specified subject.
        /// </summary>
        /// <param name="subject">The subject.</param>
        /// <returns>The subscription id</returns>
        int Subscribe(string subject);

        /// <summary>
        /// Subscribe using the client connection to a specified subject.
        /// </summary>
        /// <param name="subject">The subject.</param>
        /// <param name="callback">The callback.</param>
        /// <returns>The subscription id</returns>
        int Subscribe(string subject, SubscribeCallback callback);

        /// <summary>
        /// Subscribe using the client connection.
        /// </summary>
        /// <param name="subject">The subject.</param>
        /// <param name="callback">The callback.</param>
        /// <param name="opts">Additional options</param>
        /// <returns>The subscription id</returns>
        int Subscribe(string subject, SubscribeCallback callback, Dictionary<string, object> opts);

        /// <summary>
        /// Unsubscribe using the client connection.
        /// </summary>
        /// <param name="sid">The subscription id of the subscription</param>
        void Unsubscribe(int sid);

        /// <summary>
        /// Unsubscribe using the client connection.
        /// </summary>
        /// <param name="sid">The subscription id to witch to un-subscribe</param>
        /// <param name="optMax">Maximum number of opt</param>
        void Unsubscribe(int sid, int optMax);
    }
}
