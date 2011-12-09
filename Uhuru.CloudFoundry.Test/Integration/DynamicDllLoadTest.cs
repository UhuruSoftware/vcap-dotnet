using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using Uhuru.CloudFoundry.DEA;
using Uhuru.CloudFoundry.Server.DEA.PluginBase;

namespace Uhuru.CloudFoundry.Test.Integration
{

    /// <summary>
    /// a class containing a few basic test to confirm that PluginHost does load dynamically a test .dll and is able to acess its methods
    /// </summary>
    [TestClass]
    [DeploymentItem("TestDLLToLoad.dll")]
    public class DynamicDllLoadTest
    {
        private readonly string dllFolderPath = Path.GetFullPath(@"..\..\..\..\bin-vcap-dotnet");
        private const string DllFileName = "TestDLLToLoad.dll";
        private const string ResultFilePath = @"D:/file.txt";

        private IAgentPlugin agent;

        /// <summary>
        /// load the test .dll
        /// </summary>
        [TestInitialize]
        public void Setup()
        {
            // get an IAgentPlugin
            Guid guid = PluginHost.LoadPlugin(Path.Combine(this.dllFolderPath, DllFileName), "TheDLLToLoad.TestClass");
            this.agent = PluginHost.CreateInstance(guid);
        }

        /// <summary>
        /// clear the file written by the test methods, so that the next test would start afresh
        /// </summary>
        [TestCleanup]
        public void Teardown()
        {
            PluginHost.RemoveInstance(agent);

            if (File.Exists(ResultFilePath))
            {
                File.Delete(ResultFilePath);
            }
        }

        /// <summary>
        /// calls the StartApplication method of the test dll and checks that it had the intended effect
        /// </summary>
        [TestMethod]
        [TestCategory("Integration")]
        public void TC001_CallStartApplication()
        {
            //Arrange

            //Act
            this.agent.StartApplication();

            //Assert
            Assert.IsTrue(File.Exists(ResultFilePath)); // the file should have been created
            string[] content = File.ReadAllLines(ResultFilePath);
            Assert.IsTrue(content.Contains("StartApplication")); // file should contain this string
        }

        /// <summary>
        /// calls the StopApplication method of the test dll and checks that it had the intended effect
        /// </summary>
        [TestMethod]
        [TestCategory("Integration")]
        public void TC002_CallStopApplication()
        {
            //Arrange 
            this.agent.StartApplication();

            //Act
            this.agent.StopApplication();

            //Assert
            Assert.IsTrue(File.Exists(ResultFilePath)); // the file should have been created
            string[] content = File.ReadAllLines(ResultFilePath);
            Assert.IsTrue(content.Contains("StopApplication")); // file should contain this string
        }


        /// <summary>
        /// unloads the dynamically loaded .dll and checks that it has called StopApplication prior to closing
        /// </summary>
        [TestMethod]
        [TestCategory("Integration")]
        public void TC004_CallRemoveInstance()
        {
            //Arrange

            //Act
            PluginHost.RemoveInstance(this.agent);

            //Assert
            Assert.IsTrue(File.Exists(ResultFilePath)); // the file should have been created
            string[] content = File.ReadAllLines(ResultFilePath);
            Assert.IsTrue(content.Contains("StopApplication")); // file should contain this string, as StopApplication was called on the app
        }

    }
}

