using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.Timers;
using System.Text.RegularExpressions;
using System.Diagnostics;
using System.Globalization;
using CloudFoundry.Net.Nats.Exceptions;
using System.IO;
using Uhuru.Utilities;

namespace CloudFoundry.Net.Nats
{
    public delegate void SimpleCallback();
    public delegate void SubscribeCallback(string msg, string reply, string subject);
    public delegate void UnsubscribeCallback(int sid);


    public class Client : IDisposable
    {

        event EventHandler<NatsEventArgs> OnConnect;
        public event EventHandler<NatsEventArgs> OnError;

        Regex MSG = new Regex(@"\AMSG\s+([^\s]+)\s+([^\s]+)\s+(([^\s]+)[^\S\r\n]+)?(\d+)\r\n");
        Regex OK = new Regex(@"\A\+OK\s*\r\n");
        Regex ERR = new Regex(@"\A-ERR\s+('.+')?\r\n");
        Regex PING = new Regex(@"\APING\s*\r\n");
        Regex PONG = new Regex(@"\APONG\s*\r\n");
        Regex INFO = new Regex(@"\AINFO\s+([^\r\n]+)\r\n");
        Regex UNKNOWN = new Regex(@"\A(.*)\r\n");

        const int AWAITING_CONTROL_LINE = 1;
        const int AWAITING_MSG_PAYLOAD = 2;

        const string CR_LF = "\r\n";
        int CR_LF_SIZE = CR_LF.Length;

        string PING_REQUEST = String.Format(CultureInfo.InvariantCulture, "PING{0}", CR_LF);
        string PONG_RESPONSE = String.Format(CultureInfo.InvariantCulture, "PONG{0}", CR_LF);

        const string EMPTY_MSG = "";

        TcpClient client;

        private Dictionary<string, object> serverInfo;

        public Dictionary<string, object> ServerInfo
        {
            get { return serverInfo; }
        }

        bool verbose = false;
        bool pedantic = false;
        bool stopped = false;

        public bool Pedantic
        {
            get { return pedantic; }
            set { pedantic = value; }
        }
        bool reconnect = true;

        int MAX_RECONNECT_ATTEMPTS = 10;
        const int RECONNECT_TIME_WAIT = 2;

        private Uri uri = null;

        public Uri URI
        {
            get { return uri; }
            set { uri = value; }
        }
        private int ssid;

        private Dictionary<int, NatsSub> subs = new Dictionary<int, NatsSub>();

        bool closing = false;

        Queue<SimpleCallback> pongs = new Queue<SimpleCallback>();

        List<object> pending = new List<object>();

        List<byte> buf = new List<byte>();
        int parse_state;
        string subscription;
        int subscriptionId;
        string reply;
        int needed;

        private bool reconnecting;
        private Timer reconnect_timer;
        private  int reconnect_attempts;
        private bool connected;

        public void Start(Uri serverUri)
        {
            if (serverUri == null)
            {
                throw new ArgumentNullException("serverUri");
            }
            Connect(serverUri.ToString());
        }

        public void Start(string serverUri)
        {
            Start(new Uri(serverUri));
        }

        public void Start()
        {
            Connect(uri.ToString());
            stopped = false;
        }

        byte[] readBuffer;

        void Connect(string serverUri)
        {
            uri = new Uri(serverUri);
            client = new TcpClient();
            client.Connect(uri.Host, uri.Port);
            
            connection_completed();
            readBuffer = new byte[client.ReceiveBufferSize];


            send_connect_command();
            
            client.GetStream().BeginRead(readBuffer, 0, readBuffer.Length, ReadTCPData, client.GetStream());
        }

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
                    receive_data(localBuffer, bytesRead);
                }

                // Connection may have been closed, check again
                if (((NetworkStream)result.AsyncState).CanRead)
                {
                    ((NetworkStream)result.AsyncState).BeginRead(readBuffer, 0, readBuffer.Length, ReadTCPData, ((NetworkStream)result.AsyncState));
                }
            }
            catch (Exception ex)
            {
                EventLog.WriteEntry("NatsClient", ex.ToString(), EventLogEntryType.Error);
            }
        }

        private bool ServerRunning
        {
            get
            {
                try
                {
                    TcpClient tcpClient = new TcpClient(uri.Host, uri.Port);
                    tcpClient.Close();
                    return true;
                }
                catch(Exception)
                {
                    return false;
                }
            }
        }

        public bool Connected
        {
            get
            {
                return client.Connected;
            }
        }

        public void Publish(string subject)
        {
            Publish(subject, null, EMPTY_MSG, null);
        }
      
        public void Publish(string subject, SimpleCallback callback = null, string msg = EMPTY_MSG, string optReply = null)
        {
            if (msg == null)
            {
                throw new ArgumentNullException("msg");
            }

            if (String.IsNullOrEmpty(subject))
            {
                return;
            }
            send_command(String.Format(CultureInfo.InvariantCulture, "PUB {0} {1} {2}{3}{4}{5}", subject, optReply, msg.Length, CR_LF, msg, CR_LF));
            if (callback != null)
            {
                queue_server_rt(callback);
            }
        }

        public int Subscribe(string subject, SubscribeCallback callback = null, Dictionary<string, object> opts = null)
        {
            if (String.IsNullOrEmpty(subject))
            {
                return 0;
            }
            int sid = (ssid += 1);

            if (opts == null)
            {
                opts = new Dictionary<string, object>();
            }

            NatsSub sub = subs[sid] = new NatsSub()
            {
                Subject = subject,
                Callback = callback,
                Received = 0,
                Queue = opts.ContainsKey("queue") ? Convert.ToInt32(opts["queue"], CultureInfo.InvariantCulture) : 0,
                Max = opts.ContainsKey("max") ? Convert.ToInt32(opts["max"], CultureInfo.InvariantCulture) : 0
            };

            send_command(String.Format(CultureInfo.InvariantCulture, "SUB {0} {1} {2}{3}", subject, sub.Queue == 0 ? "" : sub.Queue.ToString(CultureInfo.InvariantCulture), sid, CR_LF));

            if (sub.Max > 0)
            {
                Unsubscribe(sid, sub.Max);
            }
            return sid;
        }

        public void Unsubscribe(int sid)
        {
            Unsubscribe(sid, 0);
        }

        public void Unsubscribe(int sid, int optMax)
        {
            string str_opt_max = optMax > 0 ? String.Format(CultureInfo.InvariantCulture, " {0}", optMax) : "";
            send_command(String.Format(CultureInfo.InvariantCulture, "UNSUB {0}{1}{2}", sid, str_opt_max, CR_LF));
            if (!subs.ContainsKey(sid))
            {
                return;
            }
            else
            {
                NatsSub sub = subs[sid];
                sub.Max = optMax;
                if (sub.Max == 0 || sub.Received >= sub.Max)
                {
                    subs.Remove(sid);
                }
            }
        }

        private void timeout(int sid, int timeout, Dictionary<string, object> opts, UnsubscribeCallback callback)
        {
            // Setup a timeout if requested
            if (!subs.ContainsKey(sid))
            {
                return;
            }

            NatsSub sub = subs[sid];

            bool auto_unsubscribe = true;
            int expected = 1;

            auto_unsubscribe = opts.ContainsKey("autounsubscribe") ? (bool)opts["autounsubscribe"] : auto_unsubscribe;
            expected = opts.ContainsKey("expected") ? (int)opts["expected"] : expected;


            if (sub.Timeout != null)
            {
                sub.Timeout.Enabled = false;
            }

            sub.Timeout = new Timer(timeout);
            sub.Timeout.AutoReset = false;
            sub.Timeout.Elapsed += new ElapsedEventHandler(delegate(object sender, ElapsedEventArgs args)
            {
                if (auto_unsubscribe)
                {
                    Unsubscribe(sid);
                    if (callback != null)
                    {
                        callback(sid);
                    }
                }
            });
            sub.Expected = expected;
        }

        private static string create_inbox()
        {
            Random rand = new Random();

            return String.Format(CultureInfo.InvariantCulture, "_INBOX.{0:x4}{1:x4}{2:x4}{3:x4}{4:x4}{5:x6}",
                rand.Next(0x0010000), rand.Next(0x0010000), rand.Next(0x0010000),
                rand.Next(0x0010000), rand.Next(0x0010000), rand.Next(0x1000000));
        }

        public int Request(string subject, Dictionary<string, object> opts = null, SubscribeCallback callback = null, string data = EMPTY_MSG)
        {
            if (String.IsNullOrEmpty(subject))
            {
                return 0;
            }

            string inbox = create_inbox();

            int s = Subscribe(inbox, callback, opts);

            Publish(subject, null, data, inbox);

            return s;
        }

        public void Stop()
        {
            Close();
            stopped = true;
        }

        private void Close()
        {
            closing = true;
            close_connection_after_writing();
        }

        private bool user_err_cb
        {
            get
            {
                return OnError != null;
            }
        }

        private void send_connect_command()
        {
            Dictionary<string, object> cs = new Dictionary<string, object>() { 
               {"verbose", verbose},
               {"pedantic", pedantic}
           };

            if (!String.IsNullOrEmpty(uri.UserInfo))
            {
                string[] pieces = uri.UserInfo.Split(':');

                cs["user"] = pieces.Length > 0 ? pieces[0] : String.Empty;
                cs["pass"] = pieces.Length > 1 ? pieces[1] : String.Empty;
            }

            send_command(String.Format(CultureInfo.InvariantCulture, "CONNECT {0}{1}", cs.ToJson(), CR_LF));
        }

        private void queue_server_rt(SimpleCallback callback)
        {
            if (callback == null)
            {
                return;
            }
            pongs.Enqueue(callback);
            send_command(PING_REQUEST);
        }

        private void on_msg(string subject, int sid, string replyMessage, string msg)
        {
            if (!subs.ContainsKey(sid))
            {
                return;
            }
            NatsSub sub = subs[sid];

            // Check for auto_unsubscribe
            sub.Received += 1;

            if (sub.Max > 0 && sub.Received > sub.Max)
            {
                Unsubscribe(sid);
                return;
            }

            if (sub.Callback != null)
            {
                sub.Callback(msg, replyMessage, subject);
            }

            // Check for a timeout, and cancel if received >= expected
            if (sub.Timeout != null && sub.Received >= sub.Expected)
            {
                sub.Timeout.Enabled = false;
                sub.Timeout = null;
            }
        }

        private void flush_pending()
        {
            if (pending.Count == 0)
            {
                return;
            }

            foreach (object data in pending)
            {
                send_data(data);
            }

            pending.Clear();
        }

        private void receive_data(byte[] data, int bytesRead)
        {
            if (buf == null)
            {
                buf = new List<byte>();
            }
            byte[] localBuf = new byte[bytesRead];
            Array.Copy(data, localBuf, bytesRead);
            buf.AddRange(localBuf);

            while (buf != null)
            {
                switch (parse_state)
                {
                    case AWAITING_CONTROL_LINE:
                        {
                            ASCIIEncoding ascii = new ASCIIEncoding();
                            string strBuffer = ascii.GetString(buf.ToArray());

                            if (MSG.IsMatch(strBuffer))
                            {
                                Match match = MSG.Match(strBuffer);
                                strBuffer = MSG.Replace(strBuffer, "");
                                subscription = match.Groups[1].Value;
                                subscriptionId = Convert.ToInt32(match.Groups[2].Value, CultureInfo.InvariantCulture);
                                reply = match.Groups[4].Value;
                                needed = Convert.ToInt32(match.Groups[5].Value, CultureInfo.InvariantCulture);
                                parse_state = AWAITING_MSG_PAYLOAD;
                            }
                            else if (OK.IsMatch(strBuffer))
                            {
                                strBuffer = OK.Replace(strBuffer, "");
                            }
                            else if (ERR.IsMatch(strBuffer))
                            {
                                if (user_err_cb)
                                {
                                    OnError(this, new NatsEventArgs(ERR.Match(strBuffer).Groups[1].Value));
                                }
                                strBuffer = ERR.Replace(strBuffer, "");
                            }
                            else if (PING.IsMatch(strBuffer))
                            {
                                send_command(PONG_RESPONSE);
                                strBuffer = PING.Replace(strBuffer, "");
                            }
                            else if (PONG.IsMatch(strBuffer))
                            {
                                if (pongs.Count > 0)
                                {
                                    SimpleCallback callback = pongs.Dequeue();
                                    callback();
                                }
                                strBuffer = PONG.Replace(strBuffer, "");
                            }
                            else if (INFO.IsMatch(strBuffer))
                            {
                                process_info(INFO.Match(strBuffer).Groups[1].Value);
                                strBuffer = INFO.Replace(strBuffer, "");
                            }
                            else if (UNKNOWN.IsMatch(strBuffer))
                            {
                                if (user_err_cb)
                                {
                                    OnError(this, new NatsEventArgs("Unknown protocol:" + UNKNOWN.Match(strBuffer).Value));
                                }
                                strBuffer = UNKNOWN.Replace(strBuffer, "");
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
                    case AWAITING_MSG_PAYLOAD:
                        {
                            if (buf.Count < (needed + CR_LF_SIZE))
                            {
                                return;
                            }

                            ASCIIEncoding ascii = new ASCIIEncoding();
                            string strBuffer = ascii.GetString(buf.ToArray());

                            on_msg(subscription, subscriptionId, reply, strBuffer.Substring(0, needed));

                            strBuffer = strBuffer.Substring(needed + CR_LF_SIZE);

                            subscription = reply = String.Empty;
                            subscriptionId = needed = 0;

                            parse_state = AWAITING_CONTROL_LINE;

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

        private void process_info(string info)
        {
            serverInfo = serverInfo.FromJson(info);
        }

        private void connection_completed()
        {
            connected = true;
            if (reconnecting)
            {
                reconnect_timer.Enabled = false;
                send_connect_command();
                foreach (KeyValuePair<int, NatsSub> kvp in subs)
                {
                    send_command(String.Format(CultureInfo.InvariantCulture, "SUB {0} {1}{2}", kvp.Value.Subject, kvp.Key, CR_LF));
                }
            }

            if (pending != null && pending.Count > 0)
            {
                flush_pending();
            }


            if (user_err_cb == false && !reconnecting)
            {
                OnError = new EventHandler<NatsEventArgs>(delegate(object sender, NatsEventArgs args)
                    {
                        throw new NatsException(this, args.Message);
                    });
            }

            if (OnConnect != null && !reconnecting)
            {
                // We will round trip the server here to make sure all state from any pending commands
                // has been processed before calling the connect callback.
                queue_server_rt(delegate()
                {
                    OnConnect(this, null);
                });
            }
            reconnecting = false;
            parse_state = AWAITING_CONTROL_LINE;
        }

        private void schedule_reconnect(int wait = RECONNECT_TIME_WAIT)
        {
            reconnecting = true;
            reconnect_attempts = 0;
            reconnect_timer = new Timer(wait);
            reconnect_timer.Elapsed +=new ElapsedEventHandler(delegate(object sender, ElapsedEventArgs args)
                {
                    attempt_reconnect();
                });
        }

        private void unbind()
        {
            if (connected && !closing && !reconnecting && reconnect)
            {
                schedule_reconnect();
            }
            else
            {
                if (!reconnecting)
                {
                    process_disconnect();
                }
            }
        }

        private bool process_disconnect()
        {
            try
            {
                if (!closing && user_err_cb)
                {
                    string err_string = connected ? String.Format(CultureInfo.InvariantCulture, "Client disconnected from server on {0}.", uri) : String.Format(CultureInfo.InvariantCulture, "Could not connect to server on {0}", uri);
                    OnError(this, new NatsEventArgs(err_string));
                }
            }
            finally
            {
                if (reconnect_timer != null)
                {
                    reconnect_timer.Enabled = false;
                }
                connected = reconnecting = false;
            }
            return true;
        }

        private void attempt_reconnect()
        {
            reconnect_attempts += 1;
            if (reconnect_attempts > MAX_RECONNECT_ATTEMPTS)
            {
                process_disconnect();
                return;
            }
            client.Connect(uri.Host, uri.Port);
        }

        private void send_data(object data)
        {
            ASCIIEncoding ascii = new ASCIIEncoding();
            byte[] objdata = ascii.GetBytes(data.ToString());
            client.GetStream().Write(objdata, 0, objdata.Length);
        }

        private void close_connection_after_writing()
        {
            //todo: vladi: make this thread safe, and make sure no other requests can come in after this is closed
            client.GetStream().Close();
            client.Close();
        }

        private void send_command(string command)
        {
            if (!Connected)
            {
                queue_command(command);
            }
            else
            {
                send_data(command);
            }
        }

        private bool queue_command(string command)
        {
            if (pending == null)
            {
                pending = new List<object>();
            }
            pending.Add(command);
            return true;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (!stopped)
                {
                    this.Stop();
                    reconnect_timer.Close();
                }
            }
        }
    }
}
