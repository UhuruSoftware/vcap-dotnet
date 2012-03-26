using Uhuru.CloudFoundry.DEA;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using Uhuru.CloudFoundry.DEA.Plugins;

namespace Uhuru.CloudFoundry.Test.Unit
{
    
    
    /// <summary>
    ///This is a test class for PluginHostTest and is intended
    ///to contain all PluginHostTest Unit Tests
    ///</summary>
    [TestClass()]
    public class PluginHostTest
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
        ///A test for LoadPlugin
        ///</summary>
        [TestMethod()]
        public void LoadPluginTest()
        {
            string pathToPlugin = typeof(IISPlugin).Assembly.Location;
            string className = typeof(IISPlugin).FullName;
            PluginHost.RemoveInstance(PluginHost.CreateInstance(PluginHost.LoadPlugin(pathToPlugin, className)));
        }
    }
}
