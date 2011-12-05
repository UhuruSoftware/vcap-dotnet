// -----------------------------------------------------------------------
// <copyright file="JsonTools.cs" company="Uhuru Software">
// </copyright>
// -----------------------------------------------------------------------

namespace Uhuru.Utilities
{
    using System.Xml;
    using System.Xml.XPath;
    
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
