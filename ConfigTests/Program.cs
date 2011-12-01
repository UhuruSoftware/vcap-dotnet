using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Configuration;

namespace ConfigTests
{
    class Program
    {
        static void Main(string[] args)
        {
            CmdArguments arguments = new CmdArguments(args);
            if (arguments.HasParam("?"))
            {
                Console.WriteLine(@"
-config=configFile
[-argname=argValue]
");
                return;
            }
            
            string configurationFile = arguments["config"];

            foreach (string key in ConfigurationManager.AppSettings.Keys)
            {              
                string value = ConfigurationManager.AppSettings[key];
                if(arguments.HasParam(value))
                {
                    UpdateKey(configurationFile, key, arguments[value]);
                    if (value == "proxy")
                    {
                        UpdateProxy(configurationFile, arguments[value]);
                    }
                }            
            }
        }

        private static void UpdateKey(string fileName, string key, string newValue)
        {
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.Load(fileName);
            XmlNode appSettingsNode = xmlDoc.SelectSingleNode("configuration/appSettings");
            foreach (XmlNode childNode in appSettingsNode)
            {
                if (childNode.Attributes["key"].Value == key)
                    childNode.Attributes["value"].Value = newValue;
            }
            xmlDoc.Save(fileName);
        }

        private static void UpdateProxy(string fileName, string proxy)
        {
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.Load(fileName);
            XmlNode appSettingsNode = xmlDoc.SelectSingleNode("configuration/system.net/defaultProxy");
            appSettingsNode.ChildNodes[0].Attributes["proxyaddress"].Value = proxy;
            xmlDoc.Save(fileName);
        }
    }
}
