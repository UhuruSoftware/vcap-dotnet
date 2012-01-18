using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading;
using Uhuru.NatsClient;
using System.Configuration;

namespace Uhuru.CloudFoundry.Test.Integration
{
    [TestClass]
    public class NatsClientTest
    {
        static String natsEndpoint = ConfigurationManager.AppSettings["nats"];

        //[ClassInitialize]
        //public static void TestFixtureSetup()
        //{
        //    natsEndpoint = new Uri();
        //}

        [TestMethod, Description("should perform basic block start and stop")]
        [TestCategory("Integration")]
        public void TC001_StartStopClient()
        {
            using (Reactor natsClient = new Reactor())
            {
                natsClient.Start(natsEndpoint);
                natsClient.Stop();
            }
        }

        [TestMethod, Description("should signal connected state")]
        [TestCategory("Integration")]
        public void TC002_CheckState()
        {
            using (Reactor natsClient = new Reactor())
            {
                natsClient.Start(natsEndpoint);
                Assert.IsTrue(natsClient.Status == ConnectionStatus.Open);
                natsClient.Stop();
            }
        }

        [TestMethod, Description("should be able to reconnect")]
        [TestCategory("Integration")]
        public void TC003_CheckReconnect()
        {
            Reactor natsClient;
            using (natsClient = new Reactor())
            {
                natsClient.Start(natsEndpoint);
                Assert.IsTrue(natsClient.Status == ConnectionStatus.Open);
                natsClient.Stop();
            }

            using (natsClient = new Reactor())
            {
                natsClient.Start(natsEndpoint);
                Assert.IsTrue(natsClient.Status == ConnectionStatus.Open);
                natsClient.Stop();
            }
        }

        [TestMethod, Description("should raise NATS::ServerError on error replies from NATSD")]
        [TestCategory("Integration")]
        public void TC004_CheckServerError()
        {
            bool errorThrown = false;
            AutoResetEvent resetEvent = new AutoResetEvent(false);

            using (Reactor natsClient = new Reactor())
            {
                natsClient.Pedantic = true;
                natsClient.OnError += new EventHandler<ReactorErrorEventArgs>(delegate(object sender, ReactorErrorEventArgs args)
                {
                    errorThrown = true;
                    resetEvent.Set();
                });
                natsClient.Start(natsEndpoint);
                natsClient.Unsubscribe(10000);
                natsClient.Publish("done");
                resetEvent.WaitOne(10000);
                natsClient.Stop();
            }
            Assert.IsTrue(errorThrown);
        }

        [TestMethod, Description("should do publish without payload and with opt_reply without error")]
        [TestCategory("Integration")]
        public void TC005_PublishWithoudPayload()
        {
            bool errorThrown = false;
            AutoResetEvent resetEvent = new AutoResetEvent(false);

            using (Reactor natsClient = new Reactor())
            {
                natsClient.OnError += new EventHandler<ReactorErrorEventArgs>(delegate(object sender, ReactorErrorEventArgs args)
                {
                    errorThrown = true;
                    resetEvent.Set();
                });

                natsClient.Start(natsEndpoint);

                natsClient.Publish("foo");
                natsClient.Publish("foo", null, "hello");
                natsClient.Publish("foo", null, "hello", "reply");

                resetEvent.WaitOne(5000);

                natsClient.Stop();
            }
            Assert.IsFalse(errorThrown);
        }

        [TestMethod, Description("should not complain when publishing to nil")]
        [TestCategory("Integration")]
        public void TC006_PublishNil()
        {
            bool errorThrown = false;
            AutoResetEvent resetEvent = new AutoResetEvent(false);

            using (Reactor natsClient = new Reactor())
            {
                natsClient.OnError += new EventHandler<ReactorErrorEventArgs>(delegate(object sender, ReactorErrorEventArgs args)
                {
                    errorThrown = true;
                    resetEvent.Set();
                });

                natsClient.Start(natsEndpoint);

                // todo: vladi: check to see if nil in Ruby evaluates to boolean true
                natsClient.Publish(null);
                natsClient.Publish(null, null, "hello");

                resetEvent.WaitOne(5000);

                natsClient.Stop();
            }
            Assert.IsFalse(errorThrown);
        }

        [TestMethod, Description("should receive a sid when doing a subscribe")]
        [TestCategory("Integration")]
        public void TC007_ReciveSidSubscribe()
        {
            using (Reactor natsClient = new Reactor())
            {
                natsClient.Start(natsEndpoint);
                int mySid = natsClient.Subscribe("foo");
                natsClient.Stop();
                Assert.IsTrue(0 < mySid);
            }
        }

        [TestMethod, Description("should receive a sid when doing a request")]
        [TestCategory("Integration")]
        public void TC008_ReciveSidRequest()
        {
            using (Reactor natsClient = new Reactor())
            {
                natsClient.Start(natsEndpoint);
                int mySid = natsClient.Request("foo");
                natsClient.Stop();
                Assert.IsTrue(0 < mySid);
            }
        }

        [TestMethod, Description("should receive a message that it has a subscription to")]
        [TestCategory("Integration")]
        public void TC009_ReciveMessageForSubscription()
        {
            bool errorThrown = false;
            string receivedMessage = "";
            AutoResetEvent resetEvent = new AutoResetEvent(false);

            using (Reactor natsClient = new Reactor())
            {
                natsClient.OnError += new EventHandler<ReactorErrorEventArgs>(delegate(object sender, ReactorErrorEventArgs args)
                {
                    errorThrown = true;
                    resetEvent.Set();
                });

                natsClient.Start(natsEndpoint);

                natsClient.Subscribe("foo", delegate(string msg, string reply, string subject)
                {
                    receivedMessage = msg;
                    resetEvent.Set();
                });

                natsClient.Publish("foo", null, "xxx");
                resetEvent.WaitOne(5000);
                natsClient.Stop();
            }
            Assert.IsFalse(errorThrown);
            Assert.AreEqual(receivedMessage, "xxx");
        }

        [TestMethod, Description("should receive empty message")]
        [TestCategory("Integration")]
        public void TC010_ReciveEmptyMessage()
        {
            bool errorThrown = false;
            string receivedMessage = "xxx";
            AutoResetEvent resetEvent = new AutoResetEvent(false);

            using (Reactor natsClient = new Reactor())
            {
                natsClient.OnError += new EventHandler<ReactorErrorEventArgs>(delegate(object sender, ReactorErrorEventArgs args)
                {
                    errorThrown = true;
                    resetEvent.Set();
                });

                natsClient.Start(natsEndpoint);

                natsClient.Subscribe("foo", delegate(string msg, string reply, string subject)
                {
                    receivedMessage = msg;
                    resetEvent.Set();
                });

                natsClient.Publish("foo", null, "");
                resetEvent.WaitOne(5000);
                natsClient.Stop();
            }
            Assert.IsFalse(errorThrown);
            Assert.AreEqual(receivedMessage, "");
        }

        [TestMethod, Description("should receive a message that it has a wildcard subscription to")]
        [TestCategory("Integration")]
        public void TC011_ReciveMessageIfWildcardSubscription()
        {
            string errorThrown = null;
            string receivedMessage = "";
            AutoResetEvent resetEvent = new AutoResetEvent(false);

            using (Reactor natsClient = new Reactor())
            {
                natsClient.OnError += new EventHandler<ReactorErrorEventArgs>(delegate(object sender, ReactorErrorEventArgs args)
                {
                    errorThrown = args.Message == null ? String.Empty : args.Message;
                    resetEvent.Set();
                });

                natsClient.Start(natsEndpoint);

                // todo: vladi: if not testing agains an empty server, this subscription may fail
                natsClient.Subscribe("*", delegate(string msg, string reply, string subject)
                {
                    receivedMessage = msg;
                    resetEvent.Set();
                });

                natsClient.Publish("foo", null, "xxx");
                resetEvent.WaitOne(5000);
                natsClient.Stop();
            }
            Assert.IsTrue(errorThrown == null, errorThrown);
            Assert.AreEqual(receivedMessage, "xxx");
        }

        [TestMethod, Description("should not receive a message that it has unsubscribed from")]
        [TestCategory("Integration")]
        public void TC012_RecieveMessageOnUnsubscription()
        {
            bool errorThrown = false;
            int receivedCount = 0;
            object receivedCountLock = new object();
            int sid = 0;

            Reactor natsClient = new Reactor();
            natsClient.OnError += new EventHandler<ReactorErrorEventArgs>(delegate(object sender, ReactorErrorEventArgs args)
            {
                errorThrown = true;
            });

            natsClient.Start(natsEndpoint);

            sid = natsClient.Subscribe("foo", delegate(string msg, string reply, string subject)
            {
                lock (receivedCountLock)
                {
                    receivedCount++;
                    if (receivedCount == 2)
                    {
                        natsClient.Unsubscribe(sid);
                    }
                }
            });

            natsClient.Publish("foo", () => 
            {
                natsClient.Publish("foo", () => 
                {
                    natsClient.Publish("foo", () => { }, "xxx");
                }, "xxx");
            }, "xxx");
            
            Thread.Sleep(5000);
            natsClient.Stop();

            Assert.IsFalse(errorThrown);
            Assert.AreEqual(2, receivedCount);
        }

        [TestMethod, Description("should receive a response from a request")]
        [TestCategory("Integration")]
        public void TC013_ResponseOnRequest()
        {
            bool errorThrown = false;
            string receivedMessage = "";
            string receivedReply = "";

            AutoResetEvent resetEvent = new AutoResetEvent(false);

            Reactor natsClient = new Reactor();
            natsClient.OnError += new EventHandler<ReactorErrorEventArgs>(delegate(object sender, ReactorErrorEventArgs args)
            {
                errorThrown = true;
                resetEvent.Set();
            });

            natsClient.Start(natsEndpoint);

            natsClient.Subscribe("need_help", delegate(string msg, string reply, string subject)
            {
                receivedMessage = msg;
                natsClient.Publish(reply, null, "help");
            });

            // todo: vladi: this doesn't work if no message is sent.
            natsClient.Request("need_help", null, delegate(string msg, string reply, string subject)
            {
                receivedReply = msg;
                resetEvent.Set();
            }, "yyy");

            resetEvent.WaitOne(5000);
            natsClient.Stop();

            Assert.IsFalse(errorThrown);
            Assert.AreEqual(receivedMessage, "yyy");
            Assert.AreEqual(receivedReply, "help");
        }

        [TestMethod, Description("should return inside closure on publish when server received msg")]
        [TestCategory("Integration")]
        public void TC014_PublishCallback()
        {
            bool errorThrown = false;
            bool done = false;

            AutoResetEvent resetEvent = new AutoResetEvent(false);

            using (Reactor natsClient = new Reactor())
            {
                natsClient.OnError += new EventHandler<ReactorErrorEventArgs>(delegate(object sender, ReactorErrorEventArgs args)
                {
                    errorThrown = true;
                    resetEvent.Set();
                });

                natsClient.Start(natsEndpoint);


                natsClient.Publish("foo", delegate()
                {
                    done = true;
                    resetEvent.Set();
                });

                resetEvent.WaitOne(5000);
                natsClient.Stop();
            }
            Assert.IsFalse(errorThrown);
            Assert.IsTrue(done);
        }

        [TestMethod, Description("should return inside closure in ordered fashion when server received msg")]
        [TestCategory("Integration")]
        public void TC015_MultipleResponseCallback()
        {

            bool errorThrown = false;
            List<int> expected = new List<int>();
            List<int> response = new List<int>();

            AutoResetEvent resetEvent = new AutoResetEvent(false);

            using (Reactor natsClient = new Reactor())
            {
                natsClient.OnError += new EventHandler<ReactorErrorEventArgs>(delegate(object sender, ReactorErrorEventArgs args)
                {
                    errorThrown = true;
                    resetEvent.Set();
                });

                natsClient.Start(natsEndpoint);

                SimpleCallback[] delegates = new SimpleCallback[] {
                delegate() { response.Add(0); },
                delegate() { response.Add(1); },
                delegate() { response.Add(2); },
                delegate() { response.Add(3); },
                delegate() { response.Add(4); },
                delegate() { response.Add(5); },
                delegate() { response.Add(6); },
                delegate() { response.Add(7); },
                delegate() { response.Add(8); },
                delegate() { response.Add(9); },
                delegate() { response.Add(10); },
                delegate() { response.Add(11); },
                delegate() { response.Add(12); },
                delegate() { response.Add(13); },
                delegate() { response.Add(14); }
                };

                for (int i = 0; i < 15; i++)
                {
                    expected.Add(i);
                    natsClient.Publish("foo", delegates[i]);
                }

                natsClient.Publish("foo", delegate()
                {
                    resetEvent.Set();
                });

                resetEvent.WaitOne(5000);
                natsClient.Stop();
            }
            Assert.IsFalse(errorThrown);

            for (int i = 0; i < 15; i++)
            {
                Assert.AreEqual(expected[i], response[i]);
            }
        }

        [TestMethod, Description("should be able to start and use a new connection inside of start block")]
        [TestCategory("Integration")]
        public void TC016_NewConInStart()
        {
            bool errorThrown = false;
            string receivedMessage = "";
            AutoResetEvent resetEvent = new AutoResetEvent(false);

            using (Reactor natsClient = new Reactor())
            {
                natsClient.OnError += new EventHandler<ReactorErrorEventArgs>(delegate(object sender, ReactorErrorEventArgs args)
                {
                    errorThrown = true;
                    resetEvent.Set();
                });

                natsClient.Start(natsEndpoint);

                natsClient.Subscribe("foo", delegate(string msg, string reply, string subject)
                {
                    receivedMessage = msg;
                    resetEvent.Set();
                });


                ManualResetEvent publishDone = new ManualResetEvent(false);

                natsClient.Publish("foobar", delegate { publishDone.Set(); });

                publishDone.WaitOne();

                using (Reactor natsClient2 = new Reactor())
                {
                    natsClient2.Start(natsEndpoint);
                    natsClient2.Publish("foo", null, "xxx");

                    resetEvent.WaitOne(5000);

                    natsClient.Stop();
                    natsClient2.Stop();
                }
            }
            Assert.IsFalse(errorThrown);
            Assert.AreEqual(receivedMessage, "xxx");
        }

        [TestMethod, Description("should allow proper request/reply across multiple connections")]
        [TestCategory("Integration")]
        public void TC017_RquestReplyMultipleConnections()
        {
            bool errorThrown = false;
            string receivedMessage = "";
            string receivedReply = "";

            AutoResetEvent resetEvent = new AutoResetEvent(false);

            using (Reactor natsClient = new Reactor())
            {
                natsClient.OnError += new EventHandler<ReactorErrorEventArgs>(delegate(object sender, ReactorErrorEventArgs args)
                {
                    errorThrown = true;
                    resetEvent.Set();
                });

                natsClient.Start(natsEndpoint);

                natsClient.Subscribe("need_help", delegate(string msg, string reply, string subject)
                {
                    receivedMessage = msg;
                    natsClient.Publish(reply, null, "help");
                });

                // todo: vladi: this doesn't work if no message is sent.
                natsClient.Request("need_help", null, delegate(string msg, string reply, string subject)
                {
                    receivedReply = msg;
                    resetEvent.Set();
                }, "yyy");

                resetEvent.WaitOne(5000);

                natsClient.Stop();

            }
            Assert.IsFalse(errorThrown);
            Assert.AreEqual(receivedMessage, "yyy");
            Assert.AreEqual(receivedReply, "help");
        }

        [TestMethod, Description("should allow proper unsubscribe from within blocks")]
        [TestCategory("Integration")]
        public void TC018_UnsubscribeWithinBlocks()
        {
            bool errorThrown = false;
            int receivedMessageCount = 0;
            int sid = 0;
            AutoResetEvent resetEvent = new AutoResetEvent(false);
            object receivedMessageCountLock = new object();

            using (Reactor natsClient = new Reactor())
            {
                natsClient.OnError += new EventHandler<ReactorErrorEventArgs>(delegate(object sender, ReactorErrorEventArgs args)
                {
                    errorThrown = true;
                    resetEvent.Set();
                });

                natsClient.Start(natsEndpoint);

                sid = natsClient.Subscribe("foo", delegate(string msg, string reply, string subject)
                {
                    lock (receivedMessageCountLock)
                    {
                        receivedMessageCount++;
                        natsClient.Unsubscribe(sid);
                    }
                });

                natsClient.Publish("foo", delegate() {
                    natsClient.Publish("foo", delegate()
                    {
                        resetEvent.Set();
                    }, "xxx");
                }, "xxx");
             

                resetEvent.WaitOne(5000);
                natsClient.Stop();
            }
            Assert.IsFalse(errorThrown);
            Assert.AreEqual(1, receivedMessageCount);
        }

        [TestMethod, Description("should not call error handler for double unsubscribe unless in pedantic mode")]
        [TestCategory("Integration")]
        public void TC019_ErrorHandlerNotPedantic()
        {
            bool errorThrown = false;
            AutoResetEvent resetEvent = new AutoResetEvent(false);

            using (Reactor natsClient = new Reactor())
            {
                natsClient.OnError += new EventHandler<ReactorErrorEventArgs>(delegate(object sender, ReactorErrorEventArgs args)
                {
                    errorThrown = true;
                    resetEvent.Set();
                });
                natsClient.Start(natsEndpoint);

                int sid = natsClient.Subscribe("foo");
                natsClient.Unsubscribe(sid);
                natsClient.Unsubscribe(sid);

                resetEvent.WaitOne(5000);
                natsClient.Stop();
            }
            Assert.IsFalse(errorThrown);
        }

        [TestMethod, Description("should call error handler for double unsubscribe if in pedantic mode")]
        [TestCategory("Integration")]
        public void TC020_ErrorHandlerPedantic()
        {
            bool errorThrown = false;
            AutoResetEvent resetEvent = new AutoResetEvent(false);

            using (Reactor natsClient = new Reactor())
            {
                natsClient.OnError += new EventHandler<ReactorErrorEventArgs>(delegate(object sender, ReactorErrorEventArgs args)
                {
                    errorThrown = true;
                    resetEvent.Set();
                });
                natsClient.Pedantic = true;
                natsClient.Start(natsEndpoint);

                int sid = natsClient.Subscribe("foo");
                natsClient.Unsubscribe(sid);
                natsClient.Unsubscribe(sid);

                resetEvent.WaitOne(5000);
                natsClient.Stop();
            }
            Assert.IsTrue(errorThrown);
        }

        [TestMethod, Description("should call error handler for double unsubscribe if in pedantic mode")]
        [TestCategory("Integration")]
        public void TC021_PublishThreadSafe()
        {

            bool errorThrown = false;
            object locker = new object();
            int callbackNr = 0;
            int sid = 0;

            using (Reactor natsClient = new Reactor())
            {
                natsClient.OnError += new EventHandler<ReactorErrorEventArgs>(delegate(object sender, ReactorErrorEventArgs args)
                {
                    errorThrown = true;

                });
                for (int i = 0; i < 40; i++)
                {
                    string subject = Guid.NewGuid().ToString();

                    sid = natsClient.Subscribe(subject, delegate(string msg, string reply, string subj)
                    {
                        lock (locker)
                        {
                            callbackNr++;
                        }
                    });

                    Thread workerThread = new Thread(new ParameterizedThreadStart(delegate(object data)
                    {
                        natsClient.Publish((string)data);
                    }));

                    workerThread.Start(subject);
                }

                natsClient.Start(natsEndpoint);
                while (callbackNr != 40 || errorThrown)
                {
                    Thread.Sleep(1000);
                }


                natsClient.Stop();
            }
            Assert.IsFalse(errorThrown);
        }

        [TestMethod, Description("should receive a huge message")]
        [TestCategory("Integration")]
        public void TC022_ReciveHugeMessageForSubscription()
        {
            bool errorThrown = false;
            string receivedMessage = "";
            AutoResetEvent resetEvent = new AutoResetEvent(false);

            using (Reactor natsClient = new Reactor())
            {
                natsClient.OnError += new EventHandler<ReactorErrorEventArgs>(delegate(object sender, ReactorErrorEventArgs args)
                {
                    errorThrown = true;
                    resetEvent.Set();
                });

                natsClient.Start(natsEndpoint);

                natsClient.Subscribe("foo", delegate(string msg, string reply, string subject)
                {
                    receivedMessage = msg;
                    resetEvent.Set();
                });

                ASCIIEncoding ascii = new ASCIIEncoding();

                natsClient.Publish("foo", null, ascii.GetString(new byte[9000]));
                resetEvent.WaitOne(10000);
                natsClient.Stop();
            }
            Assert.IsFalse(errorThrown);
            Assert.AreEqual(receivedMessage.Length, 9000);
        }

        [TestMethod, Description("should receive a giant message")]
        [TestCategory("Integration")]
        public void TC023_ReciveGiantMessageForSubscription()
        {
            bool errorThrown = false;
            string receivedMessage = "";
            AutoResetEvent resetEvent = new AutoResetEvent(false);

            using (Reactor natsClient = new Reactor())
            {
                natsClient.OnError += new EventHandler<ReactorErrorEventArgs>(delegate(object sender, ReactorErrorEventArgs args)
                {
                    errorThrown = true;
                    resetEvent.Set();
                });

                natsClient.Start(natsEndpoint);

                natsClient.Subscribe("foo", delegate(string msg, string reply, string subject)
                {
                    receivedMessage = msg;
                    resetEvent.Set();
                });

                ASCIIEncoding ascii = new ASCIIEncoding();

                natsClient.Publish("foo", null, ascii.GetString(new byte[90000]));
                resetEvent.WaitOne(10000);
                natsClient.Stop();
            }
            Assert.IsFalse(errorThrown);
            Assert.AreEqual(receivedMessage.Length, 90000);
        }
    }
}
