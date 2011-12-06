// -----------------------------------------------------------------------
// <copyright file="DynamicDllLoadTest.cs" company="Uhuru Software">
// Copyright (c) 2011 Uhuru Software, Inc., All Rights Reserved
// </copyright>
// -----------------------------------------------------------------------

namespace CloudFoundry.Net.Test.Unit
{
    using System;
    using System.IO;
    using System.Linq;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Uhuru.CloudFoundry.Server.DEA;
    using Uhuru.CloudFoundry.Server.DEA.PluginBase;
    using Uhuru.CloudFoundry.DEA;
        
    /// <summary>
    /// a class containing a few basic test to confirm that PluginHost does load dynamically a test .dll and is able to acess its methods
    /// </summary>
    [TestClass]
    [DeploymentItem("TestDLLToLoad.dll")]
    public class DynamicDllLoadTest
    {
        private readonly string dllFolderPath = Path.GetFullPath(@"..\Out");
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
            if (File.Exists(ResultFilePath)) 
                {
                    File.Delete(ResultFilePath);
                } 
        }

        /// <summary>
        /// calls the StartApplication method of the test dll and checks that it had the intended effect
        /// </summary>
        [TestMethod]
        public void T001CallStartApplication()
        {
            this.agent.StartApplication();

            Assert.IsTrue(File.Exists(ResultFilePath)); // the file should have been created

            string[] content = File.ReadAllLines(ResultFilePath);
            Assert.IsTrue(content.Contains("StartApplication")); // file should contain this string
        }

        /// <summary>
        /// calls the StopApplication method of the test dll and checks that it had the intended effect
        /// </summary>
        [TestMethod]
        public void T002CallStopApplication()
        {
            this.agent.StopApplication();

            Assert.IsTrue(File.Exists(ResultFilePath)); // the file should have been created

            string[] content = File.ReadAllLines(ResultFilePath);
            Assert.IsTrue(content.Contains("StopApplication")); // file should contain this string
        }

        /// <summary>
        /// calls the KillApplication method of the test dll and checks that it had the intended effect
        /// </summary>
        [TestMethod]
        public void T003CallKillApplication()
        {
            this.agent.KillApplication();

            Assert.IsTrue(File.Exists(ResultFilePath)); // the file should have been created

            string[] content = File.ReadAllLines(ResultFilePath);
            Assert.IsTrue(content.Contains("KillApplication")); // the file should contain this string
        }

        /// <summary>
        /// unloads the dynamically loaded .dll and checks that it has called StopApplication prior to closing
        /// </summary>
        [TestMethod]
        public void T004CallRemoveInstance()
        {
            PluginHost.RemoveInstance(this.agent);

            Assert.IsTrue(File.Exists(ResultFilePath)); // the file should have been created

            string[] content = File.ReadAllLines(ResultFilePath);
            Assert.IsTrue(content.Contains("StopApplication")); // file should contain this string, as StopApplication was called on the app
        }

        /// <summary>
        /// calls the ConfigureDebug method of the test dll and checks that it had the intended effect
        /// </summary>
        [TestMethod]
        public void T005CallConfigureDebug()
        {
            string firstParameter = "param1";
            string secondParameter = "param2";

            this.agent.ConfigureDebug(firstParameter, secondParameter, null); //// new ApplicationVariable[0]);

            Assert.IsTrue(File.Exists(ResultFilePath)); // the file should have been created
            string[] content = File.ReadAllLines(ResultFilePath);

            string row = content.Where(r => r.StartsWith("ConfigureDebug", StringComparison.OrdinalIgnoreCase)).FirstOrDefault();

            Assert.AreNotEqual(row, default(string)); // a row fulfilling the condition should be found

            string[] parts = row.Split(' ');

            Assert.AreEqual(parts.Length, 3);
            Assert.AreEqual(parts[1], firstParameter);
            Assert.AreEqual(parts[2], secondParameter);
        }
    }
}
