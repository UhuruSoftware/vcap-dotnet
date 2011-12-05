using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Web.Administration;
using System.IO;
using CloudFoundry.Net.IIS;
using System.Globalization;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CloudFoundry.Net.Test.Unit
{
    [TestClass]
    public class IISOperationsTest
    {
        static string applicationPhysicalPath;
        string applicationName = "cloudTestApp";
        string tempAppFolder;
        static List<string> foldersCreated;

        [ClassInitialize]
        public static void BeforeClass(TestContext context)
        {
            applicationPhysicalPath = Path.GetFullPath(@"..\cf.net\TestApps\CloudTestApp");
            foldersCreated = new List<string>();
        }

        [ClassCleanup]
        public static void TestFixtureTeardown()
        {
            foreach (string str in foldersCreated)
            {
                Directory.Delete(str, true);
            }
        }

        [TestInitialize]
        public void Setup()
        {
            tempAppFolder = Helper.CopyFolderToTemp(applicationPhysicalPath);
            foldersCreated.Add(tempAppFolder);
        }

        [TestCleanup]
        public void Teardown()
        {
            tempAppFolder = string.Empty;
        }

        [TestMethod]
        public void TC001_TestCreateDelete()
        {
            int port = Helper.GetEphemeralPort();
            try
            {
                Operations.Create(applicationName, port, tempAppFolder, true);
            }
            catch (Exception e)
            {
                Assert.Fail(e.ToString());
            }

            Site webSite = null;
            using (ServerManager srvManager = new ServerManager())
            {
                foreach (Site site in srvManager.Sites)
                {
                    if (site.Bindings[0].EndPoint.Port == port)
                    {
                        webSite = site;
                        break;
                    }
                }
            }
            Assert.IsNotNull(webSite);
            Assert.AreEqual(Operations.RemoveSpecialCharacters(applicationName) + port.ToString(CultureInfo.InvariantCulture), webSite.Name);

            try
            {
                Operations.Delete(port);
            }
            catch (Exception e)
            {
                Assert.Fail(e.ToString());
            }

            webSite = null;
            using (ServerManager srvManager = new ServerManager())
            {
                foreach (Site site in srvManager.Sites)
                {
                    if (site.Bindings[0].EndPoint.Port == port)
                    {
                        webSite = site;
                        break;
                    }
                }
            }
            Assert.IsNull(webSite);
        }

        [TestMethod]
        public void TC002_TestCleanup()
        {
            int port = Helper.GetEphemeralPort();
            string appsRootDirectory = Path.GetTempPath();
            try
            {
                for (int i = 0; i < 10; i++)
                {
                    Operations.Create(applicationName + i.ToString(CultureInfo.InvariantCulture), port + i, tempAppFolder, true);
                    tempAppFolder = Helper.CopyFolderToTemp(applicationPhysicalPath);
                    foldersCreated.Add(tempAppFolder);
                }
            }
            catch (Exception ex)
            {
                Assert.Fail("Failed creating website " + ex.ToString());
            }
            try
            {
                Operations.Cleanup(appsRootDirectory);
            }
            catch (Exception ex)
            {
                Assert.Fail("Failed cleaning up " + ex.ToString());
            }

            using (ServerManager serverMgr = new ServerManager())
            {
                DirectoryInfo root = new DirectoryInfo(appsRootDirectory);
                DirectoryInfo[] childDirectories = root.GetDirectories("*", SearchOption.AllDirectories);

                foreach (Site site in serverMgr.Sites)
                {
                    string sitePath = site.Applications["/"].VirtualDirectories["/"].PhysicalPath;
                    if (sitePath.ToUpperInvariant() == root.FullName.ToUpperInvariant())
                    {
                        Assert.Fail("Application " + site.Name + " has not been removed!");
                    }
                    foreach (DirectoryInfo di in childDirectories)
                    {
                        if (di.FullName.ToUpperInvariant() == sitePath.ToUpperInvariant())
                        {
                            Assert.Fail("Application " + site.Name + " has not been removed!");
                            break;
                        }
                    }
                }
            }
        }

        [TestMethod]
        public void TC003_TestCreateDelete10Parallel()
        {
            int port = Helper.GetEphemeralPort();
            object lck = new object();
            List<Exception> exceptions = new List<Exception>();
            List<Thread> threads = new List<Thread>();
            for (int i = 0; i < 10; i++)
            {
                int p = port + i;
                ThreadStart s = delegate
                {
                    string name = Guid.NewGuid().ToString();
                    try
                    {
                        string appPath = Helper.CopyFolderToTemp(applicationPhysicalPath);
                        lock (lck)
                        {
                            foldersCreated.Add(appPath);
                        }
                        Operations.Create(name, p, appPath);
                    }
                    catch (Exception e)
                    {
                        lock (lck)
                        {
                            exceptions.Add(e);
                        }
                    }
                };

                Thread t = new Thread(s);
                t.Name = i.ToString();
                threads.Add(t);
            }

            foreach (Thread t in threads)
            {
                t.Start();
            }

            foreach (Thread t in threads)
            {
                t.Join();
            }

            if (exceptions.Count > 0)
            {
                StringBuilder sb = new StringBuilder();
                foreach (Exception ex in exceptions)
                {
                    sb.AppendLine(ex.ToString());
                }
                Assert.Inconclusive("At least one exception has been  thrown:" + sb.ToString());
            }
            threads = new List<Thread>();

            for (int i = 0; i < 10; i++)
            {
                int p = port + i;
                ThreadStart s = delegate
                {
                    try
                    {
                        Operations.Delete(p);
                    }
                    catch (Exception e)
                    {
                        lock (lck)
                        {
                            exceptions.Add(e);
                        }
                    }
                };

                Thread t = new Thread(s);
                t.Name = i.ToString();
                threads.Add(t);
            }

            foreach (Thread t in threads)
            {
                t.Start();
            }

            foreach (Thread t in threads)
            {
                t.Join();
            }

            if (exceptions.Count > 0)
            {
                StringBuilder sb = new StringBuilder();
                foreach (Exception ex in exceptions)
                {
                    sb.AppendLine(ex.ToString());
                }
                Assert.Inconclusive("At least one exception has been  thrown:" + sb.ToString());
            }
        }
    }
}
