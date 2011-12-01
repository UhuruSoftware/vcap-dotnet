using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using CloudFoundry.Net;

namespace Uhuru.CloudFoundry.UI.VS2010.Extensions
{
    internal class RuntimeConverter : StringConverter
    {

        StandardValuesCollection standardValues = null;

        public override bool GetStandardValuesSupported(
                       ITypeDescriptorContext context)
        {
            return true;
        }

        public override bool GetStandardValuesExclusive(ITypeDescriptorContext context)
        {
            return false;
        }

        public override StandardValuesCollection GetStandardValues(ITypeDescriptorContext context)
        {
            if (standardValues == null)
            {
                Client client = IntegrationCenter.CloudClient;
                if (client != null)
                {
                    standardValues = new StandardValuesCollection(client.Runtimes().Select(runtime => runtime.Name).ToArray());
                }
            }

            return standardValues != null ? standardValues : new StandardValuesCollection(new string[0]);
        }
    }
}
