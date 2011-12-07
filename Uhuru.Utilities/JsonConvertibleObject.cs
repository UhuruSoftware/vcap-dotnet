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

    public sealed class JsonNameAttribute : Attribute
    {
        public JsonNameAttribute(string name)
        {
            Name = name;
        }
        
        public string Name
        {
            get;
            set;
        }
    }

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

            PropertyInfo[] properties = type.GetProperties();

            foreach (PropertyInfo property in properties)
            {
                object[] allAtributes = property.GetCustomAttributes(true);
                if (allAtributes != null)
                {
                    JsonNameAttribute nameAttribute = allAtributes.FirstOrDefault(attribute => attribute is JsonNameAttribute) as JsonNameAttribute;

                    if (nameAttribute != null && property.GetValue(this, null) != null && (elementsToIncludeSet == null || elementsToIncludeSet.Contains(nameAttribute.Name)))
                    {
                        string jsonPropertyName = nameAttribute.Name;

                        Type propertyType = property.PropertyType;

                        if (propertyType.IsSubclassOf(typeof(JsonConvertibleObject)))
                        {
                            result.Add(jsonPropertyName, ((JsonConvertibleObject)property.GetValue(this, null)).ToJsonIntermediateObject());
                        }
                        else if (propertyType.IsEnum)
                        {
                            string value = property.GetValue(this, null).ToString();
                            JsonNameAttribute[] attributes = (JsonNameAttribute[])propertyType.GetMember(value)[0].GetCustomAttributes(typeof(JsonNameAttribute), false);

                            if (attributes.Length != 0)
                            {
                                result.Add(jsonPropertyName, attributes[0].Name);
                            }
                            else
                            {
                                result.Add(jsonPropertyName, property.GetValue(this, null).ToString());
                            }

                        }
                        else
                        {
                            if (property.GetValue(this, null) != null)
                            {
                                result.Add(jsonPropertyName, property.GetValue(this, null));
                            }
                        }
                    }
                }
            }

            FieldInfo[] fields = type.GetFields();

            foreach (FieldInfo field in fields)
            {
                object[] allAtributes = field.GetCustomAttributes(true);
                if (allAtributes != null)
                {
                    JsonNameAttribute nameAttribute = allAtributes.FirstOrDefault(attribute => attribute is JsonNameAttribute) as JsonNameAttribute;

                    if (nameAttribute != null && field.GetValue(this) != null && (elementsToIncludeSet == null || elementsToIncludeSet.Contains(nameAttribute.Name)))
                    {
                        string jsonPropertyName = nameAttribute.Name;

                        Type propertyType = field.FieldType;

                        if (propertyType.IsSubclassOf(typeof(JsonConvertibleObject)))
                        {
                            result.Add(jsonPropertyName, ((JsonConvertibleObject)field.GetValue(this)).ToJsonIntermediateObject());
                        }
                        else if (propertyType.IsEnum)
                        {
                            string value = field.GetValue(this).ToString();
                            JsonNameAttribute[] attributes = (JsonNameAttribute[])propertyType.GetMember(value)[0].GetCustomAttributes(typeof(JsonNameAttribute), false);

                            if (attributes.Length != 0)
                            {
                                result.Add(jsonPropertyName, attributes[0].Name);
                            }
                            else
                            {
                                result.Add(jsonPropertyName, field.GetValue(this).ToString());
                            }
                        }
                        else
                        {
                            if (field.GetValue(this) != null)
                            {
                                result.Add(jsonPropertyName, field.GetValue(this));
                            }
                        }
                    }
                }
            }

            return result;
        }

        public void FromJsonIntermediateObject(object value)
        {
            Type type = this.GetType();

            PropertyInfo[] propertyInfos = type.GetProperties();

            if (value == null)
            {
                //TODO: what should the method do then?
                return;
            }

            // we do this here so there's no need to repeatedly cast the object within each pass of each for loop
            JObject valueAsJObject = value as JObject;
            Dictionary<string, object> valueAsDictionary = value as Dictionary<string, object>;

            foreach (PropertyInfo property in propertyInfos)
            {
                try
                {
                    object[] allAtributes = property.GetCustomAttributes(true);

                    if (allAtributes != null)
                    {
                        JsonNameAttribute nameAttribute = allAtributes.FirstOrDefault(attribute => attribute is JsonNameAttribute) as JsonNameAttribute;

                        if (nameAttribute != null)
                        {
                            string jsonPropertyName = nameAttribute.Name;

                            Type propertyType = property.PropertyType;

                            if (propertyType.IsSubclassOf(typeof(JsonConvertibleObject)))
                            {
                                if (valueAsJObject[jsonPropertyName] != null)
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

                                    property.SetValue(this, finalValue, null);
                                }
                            }
                            else if (propertyType.IsEnum)
                            {
                                if (valueAsJObject != null)
                                {
                                    value = valueAsJObject[jsonPropertyName].Value<string>();
                                }

                                if (value is string)
                                {
                                    object valueToSet = 0;
                                    bool foundMatch = false;

                                    foreach (string possibleValue in Enum.GetNames(propertyType))
                                    {
                                        JsonNameAttribute[] jsonNameAttributes = (JsonNameAttribute[])propertyType.GetMember(possibleValue)[0].GetCustomAttributes(typeof(JsonNameAttribute), false);
                                        if (jsonNameAttributes.Length != 0)
                                        {
                                            if (jsonNameAttributes[0].Name.ToLowerInvariant() == (value as string).ToLowerInvariant())
                                            {
                                                foundMatch = true;
                                                valueToSet = Enum.Parse(propertyType, possibleValue, true);
                                                break;
                                            }
                                        }
                                    }

                                    if (!foundMatch)
                                    {
                                        valueToSet = Enum.Parse(propertyType, value as string, true);
                                    }

                                    property.SetValue(this, valueToSet, null);
                                }
                            }
                            else // if (propertyType.IsPrimitive) //we are a primitive type
                            {
                                if (value.GetType() == typeof(JObject))
                                {
                                    MethodInfo method = value.GetType().GetMethod("ToObject", new Type[] { });
                                    MethodInfo genericMethod = method.MakeGenericMethod(new Type[] { propertyType });
                                    property.SetValue(this, genericMethod.Invoke(valueAsJObject[jsonPropertyName], null), null);
                                }
                                else if (value.GetType() == typeof(Dictionary<string, object>))
                                {
                                    property.SetValue(this, valueAsDictionary[jsonPropertyName], null);
                                }
                                else
                                {
                                    throw new FormatException("Unsupported intermediate format");
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                }
            }

            FieldInfo[] fields = type.GetFields();

            foreach (FieldInfo field in fields)
            {
                try
                {
                    object[] allAtributes = field.GetCustomAttributes(true);

                    if (allAtributes != null)
                    {
                        JsonNameAttribute nameAttribute = allAtributes.FirstOrDefault(attribute => attribute is JsonNameAttribute) as JsonNameAttribute;

                        if (nameAttribute != null)
                        {
                            string jsonPropertyName = nameAttribute.Name;

                            Type propertyType = field.FieldType;

                            if (propertyType.IsSubclassOf(typeof(JsonConvertibleObject)))
                            {
                                if (valueAsJObject[jsonPropertyName] != null)
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

                                    field.SetValue(this, finalValue);
                                }
                            }
                            else if (propertyType.IsEnum)
                            {
                                if (valueAsJObject != null)
                                {
                                    value = valueAsJObject[jsonPropertyName].Value<string>();
                                }

                                if (value is string)
                                {
                                    object valueToSet = 0;
                                    bool foundMatch = false;

                                    foreach (string possibleValue in Enum.GetNames(propertyType))
                                    {
                                        JsonNameAttribute[] jsonNameAttributes = (JsonNameAttribute[])propertyType.GetMember(possibleValue)[0].GetCustomAttributes(typeof(JsonNameAttribute), false);
                                        if (jsonNameAttributes.Length != 0)
                                        {
                                            if (jsonNameAttributes[0].Name.ToLowerInvariant() == (value as string).ToLowerInvariant())
                                            {
                                                foundMatch = true;
                                                valueToSet = Enum.Parse(propertyType, possibleValue, true);
                                                break;
                                            }
                                        }
                                    }

                                    if (!foundMatch)
                                    {
                                        valueToSet = Enum.Parse(propertyType, value as string, true);
                                    }

                                    field.SetValue(this, valueToSet);
                                }
                            }
                            else //if (propertyType.IsPrimitive) //we are a primitive type
                            {
                                if (value.GetType() == typeof(JObject))
                                {
                                    MethodInfo method = value.GetType().GetMethod("ToObject", new Type[] { });
                                    MethodInfo genericMethod = method.MakeGenericMethod(new Type[] { propertyType });
                                    field.SetValue(this, genericMethod.Invoke(valueAsJObject[jsonPropertyName], null));
                                }
                                else if (value.GetType() == typeof(Dictionary<string, object>))
                                {
                                    field.SetValue(this, valueAsDictionary[jsonPropertyName]);
                                }
                                else
                                {
                                    throw new FormatException("Unsupported intermediate format");
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
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

            if (typeof(T).IsEnum)
            {
                if (value is string)
                {
                    object valueToSet = 0;
                    bool foundMatch = false;

                    foreach (string possibleValue in Enum.GetNames(typeof(T)))
                    {
                        JsonNameAttribute[] jsonNameAttributes = (JsonNameAttribute[])typeof(T).GetMember(possibleValue)[0].GetCustomAttributes(typeof(JsonNameAttribute), false);
                        if (jsonNameAttributes.Length != 0)
                        {
                            if (jsonNameAttributes[0].Name.ToLowerInvariant() == (value as string).ToLowerInvariant())
                            {
                                foundMatch = true;
                                valueToSet = Enum.Parse(typeof(T), possibleValue, true);
                                break;
                            }
                        }
                    }

                    if (!foundMatch)
                    {
                        valueToSet = Enum.Parse(typeof(T), value as string, true);
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

        // todo: use in the future for deep transformation
        private static T TransformIntermediateElment<T>(object obj)
        {
            T result;
            Type toType = typeof(T);

            if (typeof(List<>) == toType.GetGenericTypeDefinition())
            {
                MethodInfo method = typeof(JsonConvertibleObject).GetMethod("TransformIntermediateList");
                MethodInfo genericMethod = method.MakeGenericMethod(new Type[] { toType.GetGenericArguments()[0] });
                result = (T)genericMethod.Invoke(null, new object[] { obj });
            }
            else
            {
                result = ((JToken)obj).ToObject<T>();
                // throw new Exception("Unsupported intermediate format");
            }

            return result;
        }

        // todo: use in the future for deep transformation
        private static List<T> TransformIntermediateList<T>(object obj)
        {
            Type elemType = typeof(T);
            List<T> result = new List<T>();
            foreach (JToken elem in (JArray)obj)
            {
                MethodInfo method = obj.GetType().GetMethod("ToObject", new Type[] { });
                MethodInfo genericMethod = method.MakeGenericMethod(new Type[] { elemType });
                result.Add((T)genericMethod.Invoke(obj, null));
            }

            return result;
        }
    }
}
