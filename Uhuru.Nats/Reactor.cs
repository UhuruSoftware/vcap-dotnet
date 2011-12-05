// -----------------------------------------------------------------------
// <copyright file="Reactor.cs" company="Uhuru Software">
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
    using Uhuru.NatsClient.Resources;
    using Uhuru.Utilities;
    
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
    /// Delegate definition for a method to be called when an unsubscription is complete.
    /// </summary>
    /// <param name="sid">Subscription id.</param>
    public delegate void UnsubscribeCallback(int sid);

    /// <summary>
    /// This class is the NATS client, it is used to communicate to the NATS server.
    /// The reactor enables pub/sub style communication.
    /// </summary>
    public sealed class Reactor : IDisposable
    {
        private Dictionary<int, Subscription> subscriptions = new Dictionary<int, Subscription>();
        private ParseState parseState;
        private bool pedantic;
        private bool verbose;
        private Dictionary<string, object> serverInfo;
        private byte[] readBuffer;
        private TcpClient tcpClient;
        private Uri serverUri;
        private ConnectionStatus status = ConnectionStatus.Closed;
        private List<object> pendingCommands = new List<object>();
        private Queue<SimpleCallback> pongs = new Queue<SimpleCallback>();
        private Resource resource = Resource.Instance;
        private int ssid = 0;
        private int reconnectAttempts = 10;
        private int reconnectTime = 10000;
        
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
            Start(new Uri(uri));  
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

            this.serverUri = uri;

            this.tcpClient = new TcpClient();
            this.tcpClient.Connect(serverUri.Host, serverUri.Port);

            Connect();
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
            if (msg == null)
            {
                throw new ArgumentNullException("msg");
            }

            if (string.IsNullOrEmpty(subject))
            {
                return;
            }

            this.SendCommand(string.Format(CultureInfo.InvariantCulture, "PUB {0} {1} {2}{3}{4}{5}", subject, optReply, msg.Length, Resource.CR_LF, msg, Resource.CR_LF));
            if (callback != null)
            {
                this.QueueServer(callback);
            }
        }

        /// <summary>
        /// Publish a message to a given subject, with optional reply subject and completion block. 
        /// </summary>
        /// <param name="subject">the subject to publish to</param>
        public void Publish(string subject)
        {
            Publish(subject, null, Resource.EMPTY_MSG, null);
        }

        /// <summary>
        /// Publish a message to a given subject, with optional reply subject and completion block. 
        /// </summary>
        /// <param name="subject">the subject to publish to</param>
        /// <param name="callback">server callback</param>
        public void Publish(string subject, SimpleCallback callback)
        {
            Publish(subject, callback, Resource.EMPTY_MSG, null);
        }

        /// <summary>
        /// Publish a message to a given subject, with optional reply subject and completion block. 
        /// </summary>
        /// <param name="subject">the subject to publish to</param>
        /// <param name="callback">server callback</param>
        /// <param name="msg">the message to publish</param>
        public void Publish(string subject, SimpleCallback callback, string msg)
        {
            Publish(subject, callback, msg, null);
        }

        /// <summary>
        /// Subscribe using the client connection.
        /// </summary>
        public int Subscribe(string subject)
        {
            return Subscribe(subject, null, null);
        }

        /// <summary>
        /// Subscribe using the client connection.
        /// </summary>
        public int Subscribe(string subject, SubscribeCallback callback)
        {
            return Subscribe(subject, callback, null);
        }

        /// <summary>
        /// Subscribe using the client connection.
        /// </summary>
        public int Subscribe(string subject, SubscribeCallback callback, Dictionary<string, object> opts)
        {
            if (string.IsNullOrEmpty(subject))
            {
                return 0;
            }

            int sid = (this.ssid += 1);

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

            this.SendCommand(string.Format(CultureInfo.InvariantCulture, "SUB {0} {1} {2}{3}", subject, sub.Queue == 0 ? "" : sub.Queue.ToString(CultureInfo.InvariantCulture), sid, Resource.CR_LF));

            if (sub.Max > 0)
            {
                Unsubscribe(sid, sub.Max);
            }

            return sid;
        }

        /// <summary>
        /// Unsubscribe using the client connection.
        /// </summary>
        public void Unsubscribe(int sid, int optMax)
        {
            string str_opt_max = optMax > 0 ? string.Format(CultureInfo.InvariantCulture, " {0}", optMax) : "";
            SendCommand(string.Format(CultureInfo.InvariantCulture, "UNSUB {0}{1}{2}", sid, str_opt_max, Resource.CR_LF));
            if (!subscriptions.ContainsKey(sid))
            {
                return;
            }
            else
            {
                Subscription sub = subscriptions[sid];
                sub.Max = optMax;
                if (sub.Max == 0 || sub.Received >= sub.Max)
                {
                    subscriptions.Remove(sid);
                }
            }
        }

        /// <summary>
        /// Unsubscribe using the client connection.
        /// </summary>
        public void Unsubscribe(int sid)
        {
            Unsubscribe(sid, 0);
        }

        /// <summary>
        /// Close the client connection. 
        /// </summary>
        public void Stop()
        {
            Status = ConnectionStatus.Closing;
            CloseConnection();
            Status = ConnectionStatus.Closed;
        }

        /// <summary>
        /// Send a request.
        /// </summary>
        /// <param name="subject"></param>
        /// <returns></returns>
        public int Request(string subject)
        {
            return Request(subject, null, null, Resource.EMPTY_MSG);
        }

        /// <summary>
        /// Send a request and have the response delivered to the supplied callback
        /// </summary>
        /// <param name="subject">the subject </param>
        /// <param name="opts">additional options</param>
        /// <param name="callback">the callback for the response</param>
        /// <param name="data">data for the requset</param>
        /// <returns></returns>
        public int Request(string subject, Dictionary<string, object> opts, SubscribeCallback callback, string data)
        {
            if (string.IsNullOrEmpty(subject))
            {
                return 0;
            }

            Random rand = new Random();

            string inbox = string.Format(CultureInfo.InvariantCulture, 
                                            "_INBOX.{0:x4}{1:x4}{2:x4}{3:x4}{4:x4}{5:x6}",
                                            rand.Next(0x0010000), 
                                            rand.Next(0x0010000), 
                                            rand.Next(0x0010000),
                                            rand.Next(0x0010000), 
                                            rand.Next(0x0010000), 
                                            rand.Next(0x1000000));

            int s = Subscribe(inbox, callback, opts);

            Publish(subject, null, data, inbox);

            return s;
        }

        /// <summary>
        /// Call this method to attempt to reconnect the client to the NATS server.
        /// </summary>
        public void AttemptReconnect()
        {
            int reconnectAttempt = 0;
            while (reconnectAttempt < reconnectAttempts)
            {
                try
                {
                    tcpClient = new TcpClient();
                    tcpClient.Connect(serverUri.Host, serverUri.Port);
                    status = ConnectionStatus.Open;
                    break;
                }
                catch (SocketException soketException)
                {
                    EventLog.WriteEntry(Resources.ClientConnection.EventSource, soketException.ToString(), EventLogEntryType.Error);
                }

                reconnectAttempt++;
                Thread.Sleep(reconnectTime);
            }
            if (status == ConnectionStatus.Reconnecting)
            {
                status = ConnectionStatus.Closed;
                throw new ReactorException(serverUri, Resources.Language.ERRCanNotConnect);
            }
            else
            {
                Connect();
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
                    CloseConnection();
                }
            }

            GC.SuppressFinalize(this);
        }
       
        private void Connect()
        {
            this.readBuffer = new byte[this.tcpClient.ReceiveBufferSize];
            this.tcpClient.GetStream().BeginRead(this.readBuffer, 0, this.readBuffer.Length, this.ReadTCPData, this.tcpClient.GetStream());

            Dictionary<string, object> cs = new Dictionary<string, object>()
            {
               {"verbose", this.verbose},
               {"pedantic", this.pedantic}
            };

            if (!String.IsNullOrEmpty(serverUri.UserInfo))
            {
                string[] credentials = serverUri.UserInfo.Split(':');
                cs["user"] = credentials.Length > 0 ? credentials[0] : String.Empty;
                cs["pass"] = credentials.Length > 1 ? credentials[1] : String.Empty;
            }

            Status = ConnectionStatus.Open;
            parseState = ParseState.AwaitingControlLine;

            SendCommand(String.Format(CultureInfo.InvariantCulture, "CONNECT {0}{1}", JsonConvertibleObject.SerializeToJson(cs), Resource.CR_LF));

            if (pendingCommands.Count > 0)
            {
                FlushPendingCommands();
            }

            if (OnError == null)
            {
                OnError = new EventHandler<ReactorErrorEventArgs>(delegate(object sender, ReactorErrorEventArgs args)
                    {
                        throw new ReactorException(serverUri, args.Message);
                    });
            }

            // if (OnConnect != null && Status != ConnectionStatus.REConecting)
            if (OnConnect != null)
            {
                // We will round trip the server here to make sure all state from any pending commands
                // has been processed before calling the connect callback.
                QueueServer(delegate()
                {
                    OnConnect(this, null);
                });
            }
        }

        private void QueueServer(SimpleCallback callback)
        {
            if (callback == null)
            {
                return;
            }
            pongs.Enqueue(callback);
            SendCommand(resource.PING_REQUEST);
        }

        private void SendCommand(string command)
        {
            if (status != ConnectionStatus.Open)
            {
                if (pendingCommands == null)
                {
                    pendingCommands = new List<object>();
                }

                pendingCommands.Add(command);
            }
            else
            {
                SendData(command);
            }
        }

        private void FlushPendingCommands()
        {
            if (pendingCommands.Count == 0)
            {
                return;
            }

            foreach (object data in pendingCommands)
            {
                SendData(data);
            }

            pendingCommands.Clear();
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        private void ReadTCPData(IAsyncResult result)
        {
            try
            {
                int bytesRead = 0;
                byte[] localBuffer = null;

                if (((NetworkStream)result.AsyncState).CanRead)
                {
                    bytesRead = ((NetworkStream)result.AsyncState).EndRead(result);
                    localBuffer = new byte[readBuffer.Length];
                    Array.Copy(readBuffer, localBuffer, readBuffer.Length);

                    ReceiveData(localBuffer, bytesRead);
                }

                // Connection may have been closed, check again
                if (((NetworkStream)result.AsyncState).CanRead)
                {
                    ((NetworkStream)result.AsyncState).BeginRead(readBuffer, 0, readBuffer.Length, ReadTCPData, ((NetworkStream)result.AsyncState));
                }
            }
            catch (ArgumentNullException argumentNullException)
            {
                EventLog.WriteEntry(Resources.ClientConnection.EventSource,  
                                    Language.ExceptionMessageBufferParameterNull + argumentNullException.ToString(), 
                                    EventLogEntryType.Error);
            }
            catch (ArgumentOutOfRangeException argumentOutOfRangeException)
            {
                EventLog.WriteEntry(Resources.ClientConnection.EventSource, 
                                    Language.ExceptionBufferOffsetOrSizeIncorrect + argumentOutOfRangeException.ToString(), 
                                    EventLogEntryType.Error);
            }
            catch (ArgumentException argumentException)
            {
                EventLog.WriteEntry(Resources.ClientConnection.EventSource, 
                                    Language.ExceptionAsyncResultParameterNull + argumentException.ToString(), 
                                    EventLogEntryType.Error);
            }
            catch (IOException exception)
            {
                EventLog.WriteEntry(Resources.ClientConnection.EventSource, 
                                    Language.ExceptionSocketReadProblem + exception.ToString(), 
                                    EventLogEntryType.Error);
            }
            catch (ObjectDisposedException objectDisposedException)
            {
                EventLog.WriteEntry(Resources.ClientConnection.EventSource, 
                                    Language.ExceptionNetworkStreamClosed + objectDisposedException.ToString(), 
                                    EventLogEntryType.Error);
            }
            catch (Exception ex)
            {
                EventLog.WriteEntry(Resources.ClientConnection.EventSource, ex.ToString(), EventLogEntryType.Error);
            }
        }
       
        private void SendData(object data)
        {
            ASCIIEncoding ascii = new ASCIIEncoding();
            byte[] objdata = ascii.GetBytes(data.ToString());
            try
            {
                tcpClient.GetStream().Write(objdata, 0, objdata.Length);
            }
            catch (System.IO.IOException exception)
            {
                EventLog.WriteEntry(Resources.ClientConnection.EventSource, exception.ToString(), EventLogEntryType.Error);
                this.status = ConnectionStatus.Reconnecting;
                this.AttemptReconnect();
            }
        }
        
        private void ReceiveData(byte[] data, int bytesRead)
        {
            List<byte> buf = new List<byte>();
            int subscriptionId=0;
            int needed =0;
            string reply=string.Empty;

            if (buf == null)
            {
                buf = new List<byte>();
            }

            byte[] localBuf = new byte[bytesRead];
            Array.Copy(data, localBuf, bytesRead);
            buf.AddRange(localBuf);

            while (buf != null)
            {
                switch (parseState)
                {
                    case ParseState.AwaitingControlLine:
                        {
                            ASCIIEncoding ascii = new ASCIIEncoding();
                            string strBuffer = ascii.GetString(buf.ToArray());

                            if (resource.MSG.IsMatch(strBuffer))
                            {
                                Match match = resource.MSG.Match(strBuffer);
                                strBuffer = resource.MSG.Replace(strBuffer, "");
                                subscriptionId = Convert.ToInt32(match.Groups[2].Value, CultureInfo.InvariantCulture);
                                reply = match.Groups[4].Value;
                                needed = Convert.ToInt32(match.Groups[5].Value, CultureInfo.InvariantCulture);
                                parseState = ParseState.AwaitingMsgPayload;
                            }
                            else if (resource.OK.IsMatch(strBuffer))
                            {
                                strBuffer = resource.OK.Replace(strBuffer, "");
                            }
                            else if (resource.ERR.IsMatch(strBuffer))
                            {
                                if (OnError != null)
                                {
                                   OnError(this, new ReactorErrorEventArgs(resource.ERR.Match(strBuffer).Groups[1].Value));
                                }

                                strBuffer = resource.ERR.Replace(strBuffer, "");
                            }
                            else if (resource.PING.IsMatch(strBuffer))
                            {
                                SendCommand(resource.PONG_RESPONSE);
                                strBuffer = resource.PING.Replace(strBuffer, "");
                            }
                            else if (resource.PONG.IsMatch(strBuffer))
                            {
                                if (pongs.Count > 0)
                                {
                                    SimpleCallback callback = pongs.Dequeue();
                                    callback();
                                }

                                strBuffer = resource.PONG.Replace(strBuffer, "");
                            }
                            else if (resource.INFO.IsMatch(strBuffer))
                            {
                                serverInfo = JsonConvertibleObject.ObjectToValue<Dictionary<string, object>>(JsonConvertibleObject.DeserializeFromJson(resource.INFO.Match(strBuffer).Groups[1].Value));
                                strBuffer = resource.INFO.Replace(strBuffer, "");
                            }
                            else if (resource.UNKNOWN.IsMatch(strBuffer))
                            {
                                if (OnError != null)
                                {
                                    OnError(this,new ReactorErrorEventArgs(Language.ERRUnknownProtocol + resource.UNKNOWN.Match(strBuffer).Value));
                                }

                                strBuffer =  resource.UNKNOWN.Replace(strBuffer, "");
                            }
                            else
                            {
                                buf = ascii.GetBytes(strBuffer).ToList();
                                return;
                            }

                            buf = ascii.GetBytes(strBuffer).ToList();
                            if (buf != null && buf.Count == 0)
                            {
                                buf = null;
                            }
                        }
                        break;
                    case ParseState.AwaitingMsgPayload:
                        {
                            if (buf.Count < (needed +  Resource.CR_LF.Length))
                            {
                                return;
                            }

                            ASCIIEncoding ascii = new ASCIIEncoding();
                            string strBuffer = ascii.GetString(buf.ToArray());

                            OnMessage(subscriptionId, reply, strBuffer.Substring(0, needed));

                            strBuffer = strBuffer.Substring(needed + Resource.CR_LF.Length);

                            reply = string.Empty;
                            subscriptionId = needed = 0;

                            parseState = ParseState.AwaitingControlLine;

                            buf = ascii.GetBytes(strBuffer).ToList();
                            if (buf != null && buf.Count == 0)
                            {
                                buf = null;
                            }
                        }
                        break;
                }
            }
        }

        private void OnMessage(int sid, string replyMessage, string msg)
        {
            if (!this.subscriptions.ContainsKey(sid))
            {
                return;
            }

            Subscription nantsSubscription = subscriptions[sid];

            // Check for auto_unsubscribe
            nantsSubscription.Received += 1;
            
            if (nantsSubscription.Max > 0 && nantsSubscription.Received > nantsSubscription.Max)
            {
                Unsubscribe(sid,0);
                return;
            }

            if (nantsSubscription.Callback != null)
            {
                nantsSubscription.Callback(msg, replyMessage, nantsSubscription.Subject);
            }

            // Check for a timeout, and cancel if received >= expected
            if (nantsSubscription.Timeout != null && nantsSubscription.Received >= 0) //>= nantsSubscription.Expected)
            {
                nantsSubscription.Timeout.Enabled = false;
                nantsSubscription.Timeout = null;
            }
        }

        private void CloseConnection()
        {
            this.tcpClient.GetStream().Close();
            this.tcpClient.Close();
        }
    }
}
