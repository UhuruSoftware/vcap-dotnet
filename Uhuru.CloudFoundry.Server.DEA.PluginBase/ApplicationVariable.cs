using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace Uhuru.CloudFoundry.Server.DEA.PluginBase
{
    public class ApplicationVariable : MarshalByRefObject, ISerializable
    {
        public string Name { get; set; }
        public string Value { get; set; }

        protected ApplicationVariable(SerializationInfo info, StreamingContext ctxt)
        {
            
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            
        }
    }

}
