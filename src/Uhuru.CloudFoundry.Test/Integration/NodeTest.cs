using System;
using System.Data.SqlClient;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Uhuru.CloudFoundry.MSSqlService;
using Uhuru.CloudFoundry.ServiceBase;
using Uhuru.Configuration;
using System.Globalization;
using System.Threading;
using System.IO;
using System.Text.RegularExpressions;

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
                target.mssqlConfig.LogicalStorageUnits = config.Service.MSSql.LogicalStorageUnits;

                target.connection = target.ConnectMSSql();

                ProvisionedService provisionedService = new ProvisionedService();

                DateTime now = DateTime.Now;

                string decoration = string.Format(
                    "{0}_{1}_{2}",
                    now.Hour,
                    now.Minute,
                    now.Second);

                provisionedService.Name = "CreateDatabaseTest_" + decoration;

                provisionedService.User = "testuser_" + decoration;
                provisionedService.Password = "password1234!";
                provisionedService.Plan = ProvisionedServicePlanType.Free;

                //////////////////////////////////////////////////////////////////////////
                // create the provisioned service db and user
                //////////////////////////////////////////////////////////////////////////

                target.CreateDatabase(provisionedService);

                target.connection.Close();

                Thread.Sleep(500);

                //////////////////////////////////////////////////////////////////////////
                // assert the existence of the db files
                //////////////////////////////////////////////////////////////////////////

                string dbScript = target.createDBScript;

                Regex fnRegex = new Regex(@"FILENAME = N'(.*)'");

                MatchCollection matches = fnRegex.Matches(dbScript);

                foreach (Match m in matches)
                {
                    string fileName = m.Value.Substring(m.Value.IndexOf('\'')).Trim(new char[] { '\'' });
                    Assert.IsTrue(File.Exists(fileName), string.Format("File '{0}' does not exist", fileName));
                }

                //////////////////////////////////////////////////////////////////////////
                // try to connect as the newly created user
                //////////////////////////////////////////////////////////////////////////

                string sqlTestConnString = string.Format(
                    CultureInfo.InvariantCulture,
                    "Data Source={0},{1};User Id={2};Password={3};MultipleActiveResultSets=true;Pooling=false",
                    target.mssqlConfig.Host,
                    target.mssqlConfig.Port,
                    provisionedService.User,
                    provisionedService.Password);

                SqlConnection sqlTest = new SqlConnection(sqlTestConnString);

                sqlTest.Open();

                //////////////////////////////////////////////////////////////////////////
                // try to connect create a table as the newly created user
                ////////////////////////////////////////////////////////////////////////// 

                SqlCommand cmdTest = new SqlCommand(
                    string.Format("CREATE TABLE [{0}].[dbo].[TestTable]([Command] [varchar](100) NULL, [Description] [varchar](50) NULL) ON [DATA]", provisionedService.Name),
                    sqlTest);
                 
                cmdTest.ExecuteNonQuery();

                sqlTest.Close();

                //////////////////////////////////////////////////////////////////////////
                // try to operate on the service db as a different user
                ////////////////////////////////////////////////////////////////////////// 

                // connect as sa
                sqlTest.ConnectionString = string.Format(
                    CultureInfo.InvariantCulture,
                    "Data Source={0},{1};User Id={2};Password={3};MultipleActiveResultSets=true;Pooling=false",
                    target.mssqlConfig.Host,
                    target.mssqlConfig.Port,
                    target.mssqlConfig.User,
                    target.mssqlConfig.Password);

                sqlTest.Open();

                string dummyUser = "dummy" + decoration;
                string dummyPwd = "password1234!";

                //create a dummy user
                string createLoginString = string.Format(@"CREATE LOGIN {0} WITH PASSWORD = '{1}'", dummyUser, dummyPwd);

                cmdTest = new SqlCommand(createLoginString, sqlTest);

                cmdTest.ExecuteNonQuery();

                sqlTest.Close();

                // connect as the dummy user

                sqlTest.ConnectionString = string.Format(
                    CultureInfo.InvariantCulture,
                    "Data Source={0},{1};User Id={2};Password={3};MultipleActiveResultSets=true;Pooling=false",
                    target.mssqlConfig.Host,
                    target.mssqlConfig.Port,
                    dummyUser,
                    dummyPwd);

                sqlTest.Open();
                
                // try to drop the service db

                try
                {
                    cmdTest.CommandText = string.Format("CREATE TABLE [{0}].[dbo].[DummyTable]([Command] [varchar](100) NULL, [Description] [varchar](50) NULL) ON [DATA]", provisionedService.Name);
                    cmdTest.ExecuteNonQuery();
                    Assert.Fail("Other users have read/write access to the service db");
                }
                catch (SqlException sex)
                {
                }

                sqlTest.Close();

                //////////////////////////////////////////////////////////////////////////
            }
            catch (System.Exception ex)
            {
                Assert.Fail(ex.Message);
            }
        }
    }
}
