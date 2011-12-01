using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Globalization;
using Uhuru.CloudFoundry.UI.Packaging;

namespace Uhuru.CloudFoundry.UI.VS2010.Extensions
{
    internal class ServicesConverter : ExpandableObjectConverter
    {
        public override bool CanConvertFrom(ITypeDescriptorContext context, Type type)
        {
            if (type == typeof(string))
            {
                return true;
            }
            return base.CanConvertFrom(context, type);
        }

        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo info, object value)
        {
            if (value is string)
            {
                string[] services = ((string)value).Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                List<CloudApplicationService> result = new List<CloudApplicationService>();

                foreach (string service in services)
                {
                    string[] pieces = service.Split(new char[] {'#'}, StringSplitOptions.RemoveEmptyEntries);
                    CloudApplicationService newService = new CloudApplicationService();
                    newService.ServiceType = pieces.Length > 0 ? pieces[0].Trim() : String.Empty;
                    newService.ServiceName = pieces.Length > 1 ? pieces[1].Trim() : String.Empty;
                    result.Add(newService);
                }
                return result.ToArray();
            }
            return base.ConvertFrom(context, info, value);
        }

        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destType)
        {
            if (destType == typeof(string) && value is CloudApplicationService[])
            {
                CloudApplicationService[] services = (CloudApplicationService[])value;

                return String.Join(", ", services.Select(service => service.ServiceType + "#" + service.ServiceName));
            }
            return base.ConvertTo(context, culture, value, destType);
        }
    }

}
