using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Uhuru.CloudFoundry.MSSqlService;
using Uhuru.CloudFoundry.ServiceBase;
using Uhuru.Configuration;

namespace Uhuru.CloudFoundry.Test.Integration
{
    /// <summary>
    ///This is a test class for NodeTest and is intended
    ///to contain all NodeTest Unit Tests
    ///</summary>
    [TestClass()]
    [DeploymentItem("uhuruTest.config")]
    public class NodeTest
    {
        /// <summary>
        ///A test for CreateDatabase
        ///</summary>
        [TestMethod()]
        [TestCategory("Integration")]
        [DeploymentItem("Uhuru.CloudFoundry.MSSqlService.dll")]
        [DeploymentItem("log4net.config")]
        [DeploymentItem("Uhuru.CloudFoundry.Test\\lib\\TestDLLToLoad.dll")]
        public void CreateDatabaseTest()
        {
            try
            {
                Node_Accessor target = new Node_Accessor();
                target.mssqlConfig = new MSSqlOptions();
                UhuruSection config = UhuruSection.GetSection();

                target.mssqlConfig.Host = config.Service.MSSql.Host;
                target.mssqlConfig.User = config.Service.MSSql.User;
                target.mssqlConfig.Password = config.Service.MSSql.Password;
                target.mssqlConfig.Port = config.Service.MSSql.Port;

                target.connection = target.ConnectMSSql();
                
                ProvisionedService provisionedService = new ProvisionedService();

                provisionedService.Name = "CreateDatabaseTest_" + string.Format(
                    "{0}_{1}_{2}__{3}", 
                    System.DateTime.Now.Hour, 
                    System.DateTime.Now.Minute, 
                    System.DateTime.Now.Second, 
                    System.DateTime.Now.Millisecond);
              
                provisionedService.User = "testuser";
                provisionedService.Password = "password1234!";
                provisionedService.Plan = ProvisionedServicePlanType.Free;

                target.CreateDatabase(provisionedService);


            }
            catch (System.Exception ex)
            {
                Assert.Fail(ex.Message);
            }
        }
    }
}
