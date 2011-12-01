using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Globalization;

namespace Uhuru.CloudFoundry.UI.VS2010.Extensions
{
    internal class UrlsConverter : ExpandableObjectConverter
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
                return ((string)value).Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(url => url.Trim()).ToArray();
            }
            return base.ConvertFrom(context, info, value);
        }

        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destType)
        {
            if (destType == typeof(string) && value is string[])
            {
                string[] urls = (string[])value;
                return String.Join(", ", urls);
            }
            return base.ConvertTo(context, culture, value, destType);
        }
    }

}
