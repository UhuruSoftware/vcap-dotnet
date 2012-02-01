// -----------------------------------------------------------------------
// <copyright file="JsonConvertibleObject.cs" company="Uhuru Software, Inc.">
// Copyright (c) 2011 Uhuru Software, Inc., All Rights Reserved
// </copyright>
// -----------------------------------------------------------------------

namespace Uhuru.Utilities.Json
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Reflection;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;

    /// <summary>
    /// This object is used for serialization/deserialization of objects into/from JSON.
    /// </summary>
    public class JsonConvertibleObject
    {
        /// <summary>
        /// Deserializes a json string that is supposed to contain an array (i.e. [{"field1" : "value1"}, "value2", 0])
        /// </summary>
        /// <param name="json">The json string.</param>
        /// <returns>An array of objects</returns>
        public static object[] DeserializeFromJsonArray(string json)
        {
            return ((JArray)DeserializeFromJson(json)).ToObject<object[]>();
        }

        /// <summary>
        /// Deserializes json string that is supposed to contain an object (i.e. {"field1" : "value1"}).
        /// </summary>
        /// <param name="json">The json string.</param>
        /// <returns>An object.</returns>
        public static object DeserializeFromJson(string json)
        {
            return JsonConvert.DeserializeObject(json);
        }

        /// <summary>
        /// Serializes an intermediate object (a Dictionary&lt;string, object&gt; or a newtonsoft JObject) to a JSON string.
        /// </summary>
        /// <param name="intermediateValue">A Dictionary&lt;string, object&gt; or a newtonsoft JObject.</param>
        /// <returns>The JSON string.</returns>
        public static string SerializeToJson(object intermediateValue)
        {
            return JsonConvert.SerializeObject(intermediateValue);
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
            JObject jsonObject = value as JObject;

            if (jsonObject != null)
            {
                return jsonObject.ToObject<T>();
            }

            JArray jsonArray = value as JArray;

            if (jsonArray != null)
            {
                return jsonArray.ToObject<T>();
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
                            if (jsonNameAttributes[0].Name.ToUpperInvariant() == stringValue.ToUpperInvariant())
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

        /// <summary>
        /// Serializes the instance to a JSON string.
        /// </summary>
        /// <returns>The JSON string.</returns>
        public string SerializeToJson()
        {
            return JsonConvert.SerializeObject(this.ToJsonIntermediateObject());
        }

        /// <summary>
        /// Converts this instance to a Dictionary&lt;string, object&gt; that is ready to be serialized to a Ruby-compatible JSON.
        /// </summary>
        /// <returns>A Dictionary&lt;string, object&gt;</returns>
        public Dictionary<string, object> ToJsonIntermediateObject()
        {
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

                    if (nameAttribute != null && memberValue != null)
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
        /// Goes through a deserialized JSON object (a Dictionary&lt;string, object&gt; or a newtonsoft JObject) and updates all field an properties of this instance.
        /// </summary>
        /// <param name="value">The value.</param>
        public void FromJsonIntermediateObject(object value)
        {
            if (value == null)
            {
                // TODO: what should the method do then?
                return;
            }

            Type type = this.GetType();
            MemberInfo[] fieldAndPropertyInfos = type.GetProperties().Select(prop => (MemberInfo)prop).Union(type.GetFields().Select(field => (MemberInfo)field)).ToArray();

            // we do this here so there's no need to repeatedly cast the object within each pass of each for loop
            JObject valueAsJObject = value as JObject;
            Dictionary<string, object> valueAsDictionary = value as Dictionary<string, object>;

            if (valueAsJObject == null && valueAsDictionary == null)
            {
                throw new FormatException("Unsupported intermediate format");
            }

            foreach (MemberInfo member in fieldAndPropertyInfos)
            {
                FieldInfo field = member as FieldInfo;
                PropertyInfo property = member as PropertyInfo;
                object[] allAtributes = member.GetCustomAttributes(true);

                // If we don't have any custom attributes for this member, ignore it
                if (allAtributes == null)
                {
                    continue;
                }

                JsonNameAttribute nameAttribute = allAtributes.FirstOrDefault(attribute => attribute is JsonNameAttribute) as JsonNameAttribute;

                // If we don't have a JsonName attribute for this member, ignore it
                if (nameAttribute == null)
                {
                    continue;
                }

                string jsonPropertyName = nameAttribute.Name;

                Type memberType = field == null ? property.PropertyType : field.FieldType;

                object memberValue = null;

                if (valueAsJObject != null && valueAsJObject[jsonPropertyName] != null)
                {
                    memberValue = valueAsJObject[jsonPropertyName];
                }
                else if (valueAsDictionary != null && valueAsDictionary.ContainsKey(jsonPropertyName))
                {
                    memberValue = valueAsDictionary[jsonPropertyName];
                }

                this.SetMemberValue(member, ConvertMember(memberValue, memberType));
            }
        }

        /// <summary>
        /// Converts an individual member.
        /// </summary>
        /// <param name="memberValue">The member value.</param>
        /// <param name="memberType">Type of the member.</param>
        /// <returns>The converted memeber.</returns>
        private static object ConvertMember(object memberValue, Type memberType)
        {
            Type actualMemberType = memberType;

            // If the type is nullable, get the actual type (e.g. int? is actually Nullable<int>)
            if (memberType.IsGenericType && memberType.GetGenericTypeDefinition() == typeof(Nullable<>))
            {
                actualMemberType = memberType.GetGenericArguments()[0];
            }

            if (actualMemberType.IsSubclassOf(typeof(JsonConvertibleObject)))
            {
                if (memberValue != null)
                {
                    JsonConvertibleObject finalValue = (JsonConvertibleObject)memberType.GetConstructor(new Type[0]).Invoke(null);
                    finalValue.FromJsonIntermediateObject(memberValue);
                    return finalValue;
                }
            }
            else if (actualMemberType.IsEnum)
            {
                object enumValue = memberValue;
                JValue enumValueJObject = enumValue as JValue;
                if (enumValueJObject != null)
                {
                    enumValue = enumValueJObject.Value<string>();
                }

                string strEnumValue = enumValue as string;

                if (strEnumValue == null)
                {
                    return null;
                }

                object valueToSet = GetEnumValueFromString(actualMemberType, strEnumValue);

                return valueToSet;
            }
            else
            {
                JToken memberValueAsJObject = memberValue as JToken;
                if (memberValueAsJObject != null)
                {
                    MethodInfo method = memberValueAsJObject.GetType().GetMethod("ToObject", new Type[] { });
                    MethodInfo genericMethod = method.MakeGenericMethod(new Type[] { memberType });
                    memberValue = genericMethod.Invoke(memberValueAsJObject, null);
                    return memberValue;
                }
            }

            return null;
        }

        /// <summary>
        /// Gets an enum value from a string. The method first tries to match the string value to any defined JsonName attributes, then defaults to Enum.Parse.
        /// </summary>
        /// <param name="enumType">Type of the enum.</param>
        /// <param name="enumValue">The enum value.</param>
        /// <returns>The parsed enum value.</returns>
        private static object GetEnumValueFromString(Type enumType, string enumValue)
        {
            foreach (string possibleValue in Enum.GetNames(enumType))
            {
                JsonNameAttribute[] jsonNameAttributes = (JsonNameAttribute[])enumType.GetMember(possibleValue)[0].GetCustomAttributes(typeof(JsonNameAttribute), false);
                if (jsonNameAttributes.Length != 0 && jsonNameAttributes[0].Name.ToUpperInvariant() == enumValue.ToUpperInvariant())
                {
                    return Enum.Parse(enumType, possibleValue, true);
                }
            }

            return Enum.Parse(enumType, enumValue, true);
        }

        /// <summary>
        /// Sets a member's value.
        /// </summary>
        /// <param name="member">The member (can be a field or a property).</param>
        /// <param name="value">The value.</param>
        private void SetMemberValue(MemberInfo member, object value)
        {
            // if the member isn't in the json object, this the value is null, don't set anything to the memeber.
            if (value == null)
            {
                return;
            }

            PropertyInfo property = member as PropertyInfo;
            FieldInfo field = member as FieldInfo;

            if (property != null)
            {
                property.SetValue(this, value, null);
            }

            if (field != null)
            {
                field.SetValue(this, value);
            }
        }
    }
}
