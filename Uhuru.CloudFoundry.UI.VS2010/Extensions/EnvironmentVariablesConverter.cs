using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Globalization;
using Uhuru.CloudFoundry.UI.Packaging;

namespace Uhuru.CloudFoundry.UI.VS2010.Extensions
{
    internal class EnvironmentVariablesConverter : ExpandableObjectConverter
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
                string[] variables = ((string)value).Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                List<EnvironmentVariable> result = new List<EnvironmentVariable>();

                foreach (string variable in variables)
                {
                    string[] pieces = variable.Split(new char[] {'='}, StringSplitOptions.RemoveEmptyEntries);
                    EnvironmentVariable newVariable = new EnvironmentVariable();
                    newVariable.Name = pieces.Length > 0 ? pieces[0].Trim() : String.Empty;
                    newVariable.Value = pieces.Length > 1 ? pieces[1].Trim() : String.Empty;
                    result.Add(newVariable);
                }
                return result.ToArray();
            }
            return base.ConvertFrom(context, info, value);
        }

        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destType)
        {
            if (destType == typeof(string) && value is EnvironmentVariable[])
            {
                EnvironmentVariable[] variables = (EnvironmentVariable[])value;

                return String.Join(", ", variables.Select(variable => variable.Name + "=" + variable.Value));
            }
            return base.ConvertTo(context, culture, value, destType);
        }
    }

}
