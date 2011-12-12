// -----------------------------------------------------------------------
// <copyright file="JsonConvertibleObject.cs" company="Uhuru Software">
// Copyright (c) 2011 Uhuru Software, Inc., All Rights Reserved
// </copyright>
// -----------------------------------------------------------------------

namespace Uhuru.Utilities
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Reflection;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;

    /// <summary>
    /// This is an attribute that is used to decorate fields/properties/enums with JSON names.
    /// The JSON name will be used instead of the member's name when serializing.
    /// This is used in conjunction <see cref="JsonConvertibleObject"/>.
    /// </summary>
    [AttributeUsage(AttributeTargets.Enum | AttributeTargets.Property | AttributeTargets.Field)]
    public sealed class JsonNameAttribute : Attribute
    {
        private string name;

        /// <summary>
        /// Initializes a new instance of the <see cref="JsonNameAttribute"/> class.
        /// </summary>
        /// <param name="name">The JSON name of the member.</param>
        public JsonNameAttribute(string name)
        {
            this.name = name;
        }

        /// <summary>
        /// Gets or sets the Name of the member.
        /// </summary>
        public string Name
        {
            get
            {
                return name;
            }
        }
    }

    /// <summary>
    /// This object is used for serialization/deserialization of objects into/from JSON.
    /// </summary>
    public class JsonConvertibleObject
    {
        public static object[] DeserializeFromJsonArray(string json)
        {
            return ((JArray)DeserializeFromJson(json)).ToObject<object[]>();
        }

        public static object DeserializeFromJson(string json)
        {
            return JsonConvert.DeserializeObject(json);
        }

        public static string SerializeToJson(object intermediateValue)
        {
            return JsonConvert.SerializeObject(intermediateValue);
        }

        public string SerializeToJson()
        {
            return JsonConvert.SerializeObject(this.ToJsonIntermediateObject());
        }

        public Dictionary<string, object> ToJsonIntermediateObject(params string[] elementsToInclude)
        {
            HashSet<string> elementsToIncludeSet = null;
            if (elementsToInclude.Length != 0)
            {
                elementsToIncludeSet = new HashSet<string>(elementsToInclude);
            }

            Dictionary<string, object> result = new Dictionary<string, object>();
            Type type = this.GetType();

            MemberInfo[] fieldAndPropertyInfos = type.GetProperties().Select(prop => (MemberInfo)prop).Union(type.GetFields().Select(field => (MemberInfo)field)).ToArray();

            foreach (MemberInfo member in fieldAndPropertyInfos)
            {
                FieldInfo field = member as FieldInfo;
                PropertyInfo property = member as PropertyInfo;

                object[] allAtributes = member.GetCustomAttributes(true);
                if (allAtributes != null)
                {
                    JsonNameAttribute nameAttribute = allAtributes.FirstOrDefault(attribute => attribute is JsonNameAttribute) as JsonNameAttribute;

                    object memberValue = property == null ? field.GetValue(this) : property.GetValue(this, null);

                    if (nameAttribute != null && memberValue != null && (elementsToIncludeSet == null || elementsToIncludeSet.Contains(nameAttribute.Name)))
                    {
                        string jsonPropertyName = nameAttribute.Name;

                        Type propertyType = property == null ? field.FieldType : property.PropertyType;

                        if (propertyType.IsGenericType && propertyType.GetGenericTypeDefinition() == typeof(Nullable<>))
                        {
                            propertyType = propertyType.GetGenericArguments()[0];
                        }

                        if (propertyType.IsSubclassOf(typeof(JsonConvertibleObject)))
                        {
                            result.Add(jsonPropertyName, ((JsonConvertibleObject)memberValue).ToJsonIntermediateObject());
                        }
                        else if (propertyType.IsEnum)
                        {
                            string value = memberValue.ToString();
                            JsonNameAttribute[] attributes = (JsonNameAttribute[])propertyType.GetMember(value)[0].GetCustomAttributes(typeof(JsonNameAttribute), false);

                            if (attributes.Length != 0)
                            {
                                result.Add(jsonPropertyName, attributes[0].Name);
                            }
                            else
                            {
                                result.Add(jsonPropertyName, memberValue.ToString());
                            }

                        }
                        else
                        {
                            if (memberValue != null)
                            {
                                result.Add(jsonPropertyName, memberValue);
                            }
                        }
                    }
                }
            }
      
            return result;
        }

        /// <summary>
        /// Goes through a deserialized JSON object (a Dictionary&ltstring, object&gt or a newtonsoft JObject) and updates all field an properties of this instance.
        /// </summary>
        /// <param name="value">The value.</param>
        public void FromJsonIntermediateObject(object value)
        {
            Type type = this.GetType();

            MemberInfo[] fieldAndPropertyInfos = type.GetProperties().Select(prop => (MemberInfo)prop).Union(type.GetFields().Select(field => (MemberInfo)field)).ToArray();

            if (value == null)
            {
                //TODO: what should the method do then?
                return;
            }

            // we do this here so there's no need to repeatedly cast the object within each pass of each for loop
            JObject valueAsJObject = value as JObject;
            Dictionary<string, object> valueAsDictionary = value as Dictionary<string, object>;

            foreach (MemberInfo member in fieldAndPropertyInfos)
            {
                try
                {
                    FieldInfo field = member as FieldInfo;
                    PropertyInfo property = member as PropertyInfo;
                    object[] allAtributes = member.GetCustomAttributes(true);

                    if (allAtributes == null)
                    {
                        continue;
                    }

                    JsonNameAttribute nameAttribute = allAtributes.FirstOrDefault(attribute => attribute is JsonNameAttribute) as JsonNameAttribute;

                    if (nameAttribute == null)
                    {
                        continue;
                    }

                    string jsonPropertyName = nameAttribute.Name;

                    Type propertyType = field == null ? property.PropertyType : field.FieldType;

                    if (propertyType.IsGenericType && propertyType.GetGenericTypeDefinition() == typeof(Nullable<>))
                    {
                        propertyType = propertyType.GetGenericArguments()[0];
                    }

                    if (propertyType.IsSubclassOf(typeof(JsonConvertibleObject)))
                    {
                        if (valueAsJObject[jsonPropertyName] != null || valueAsDictionary[jsonPropertyName] != null)
                        {
                            JsonConvertibleObject finalValue = (JsonConvertibleObject)propertyType.GetConstructor(new Type[0]).Invoke(null);

                            if (value.GetType() == typeof(JObject))
                            {
                                finalValue.FromJsonIntermediateObject(valueAsJObject[jsonPropertyName]);
                            }
                            else if (value.GetType() == typeof(Dictionary<string, object>))
                            {
                                finalValue.FromJsonIntermediateObject(valueAsDictionary[jsonPropertyName]);
                            }
                            else
                            {
                                throw new FormatException("Unsupported intermediate format");
                            }

                            if (property != null)
                            {
                                property.SetValue(this, finalValue, null);
                            }
                            else
                            {
                                field.SetValue(this, finalValue);
                            }

                        }
                    }
                    else if (propertyType.IsEnum)
                    {
                        object enumValue = null;
                        if (valueAsJObject != null)
                        {
                            enumValue = valueAsJObject[jsonPropertyName].Value<string>();
                        }

                        if (enumValue is string)
                        {
                            object valueToSet = 0;
                            bool foundMatch = false;

                            foreach (string possibleValue in Enum.GetNames(propertyType))
                            {
                                JsonNameAttribute[] jsonNameAttributes = (JsonNameAttribute[])propertyType.GetMember(possibleValue)[0].GetCustomAttributes(typeof(JsonNameAttribute), false);
                                if (jsonNameAttributes.Length != 0 && jsonNameAttributes[0].Name.ToLowerInvariant() == (enumValue as string).ToLowerInvariant())
                                {
                                    foundMatch = true;
                                    valueToSet = Enum.Parse(propertyType, possibleValue, true);
                                    break;
                                }
                            }

                            if (!foundMatch)
                            {
                                valueToSet = Enum.Parse(propertyType, enumValue as string, true);
                            }

                            if (property != null)
                            {
                                property.SetValue(this, valueToSet, null);
                            }
                            else
                            {
                                field.SetValue(this, valueToSet);
                            }
                        }
                    }
                    else
                    {
                        if (value.GetType() == typeof(JObject))
                        {
                            MethodInfo method = value.GetType().GetMethod("ToObject", new Type[] { });
                            MethodInfo genericMethod = method.MakeGenericMethod(new Type[] { propertyType });

                            if (property != null)
                            {
                                property.SetValue(this, genericMethod.Invoke(valueAsJObject[jsonPropertyName], null), null);
                            }
                            else
                            {
                                field.SetValue(this, genericMethod.Invoke(valueAsJObject[jsonPropertyName], null));
                            }
                        }
                        else if (value.GetType() == typeof(Dictionary<string, object>))
                        {
                            if (property != null)
                            {
                                property.SetValue(this, valueAsDictionary[jsonPropertyName], null);
                            }
                            else
                            {
                                field.SetValue(this, valueAsDictionary[jsonPropertyName]);
                            }
                        }
                        else
                        {
                            throw new FormatException("Unsupported intermediate format");
                        }
                    }
                }
                catch //TODO: this is very very bad, this needs to catch exactly the exceptions we're expecting.
                {
                }
            }
        }

        /// <summary>
        /// Converts an object into another type.
        /// If the object is a JObject or JArray, this method uses their respective methods for conversion.
        /// Otherwise, it uses Convert.ChangeType.
        /// </summary>
        /// <typeparam name="T">The type to convert to.</typeparam>
        /// <param name="value">The object to convert.</param>
        /// <returns>The converted object.</returns>
        public static T ObjectToValue<T>(object value)
        {
            JObject jObject = value as JObject;

            if (jObject != null)
            {
                return jObject.ToObject<T>();
            }

            JArray jArray = value as JArray;

            if (jArray != null)
            {
                return jArray.ToObject<T>();
            }

            string stringValue = value as string;

            if (typeof(T).IsEnum)
            {
                if (stringValue != null)
                {
                    object valueToSet = 0;
                    bool foundMatch = false;

                    foreach (string possibleValue in Enum.GetNames(typeof(T)))
                    {
                        JsonNameAttribute[] jsonNameAttributes = (JsonNameAttribute[])typeof(T).GetMember(possibleValue)[0].GetCustomAttributes(typeof(JsonNameAttribute), false);
                        if (jsonNameAttributes.Length != 0)
                        {
                            if (jsonNameAttributes[0].Name.ToLowerInvariant() == stringValue.ToLowerInvariant())
                            {
                                foundMatch = true;
                                valueToSet = Enum.Parse(typeof(T), possibleValue, true);
                                break;
                            }
                        }
                    }

                    if (!foundMatch)
                    {
                        valueToSet = Enum.Parse(typeof(T), stringValue, true);
                    }

                    return (T)valueToSet;
                }
            }

            if (value is T)
            {
                return (T)value;
            }
            else
            {
                return (T)Convert.ChangeType(value, typeof(T), CultureInfo.InvariantCulture);
            }
        }
    }
}
