using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using BookSleeve;
using System.Threading;
using Uhuru.CloudFoundry.ServiceBase.Worker.AsyncJob;
using System.Diagnostics;

namespace Uhuru.CloudFoundry.Test.Integration
{
    [TestClass]
    public class RedisLockTest
    {
        RedisConnection conn = null;
        string redisHost, redisPassword;
        int redisPort;

        [TestInitialize]
        public void Setup()
        {
            redisHost = "10.0.7.124";
            redisPort = 3456;
            redisPassword = "redis";

            conn = new RedisConnection(redisHost, port: redisPort, password: redisPassword);
        }

        [TestCleanup]
        public void Teardown()
        {
        }

        [TestMethod, TestCategory("Integration"), Description("Should block another lock from acquiring the same lock.")]
        public void RedisLockTest1()
        {

            string flow = "";

            conn.Open().Wait();

            var t1 = new Thread(delegate()
            {
                Debug.WriteLine("thread 1 started");
                using (var rlock1 = new Lock(conn, "lock", 20, 10, 5))
                {
                    rlock1.Lock();

                    Debug.WriteLine("lock 1 acquired");
                    flow += "lock1";
                }
                Debug.WriteLine("lock 1 released");
            });

            var t2 = new Thread(delegate()
             {
                 Debug.WriteLine("thread 2 started");
                 using (var rlock2 = new Lock(conn, "lock", 20, 10, 5))
                 {
                     rlock2.Lock();

                     Debug.WriteLine("lock 2 acquired");
                     Thread.Sleep(1000);
                     flow += "lock2";
                 }
                 Debug.WriteLine("lock 2 released");
             });

            t2.Start();
            Thread.Sleep(400);
            t1.Start();

            t1.Join();
            t2.Join();

            Assert.Equals(flow, "lock2lock1");



        }

        [TestMethod, TestCategory("Integration"), Description("Should release the lock after TTL timeout")]
        public void RedisLockTest2()
        {

            string flow = "";

            conn.Open().Wait();

            var t1 = new Thread(delegate()
            {
                Debug.WriteLine("thread 1 started");
                using (var rlock1 = new Lock(conn, "lock", timeout: 20, expiration: 10, ttl: 2))
                {
                    rlock1.Lock();

                    Debug.WriteLine("lock 1 acquired");
                    flow += "lock1";
                }
                Debug.WriteLine("lock 1 released");
            });

            var t2 = new Thread(delegate()
            {
                Debug.WriteLine("thread 2 started");
                using (var rlock2 = new Lock(conn, "lock", timeout: 20, expiration: 10, ttl: 2))
                {
                    rlock2.Lock();
                    bool expired = false;
                    rlock2.OnTtlExpired += new EventHandler(delegate
                    {
                        expired = true;
                    });

                    Debug.WriteLine("lock 2 acquired");
                    Thread.Sleep(3000); // Do some work
                    if (!expired)
                    {
                        flow += "lock2";
                    }
                }
                Debug.WriteLine("lock 2 released");
            });

            t2.Start();
            Thread.Sleep(400);
            t1.Start();

            t1.Join();
            t2.Join();

            Assert.Equals(flow, "lock1");
        }


        [TestMethod, TestCategory("Integration"), Description("ExecuteWithinLock Should abort thread if ttl expired.")]
        public void RedisLockTest2()
        {

            string flow = "";

            conn.Open().Wait();

            using (var rlock2 = new Lock(conn, "lock", timeout: 20, expiration: 10, ttl: 1))
            {
                rlock2.ExecuteWithinLock(delegate
                {
                    try
                    {
                        flow += "start.";
                        Thread.Sleep(2000); // Do some work. Takes more time then TTL
                        flow += "end.";
                    }
                    catch (ThreadAbortException)
                    {
                        flow += "abort.";
                    }
                });
            }

            Assert.Equals(flow, "start.abort.");

        }


    }
}
