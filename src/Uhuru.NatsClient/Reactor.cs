// -----------------------------------------------------------------------
// <copyright file="Reactor.cs" company="Uhuru Software, Inc.">
// Copyright (c) 2011 Uhuru Software, Inc., All Rights Reserved
// </copyright>
// -----------------------------------------------------------------------

using System;

[assembly: CLSCompliant(true)]

namespace Uhuru.NatsClient
{
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Net.Sockets;
    using System.Text;
    using System.Text.RegularExpressions;
    using System.Threading;
    using System.Threading.Tasks;
    using Uhuru.NatsClient.Resources;
    using Uhuru.Utilities;
    using Uhuru.Utilities.Json;
    
    /// <summary>
    /// Delegate definition for a method to be called by the Reactor when a pong is received.
    /// </summary>
    public delegate void SimpleCallback();

    /// <summary>
    /// Delegate definition for a method to be called by the reactor when a subscription receives a message.
    /// </summary>
    /// <param name="msg">Message that was received.</param>
    /// <param name="reply">Reply string.</param>
    /// <param name="subject">Subject of the message.</param>
    public delegate void SubscribeCallback(string msg, string reply, string subject);
    
    /// <summary>
    /// Delegate definition for a method to be called when an un-subscription is complete.
    /// </summary>
    /// <param name="sid">Subscription id.</param>
    public delegate void UnsubscribeCallback(int sid);

    /// <summary>
    /// This class is the NATS client, it is used to communicate to the NATS server.
    /// The reactor enables pub/sub style communication.
    /// </summary>
    public sealed class Reactor : IDisposable
    {
        /// <summary>
        /// Used for creating OnMessage tasks.
        /// </summary>
        private TaskFactory messageCallbackFactory;

        /// <summary>
        /// Dictionary containing all subscription 
        /// </summary>
        private Dictionary<int, Subscription> subscriptions = new Dictionary<int, Subscription>();
        
        /// <summary>
        /// Determines the current receive data state
        /// </summary>
        private ParseState parseState;

        /// <summary>
        /// Determines if the connection is pedantic
        /// </summary>
        private bool pedantic;

        /// <summary>
        /// Determines if the connection is verbose
        /// </summary>
        private bool verbose;

        /// <summary>
        /// Dictionary containing the server info
        /// </summary>
        private Dictionary<string, object> serverInfo;

        /// <summary>
        /// Buffer retrieved from the network stream
        /// </summary>
        private byte[] readBuffer;

        /// <summary>
        /// Current buffer
        /// </summary>
        private List<byte> buf = new List<byte>();

        /// <summary>
        /// Tcp client used to connect to the NATS Server
        /// </summary>
        private TcpClient tcpClient;

        /// <summary>
        /// The uri to the NATS server
        /// </summary>
        private Uri serverUri;

        /// <summary>
        /// The status of the connection
        /// </summary>
        private ConnectionStatus status = ConnectionStatus.Closed;

        /// <summary>
        /// List containing the pending, not processed commands
        /// </summary>
        private List<object> pendingCommands = new List<object>();

        /// <summary>
        /// PendingCommands synchronization lock.
        /// </summary>
        private object pendingCommandsLock = new object();

        /// <summary>
        /// Queue containing all the pong callbacks
        /// </summary>
        private Queue<SimpleCallback> pongs = new Queue<SimpleCallback>();

        /// <summary>
        /// Resource instance containing all the strings from the resx files
        /// </summary>
        private Resource resource = Resource.Instance;

        /// <summary>
        /// Current subject id
        /// </summary>
        private int ssid = 0;

        /// <summary>
        /// Number of reconnect attempts
        /// </summary>
        private int reconnectAttempts = 10;

        /// <summary>
        /// Time to wait between attempts
        /// </summary>
        private int reconnectTime = 10000;

        /// <summary>
        /// Locker for the publish method
        /// </summary>
        private object publishLock = new object();

        /// <summary>
        /// the total size of the message that must be retrieved
        /// </summary>
        private int needed;

        /// <summary>
        /// The current subscription id 
        /// </summary>
        private int subscriptionId;

        /// <summary>
        /// the current reply
        /// </summary>
        private string reply;

        /// <summary>
        /// an event raised on connection
        /// </summary>
        public event EventHandler<ReactorErrorEventArgs> OnConnect;

        /// <summary>
        /// This event is raised when an error message is received from the NATS server.
        /// </summary>
        public event EventHandler<ReactorErrorEventArgs> OnError;
        
        /// <summary>
        /// Gets or sets the reconnect attempts if the tcp connection is lost
        /// </summary>
        public int ReconnectAttempts
        {
            get
            {
                return this.reconnectAttempts;
            }

            set
            {
                this.reconnectAttempts = value;
            }
        }

        /// <summary>
        /// Gets or sets the time between reconnect attempts
        /// </summary>
        public int ReconnectTime
        {
            get
            {
                return this.reconnectTime;
            }

            set
            {
                this.reconnectTime = value;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the connection is pedantic
        /// </summary>
        public bool Pedantic
        {
            get
            {
                return this.pedantic;
            }

            set
            {
                this.pedantic = value;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the connection is verbose
        /// </summary>
        public bool Verbose
        {
            get
            {
                return this.verbose;
            }

            set
            {
                this.verbose = value;
                throw new NotImplementedException();
            }
        }

        /// <summary>
        /// Gets information about the server
        /// </summary>
        public Dictionary<string, object> ServerInfo
        {
            get { return this.serverInfo; }
        }

        /// <summary>
        /// Gets the connection status
        /// </summary>
        public ConnectionStatus Status
        {
            get
            {
                return this.status;
            }

            private set
            {
                this.status = value;
            }
        }

        /// <summary>
        /// Gets the uri of the NAT Server
        /// </summary>
        public Uri ServerUri
        {
            get
            {
                return this.serverUri;
            }
        }

        /// <summary>
        /// Create a client connection to the server
        /// </summary>
        /// <param name="uri">the uri of the NATS server</param>
        public void Start(string uri)
        {
            this.Start(new Uri(uri));  
        }

        /// <summary>
        /// Create a client connection to the serve
        /// </summary>
        /// <param name="uri">the uri of the NATS server</param>
        public void Start(Uri uri)
        {
            if (uri == null)
            {
                throw new ArgumentNullException("uri");
            }

            this.messageCallbackFactory = new TaskFactory(TaskCreationOptions.LongRunning, TaskContinuationOptions.ExecuteSynchronously);

            this.serverUri = uri;

            this.tcpClient = new TcpClient();
            this.tcpClient.Connect(this.serverUri.Host, this.serverUri.Port);

            this.Connect();
        }

        /// <summary>
        /// Publish a message to a given subject, with optional reply subject and completion block. 
        /// </summary>
        /// <param name="subject">the subject to publish to</param>
        /// <param name="callback">server callback</param>
        /// <param name="msg">the message to publish</param>
        /// <param name="optReply">replay subject</param>
        public void Publish(string subject, SimpleCallback callback, string msg, string optReply)
        {
            lock (this.publishLock)
            {
                if (msg == null)
                {
                    throw new ArgumentNullException("msg");
                }

                if (string.IsNullOrEmpty(subject))
                {
                    return;
                }

                this.SendCommand(string.Format(CultureInfo.InvariantCulture, "PUB {0} {1} {2}{3}{4}{5}", subject, optReply, msg.Length, Resource.CRLF, msg, Resource.CRLF));
                if (callback != null)
                {
                    this.QueueServer(callback);
                }
            }
        }

        /// <summary>
        /// Publish a message to a given subject, with optional reply subject and completion block. 
        /// </summary>
        /// <param name="subject">the subject to publish to</param>
        public void Publish(string subject)
        {
            this.Publish(subject, null, Resource.EMPTYMSG, null);
        }

        /// <summary>
        /// Publish a message to a given subject, with optional reply subject and completion block. 
        /// </summary>
        /// <param name="subject">the subject to publish to</param>
        /// <param name="callback">server callback</param>
        public void Publish(string subject, SimpleCallback callback)
        {
            this.Publish(subject, callback, Resource.EMPTYMSG, null);
        }

        /// <summary>
        /// Publish a message to a given subject, with optional reply subject and completion block. 
        /// </summary>
        /// <param name="subject">the subject to publish to</param>
        /// <param name="callback">server callback</param>
        /// <param name="msg">the message to publish</param>
        public void Publish(string subject, SimpleCallback callback, string msg)
        {
            this.Publish(subject, callback, msg, null);
        }

        /// <summary>
        /// Subscribes to the specified subject.
        /// </summary>
        /// <param name="subject">The subject.</param>
        /// <returns>The subscription id</returns>
        public int Subscribe(string subject)
        {
            return Subscribe(subject, null, null);
        }

        /// <summary>
        /// Subscribe using the client connection to a specified subject.
        /// </summary>
        /// <param name="subject">The subject.</param>
        /// <param name="callback">The callback.</param>
        /// <returns>The subscription id</returns>
        public int Subscribe(string subject, SubscribeCallback callback)
        {
            return Subscribe(subject, callback, null);
        }

        /// <summary>
        /// Subscribe using the client connection.
        /// </summary>
        /// <param name="subject">The subject.</param>
        /// <param name="callback">The callback.</param>
        /// <param name="opts">Additional options</param>
        /// <returns>The subscription id</returns>
        public int Subscribe(string subject, SubscribeCallback callback, Dictionary<string, object> opts)
        {
            if (string.IsNullOrEmpty(subject))
            {
                return 0;
            }

            int sid = this.ssid += 1;

            if (opts == null)
            {
                opts = new Dictionary<string, object>();
            }

            Subscription sub = this.subscriptions[sid] = new Subscription()
            {
                Subject = subject,
                Callback = callback,
                Received = 0,
                Queue = opts.ContainsKey("queue") ? Convert.ToInt32(opts["queue"], CultureInfo.InvariantCulture) : 0,
                Max = opts.ContainsKey("max") ? Convert.ToInt32(opts["max"], CultureInfo.InvariantCulture) : 0
            };

            this.SendCommand(string.Format(CultureInfo.InvariantCulture, "SUB {0} {1} {2}{3}", subject, sub.Queue == 0 ? string.Empty : sub.Queue.ToString(CultureInfo.InvariantCulture), sid, Resource.CRLF));

            if (sub.Max > 0)
            {
                this.Unsubscribe(sid, sub.Max);
            }

            return sid;
        }

        /// <summary>
        /// Unsubscribe using the client connection.
        /// </summary>
        /// <param name="sid">The subscription id to witch to un-subscribe</param>
        /// <param name="optMax">Maximum number of opt</param>
        public void Unsubscribe(int sid, int optMax)
        {
            string str_opt_max = optMax > 0 ? string.Format(CultureInfo.InvariantCulture, " {0}", optMax) : string.Empty;
            this.SendCommand(string.Format(CultureInfo.InvariantCulture, "UNSUB {0}{1}{2}", sid, str_opt_max, Resource.CRLF));
            if (!this.subscriptions.ContainsKey(sid))
            {
                return;
            }
            else
            {
                Subscription sub = this.subscriptions[sid];
                sub.Max = optMax;
                if (sub.Max == 0 || sub.Received >= sub.Max)
                {
                    this.subscriptions.Remove(sid);
                }
            }
        }

        /// <summary>
        /// Unsubscribe using the client connection.
        /// </summary>
        /// <param name="sid">The subscription id of the subscription</param>
        public void Unsubscribe(int sid)
        {
            this.Unsubscribe(sid, 0);
        }

        /// <summary>
        /// Close the client connection. 
        /// </summary>
        public void Stop()
        {
            this.Status = ConnectionStatus.Closing;
            this.CloseConnection();
            this.Status = ConnectionStatus.Closed;
        }

        /// <summary>
        /// Send a request.
        /// </summary>
        /// <param name="subject">The subject of the request</param>
        /// <returns>returns the subscription id</returns>
        public int Request(string subject)
        {
            return Request(subject, null, null, Resource.EMPTYMSG);
        }

        /// <summary>
        /// Send a request and have the response delivered to the supplied callback
        /// </summary>
        /// <param name="subject">the subject </param>
        /// <param name="opts">additional options</param>
        /// <param name="callback">the callback for the response</param>
        /// <param name="data">data for the request</param>
        /// <returns>returns the subscription id</returns>
        public int Request(string subject, Dictionary<string, object> opts, SubscribeCallback callback, string data)
        {
            if (string.IsNullOrEmpty(subject))
            {
                return 0;
            }

            Random rand = new Random();

            string inbox = string.Format(
                                            CultureInfo.InvariantCulture,
                                            "_INBOX.{0:x4}{1:x4}{2:x4}{3:x4}{4:x4}{5:x6}",
                                            rand.Next(0x0010000),
                                            rand.Next(0x0010000),
                                            rand.Next(0x0010000),
                                            rand.Next(0x0010000),
                                            rand.Next(0x0010000),
                                            rand.Next(0x1000000));

            int s = Subscribe(inbox, callback, opts);

            this.Publish(subject, null, data, inbox);

            return s;
        }

        /// <summary>
        /// Call this method to attempt to reconnect the client to the NATS server.
        /// </summary>
        public void AttemptReconnect()
        {
            int reconnectAttempt = 0;
            while (reconnectAttempt < this.reconnectAttempts)
            {
                try
                {
                    this.tcpClient = new TcpClient();
                    this.tcpClient.Connect(this.serverUri.Host, this.serverUri.Port);
                    this.status = ConnectionStatus.Open;
                    break;
                }
                catch (SocketException soketException)
                {
                    Logger.Fatal(soketException.ToString());
                }

                reconnectAttempt++;
                Thread.Sleep(this.reconnectTime);
            }

            if (this.status == ConnectionStatus.Reconnecting)
            {
                this.status = ConnectionStatus.Closed;
                throw new ReactorException(this.serverUri, Resources.Language.ERRCanNotConnect);
            }
            else
            {
                this.Connect();
            }
        }

        /// <summary>
        /// disposes the current object
        /// </summary>
        public void Dispose()
        {
            if (this.tcpClient != null)
            {
                if (this.tcpClient.Connected)
                {
                    this.CloseConnection();
                }
            }

            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Connects this instance.
        /// </summary>
        private void Connect()
        {
            this.readBuffer = new byte[this.tcpClient.ReceiveBufferSize];
            this.tcpClient.GetStream().BeginRead(this.readBuffer, 0, this.readBuffer.Length, this.ReadTCPData, this.tcpClient.GetStream());

            Dictionary<string, object> cs = new Dictionary<string, object>()
            {
               { "verbose", this.verbose },
               { "pedantic", this.pedantic }
            };

            if (!string.IsNullOrEmpty(this.serverUri.UserInfo))
            {
                string[] credentials = this.serverUri.UserInfo.Split(':');
                cs["user"] = credentials.Length > 0 ? credentials[0] : string.Empty;
                cs["pass"] = credentials.Length > 1 ? credentials[1] : string.Empty;
            }

            this.status = ConnectionStatus.Open;
            this.parseState = ParseState.AwaitingControlLine;

            this.SendCommand(string.Format(CultureInfo.InvariantCulture, "CONNECT {0}{1}", JsonConvertibleObject.SerializeToJson(cs), Resource.CRLF));

            lock (this.pendingCommandsLock)
            {
                if (this.pendingCommands.Count > 0)
                {
                    this.FlushPendingCommands();
                }
            }

            if (this.OnError == null)
            {
                this.OnError = new EventHandler<ReactorErrorEventArgs>(delegate(object sender, ReactorErrorEventArgs args)
                    {
                        throw new ReactorException(this.serverUri, args.Message);
                    });
            }

            // if (OnConnect != null && Status != ConnectionStatus.REConecting)
            if (this.OnConnect != null)
            {
                // We will round trip the server here to make sure all state from any pending commands
                // has been processed before calling the connect callback.
                this.QueueServer(delegate
                {
                    this.OnConnect(this, null);
                });
            }
        }

        /// <summary>
        /// Queues the server.
        /// </summary>
        /// <param name="callback">The callback.</param>
        private void QueueServer(SimpleCallback callback)
        {
            if (callback == null)
            {
                return;
            }

            this.pongs.Enqueue(callback);
            this.SendCommand(this.resource.PINGREQUEST);
        }

        /// <summary>
        /// Sends the command to the server.
        /// </summary>
        /// <param name="command">The command.</param>
        private void SendCommand(string command)
        {
            if (this.status != ConnectionStatus.Open)
            {
                lock (this.pendingCommandsLock)
                {
                    if (this.pendingCommands == null)
                    {
                        this.pendingCommands = new List<object>();
                    }
                }

                lock (this.pendingCommandsLock)
                {
                    this.pendingCommands.Add(command);
                }
            }
            else
            {
                this.SendData(command);
            }
        }

        /// <summary>
        /// Flushes the pending commands.
        /// </summary>
        private void FlushPendingCommands()
        {
            lock (this.pendingCommandsLock)
            {
                if (this.pendingCommands.Count == 0)
                {
                    return;
                }

                foreach (object data in this.pendingCommands)
                {
                    this.SendData(data);
                }

                this.pendingCommands.Clear();
            }
        }

        /// <summary>
        /// Reads the TCP data from the NATS server.
        /// </summary>
        /// <param name="result">The result.</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "Unknown type of error received")]
        private void ReadTCPData(IAsyncResult result)
        {
            try
            {
                int bytesRead = 0;
                byte[] localBuffer = null;

                if (((NetworkStream)result.AsyncState).CanRead)
                {
                    bytesRead = ((NetworkStream)result.AsyncState).EndRead(result);
                    localBuffer = new byte[this.readBuffer.Length];
                    Array.Copy(this.readBuffer, localBuffer, this.readBuffer.Length);

                    this.ReceiveData(localBuffer, bytesRead);
                }

                // Connection may have been closed, check again
                if (((NetworkStream)result.AsyncState).CanRead)
                {
                    ((NetworkStream)result.AsyncState).BeginRead(this.readBuffer, 0, this.readBuffer.Length, this.ReadTCPData, ((NetworkStream)result.AsyncState));
                }
            }
            catch (ArgumentNullException argumentNullException)
            {
                Logger.Fatal(Language.ExceptionMessageBufferParameterNull + argumentNullException.ToString());
            }
            catch (ArgumentOutOfRangeException argumentOutOfRangeException)
            {
                Logger.Fatal(Language.ExceptionBufferOffsetOrSizeIncorrect + argumentOutOfRangeException.ToString());
            }
            catch (ArgumentException argumentException)
            {
                Logger.Fatal(Language.ExceptionAsyncResultParameterNull + argumentException.ToString());
            }
            catch (IOException exception)
            {
                Logger.Fatal(Language.ExceptionSocketReadProblem + exception.ToString());
            }
            catch (ObjectDisposedException objectDisposedException)
            {
                Logger.Fatal(Language.ExceptionNetworkStreamClosed + objectDisposedException.ToString());
            }
            catch (Exception ex)
            {
                Logger.Fatal(ex.ToString());
            }
        }

        /// <summary>
        /// Sends data to the NATS Server
        /// </summary>
        /// <param name="data">The data that is sent</param>
        private void SendData(object data)
        {
            ASCIIEncoding ascii = new ASCIIEncoding();
            byte[] objdata = ascii.GetBytes(data.ToString());
            try
            {
                this.tcpClient.GetStream().Write(objdata, 0, objdata.Length);
            }
            catch (System.IO.IOException exception)
            {
                EventLog.WriteEntry(Resources.ClientConnection.EventSource, exception.ToString(), EventLogEntryType.Error);
                this.status = ConnectionStatus.Reconnecting;
                this.AttemptReconnect();
            }
        }

        /// <summary>
        /// Receives the data from the NATS server.
        /// </summary>
        /// <param name="data">The data.</param>
        /// <param name="bytesRead">The bytes read.</param>
        private void ReceiveData(byte[] data, int bytesRead)
        {
            if (this.buf == null)
            {
                this.buf = new List<byte>(bytesRead);
            }

            this.buf.AddRange(data.Take(bytesRead));

            while (this.buf != null)
            {
                switch (this.parseState)
                {
                    case ParseState.AwaitingControlLine:
                        {
                            ASCIIEncoding ascii = new ASCIIEncoding();
                            string strBuffer = ascii.GetString(this.buf.ToArray());

                            if (this.resource.MSG.IsMatch(strBuffer))
                            {
                                Match match = this.resource.MSG.Match(strBuffer);
                                strBuffer = this.resource.MSG.Replace(strBuffer, string.Empty);
                                this.subscriptionId = Convert.ToInt32(match.Groups[2].Value, CultureInfo.InvariantCulture);
                                this.reply = match.Groups[4].Value;
                                this.needed = Convert.ToInt32(match.Groups[5].Value, CultureInfo.InvariantCulture);
                                this.parseState = ParseState.AwaitingMsgPayload;
                            }
                            else if (this.resource.OK.IsMatch(strBuffer))
                            {
                                strBuffer = this.resource.OK.Replace(strBuffer, string.Empty);
                            }
                            else if (this.resource.ERR.IsMatch(strBuffer))
                            {
                                if (this.OnError != null)
                                {
                                    this.OnError(this, new ReactorErrorEventArgs(this.resource.ERR.Match(strBuffer).Groups[1].Value));
                                }

                                strBuffer = this.resource.ERR.Replace(strBuffer, string.Empty);
                            }
                            else if (this.resource.PING.IsMatch(strBuffer))
                            {
                                this.SendCommand(this.resource.PONGRESPONSE);
                                strBuffer = this.resource.PING.Replace(strBuffer, string.Empty);
                            }
                            else if (this.resource.PONG.IsMatch(strBuffer))
                            {
                                if (this.pongs.Count > 0)
                                {
                                    SimpleCallback callback = this.pongs.Dequeue();
                                    callback();
                                }

                                strBuffer = this.resource.PONG.Replace(strBuffer, string.Empty);
                            }
                            else if (this.resource.INFO.IsMatch(strBuffer))
                            {
                                this.serverInfo = JsonConvertibleObject.ObjectToValue<Dictionary<string, object>>(JsonConvertibleObject.DeserializeFromJson(this.resource.INFO.Match(strBuffer).Groups[1].Value));
                                strBuffer = this.resource.INFO.Replace(strBuffer, string.Empty);
                            }
                            else if (this.resource.UNKNOWN.IsMatch(strBuffer))
                            {
                                if (this.OnError != null)
                                {
                                    this.OnError(this, new ReactorErrorEventArgs(Language.ERRUnknownProtocol + this.resource.UNKNOWN.Match(strBuffer).Value));
                                }

                                strBuffer = this.resource.UNKNOWN.Replace(strBuffer, string.Empty);
                            }
                            else
                            {
                                this.buf = ascii.GetBytes(strBuffer).ToList();
                                return;
                            }

                            this.buf = ascii.GetBytes(strBuffer).ToList();
                            if (this.buf != null && this.buf.Count == 0)
                            {
                                this.buf = null;
                            }
                        }

                        break;
                    case ParseState.AwaitingMsgPayload:
                        {
                            if (this.buf.Count < (this.needed + Resource.CRLF.Length))
                            {
                                return;
                            }

                            ASCIIEncoding ascii = new ASCIIEncoding();
                            string strBuffer = ascii.GetString(this.buf.ToArray());

                            this.OnMessage(this.subscriptionId, this.reply, strBuffer.Substring(0, this.needed));

                            strBuffer = strBuffer.Substring(this.needed + Resource.CRLF.Length);

                            this.reply = string.Empty;
                            this.subscriptionId = this.needed = 0;

                            this.parseState = ParseState.AwaitingControlLine;

                            this.buf = ascii.GetBytes(strBuffer).ToList();
                            if (this.buf != null && this.buf.Count == 0)
                            {
                                this.buf = null;
                            }
                        }

                        break;
                }
            }
        }

        /// <summary>
        /// Called when a message is recived.
        /// </summary>
        /// <param name="sid">The subscription ID.</param>
        /// <param name="replyMessage">The reply message.</param>
        /// <param name="msg">The message.</param>
        private void OnMessage(int sid, string replyMessage, string msg)
        {
            if (!this.subscriptions.ContainsKey(sid))
            {
                return;
            }

            Subscription nantsSubscription = this.subscriptions[sid];

            // Check for auto_unsubscribe
            nantsSubscription.Received += 1;

            if (nantsSubscription.Max > 0 && nantsSubscription.Received > nantsSubscription.Max)
            {
                this.Unsubscribe(sid, 0);
            }

            if (nantsSubscription.Callback != null)
            {
                this.messageCallbackFactory.StartNew(
                    () => 
                        {
                            if (!this.subscriptions.ContainsKey(sid))        
                            {
                                return;
                            }

                            nantsSubscription.Callback(msg, replyMessage, nantsSubscription.Subject);
                        });
            }

            // Check for a timeout, and cancel if received >= expected
            if (nantsSubscription.Timeout != null && nantsSubscription.Received >= 0)
            {
                nantsSubscription.Timeout.Enabled = false;
                nantsSubscription.Timeout = null;
            }
        }

        /// <summary>
        /// Closes the connection.
        /// </summary>
        private void CloseConnection()
        {
            this.tcpClient.GetStream().Close();
            this.tcpClient.Close();
        }
    }
}
