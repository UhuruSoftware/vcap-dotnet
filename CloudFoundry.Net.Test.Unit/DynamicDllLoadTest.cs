using System;
using System.IO;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using Uhuru.CloudFoundry.Server.DEA;
using Uhuru.CloudFoundry.Server.DEA.PluginBase;

namespace CloudFoundry.Net.Test.Unit
{
    [TestFixture]
    [Ignore]
    public class DynamicDllLoadTest
    {
        IAgentPlugin agent;
        string dllFolderPath = Path.GetFullPath(@"..\Target");
        string dllFileName = "TestDLLToLoad.dll";

        string resultFilePath = @"D:/file.txt";


        public DynamicDllLoadTest()
        { 
            //copy the dll file in the base folder
            string destinationFileName = Path.Combine(dllFolderPath, dllFileName);
            if (File.Exists(destinationFileName)) File.Delete(destinationFileName);

            File.Copy(Path.Combine(dllFolderPath, "lib", dllFileName), destinationFileName);
        }

        [SetUp]
        public void Setup()
        {
            //get an IAgentPlugin
            Guid guid = PluginHost.LoadPlugin(Path.Combine(dllFolderPath, dllFileName), "TestClass");
            agent = PluginHost.CreateInstance(guid);
        }

        [TearDown]
        public void Teardown()
        {
            //clear file
            if (File.Exists(resultFilePath)) File.Delete(resultFilePath); 
        }

        [Test]
        public void T001_CallStartApplication()
        {
            agent.StartApplication();

            Assert.IsTrue(File.Exists(resultFilePath)); //the file should have been created

            string[] content = File.ReadAllLines(resultFilePath);
            Assert.IsTrue(content.Contains("StartApplication")); //file should contain this string
        }

        [Test]
        public void T002_CallStopApplication()
        {
            agent.StopApplication();

            Assert.IsTrue(File.Exists(resultFilePath)); //the file should have been created

            string[] content = File.ReadAllLines(resultFilePath);
            Assert.IsTrue(content.Contains("StopApplication")); //file should contain this string
        }

        [Test]
        public void T003_CallKillApplication()
        {
            agent.KillApplication();

            Assert.IsTrue(File.Exists(resultFilePath)); //the file should have been created

            string[] content = File.ReadAllLines(resultFilePath);
            Assert.IsTrue(content.Contains("KillApplication")); //file should contain this string
        }

        [Test]
        public void T004_CallRemoveInstance()
        {
            PluginHost.RemoveInstance(agent);

            Assert.IsTrue(File.Exists(resultFilePath)); //the file should have been created

            string[] content = File.ReadAllLines(resultFilePath);
            Assert.IsTrue(content.Contains("StopApplication")); //file should contain this string, as StopApplication was called on the app
        }

        [Test]
        public void T005_CallConfigureDebug()
        {
            string firstParameter = "param1";
            string secondParameter = "param2";

            agent.ConfigureDebug(firstParameter, secondParameter, new ApplicationVariable[0]);

            Assert.IsTrue(File.Exists(resultFilePath)); //the file should have been created
            string[] content = File.ReadAllLines(resultFilePath);

            string row = content.Where(r => r.StartsWith("ConfigureDebug")).FirstOrDefault();

            Assert.AreNotEqual(row, default(string)); //a row fulfilling the condition should have been found

            string[] parts = row.Split(' ');

            Assert.AreEqual(parts.Length, 3);
            Assert.AreEqual(parts[1], firstParameter);
            Assert.AreEqual(parts[2], secondParameter);
        }

    }
}
