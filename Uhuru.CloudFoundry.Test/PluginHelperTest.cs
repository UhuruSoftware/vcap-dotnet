using Uhuru.CloudFoundry.Server.DEA.PluginBase;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace Uhuru.CloudFoundry.Test
{
    
    
    /// <summary>
    ///This is a test class for PluginHelperTest and is intended
    ///to contain all PluginHelperTest Unit Tests
    ///</summary>
    [TestClass()]
    public class PluginHelperTest
    {


        private TestContext testContextInstance;

        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext
        {
            get
            {
                return testContextInstance;
            }
            set
            {
                testContextInstance = value;
            }
        }

        #region Additional test attributes
        // 
        //You can use the following additional attributes as you write your tests:
        //
        //Use ClassInitialize to run code before running the first test in the class
        //[ClassInitialize()]
        //public static void MyClassInitialize(TestContext testContext)
        //{
        //}
        //
        //Use ClassCleanup to run code after all tests in a class have run
        //[ClassCleanup()]
        //public static void MyClassCleanup()
        //{
        //}
        //
        //Use TestInitialize to run code before running each test
        //[TestInitialize()]
        //public void MyTestInitialize()
        //{
        //}
        //
        //Use TestCleanup to run code after each test has run
        //[TestCleanup()]
        //public void MyTestCleanup()
        //{
        //}
        //
        #endregion


        /// <summary>
        ///A test for GetParsedData
        ///</summary>
        [TestMethod()]
        public void GetParsedDataTest()
        {
            ApplicationVariable[] appVariables = new ApplicationVariable[] {
              new ApplicationVariable() { Name = "VCAP_PLUGIN_STAGING_INFO", Value=@"{""assembly"":""Uhuru.CloudFoundry.DEA.Plugins.dll"",""class_name"":""Uhuru.CloudFoundry.DEA.Plugins.IISPlugin"",""logs"":{""app_error"":""logs/stderr.log"",""dea_error"":""logs/err.log"",""startup"":""logs/startup.log"",""app"":""logs/stdout.log""},""auto_wire_templates"":{""mssql-2008"":""Data Source={host},{port};Initial Catalog={name};User Id={user};Password={password};MultipleActiveResultSets=true"",""mysql-5.1"":""server={host};port={port};Database={name};Uid={user};Pwd={password};""}}" },
              new ApplicationVariable() { Name = "VCAP_APPLICATION", Value=@"{""instance_id"":""646c477f54386d8afb279ec2f990a823"",""instance_index"":0,""name"":""sinatra_env_test_app"",""uris"":[""sinatra_env_test_app.uhurucloud.net""],""users"":[""dev@cloudfoundry.org""],""version"":""c394f661a907710b8a8bb70b84ff0c83354dbbed-1"",""start"":""2011-12-07 14:40:12 +0200"",""runtime"":""iis"",""state_timestamp"":1323261612,""port"":51202,""limits"":{""fds"":256,""mem"":67108864,""disk"":2147483648},""host"":""192.168.1.117""}" },
              new ApplicationVariable() { Name = "VCAP_SERVICES", Value=@"{""mssql-2008"":[{""name"":""mssql-b24a2"",""label"":""mssql-2008"",""plan"":""free"",""tags"":[""mssql"",""2008"",""relational""],""credentials"":{""name"":""D4Tac4c307851cfe495bb829235cd384f094"",""username"":""US3RTfqu78UpPM5X"",""user"":""US3RTfqu78UpPM5X"",""password"":""P4SSdCGxh2gYjw54"",""hostname"":""192.168.1.3"",""port"":1433,""bind_opts"":{}}}]}" },
              new ApplicationVariable() { Name = "VCAP_APP_HOST", Value=@"192.168.1.118" },
              new ApplicationVariable() { Name = "VCAP_APP_PORT", Value=@"65498" },
              new ApplicationVariable() { Name = "VCAP_WINDOWS_USER_PASSWORD", Value=@"password" },
              new ApplicationVariable() { Name = "VCAP_WINDOWS_USER", Value=@"cfuser" },
              new ApplicationVariable() { Name = "VCAP_APP_PATH", Value=@"c:\droplets\mydroplet" }
            };
            ApplicationParsedData actual;
            actual = PluginHelper.GetParsedData(appVariables);
            Assert.AreEqual("646c477f54386d8afb279ec2f990a823", actual.AppInfo.InstanceId);
            Assert.AreEqual("192.168.1.118", actual.AppInfo.LocalIp);
            Assert.AreEqual("sinatra_env_test_app", actual.AppInfo.Name);
            Assert.AreEqual(@"c:\droplets\mydroplet\app", actual.AppInfo.Path);
            Assert.AreEqual(65498, actual.AppInfo.Port);
            Assert.AreEqual("cfuser", actual.AppInfo.WindowsUsername);
            Assert.AreEqual("password", actual.AppInfo.WindowsPassword);
            Assert.AreEqual(2, actual.AutoWireTemplates.Count);
            Assert.AreEqual(@"c:\droplets\mydroplet\logs/stderr.log", actual.ErrorLogFilePath);
            Assert.AreEqual(@"c:\droplets\mydroplet\logs/stdout.log", actual.LogFilePath);
            Assert.AreEqual("iis", actual.Runtime);
            Assert.AreEqual("192.168.1.3", actual.Services[0].Host);
            Assert.AreEqual("", actual.Services[0].InstanceName);
            Assert.AreEqual("D4Tac4c307851cfe495bb829235cd384f094", actual.Services[0].Name);
            Assert.AreEqual("P4SSdCGxh2gYjw54", actual.Services[0].Password);
            Assert.AreEqual("free", actual.Services[0].Plan);
            Assert.AreEqual(1433, actual.Services[0].Port);
            Assert.AreEqual("mssql-2008", actual.Services[0].ServiceLabel);
            Assert.AreEqual(3, actual.Services[0].ServiceTags.Length);
            Assert.AreEqual("US3RTfqu78UpPM5X", actual.Services[0].User);
            Assert.AreEqual(@"c:\droplets\mydroplet\logs/startup.log", actual.StartupLogFilePath);
        }
    }
}
