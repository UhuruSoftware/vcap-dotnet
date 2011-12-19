// -----------------------------------------------------------------------
// <copyright file="JsonTools.cs" company="Uhuru Software, Inc.">
// Copyright (c) 2011 Uhuru Software, Inc., All Rights Reserved
// </copyright>
// -----------------------------------------------------------------------

namespace Uhuru.Utilities
{
    using System.Xml;
    using System.Xml.XPath;
    
    /// <summary>
    /// a class offering json-related functionalities
    /// </summary>
    public static class JsonTools
    {
        /// <summary>
        /// converts a json object to an xmlPath
        /// </summary>
        /// <param name="json">the json object to convert</param>
        /// <returns>the conversion result</returns>
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
