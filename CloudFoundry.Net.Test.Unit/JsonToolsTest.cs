using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Uhuru.Utilities;

namespace CloudFoundry.Net.Test.Unit
{
    [TestClass]
    public class JsonToolsTest
    {
        [TestMethod]
        public void TestLoad()
        {
            string jsonVariable = @"{""mssql-2008"":[{""name"":""mytest"",""label"":""mssql-2008"",""plan"":""free"",""tags"":[""mssql"",""2008"",""relational""],""credentials"":{""name"":""D4TA82606758d99a463f85e067a715f92997"",""hostname"":""192.168.1.162"",""host"":""192.168.1.162"",""port"":1433,""user"":""US3R6qePz8ohwhbo"",""username"":""US3R6qePz8ohwhbo"",""password"":""P4SSFItfAuFA1VZZ""}}]}";

            XmlDocument services = (XmlDocument)JsonTools.JsonToXml(jsonVariable);
            XmlNodeList serviceList = services.SelectNodes("/root/mssql-2008/item/name");

            Dictionary<string, string> connections = new Dictionary<string, string>();

            foreach (XmlNode node in serviceList)
            {
                string serviceName = node.InnerText;
                string selectQuery = String.Format("/root/mssql-2008/item[name=\"{0}\"]/credentials/", serviceName);
                string databaseName = services.SelectSingleNode(selectQuery + "name").InnerText;
                string host = services.SelectSingleNode(selectQuery + "host").InnerText;
                string serverPort = services.SelectSingleNode(selectQuery + "port").InnerText;
                string username = services.SelectSingleNode(selectQuery + "username").InnerText;
                string password = services.SelectSingleNode(selectQuery + "password").InnerText;

                connections.Add(serviceName,
                    String.Format("Data Source={0},{2};Initial Catalog={1};User Id={2};Password={3};",
                    host, serverPort, databaseName, username, password));
            }

        }
    }
}
