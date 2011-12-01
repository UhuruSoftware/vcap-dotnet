using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.XPath;

namespace CloudFoundry.Net.IIS.Utilities
{
    static public class JsonTools
    {
        public static IXPathNavigable JsonToXml(string json)    
        {
            System.Text.UTF8Encoding str = new System.Text.UTF8Encoding();

            byte[] bytes = str.GetBytes(json);
            
            System.Xml.XmlDictionaryReaderQuotas quotas = System.Xml.XmlDictionaryReaderQuotas.Max;

            XmlDocument xml;
            using (XmlDictionaryReader jsonReader = System.Runtime.Serialization.Json.JsonReaderWriterFactory.CreateJsonReader(bytes, quotas))
            {
                xml = new XmlDocument();
                xml.Load(jsonReader);
            }
            return xml;
        }
    }
}
