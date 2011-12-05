// -----------------------------------------------------------------------
// <copyright file="JsonExtensions.cs" company="Uhuru Software">
// </copyright>
// -----------------------------------------------------------------------

namespace Uhuru.Utilities
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    
    /// <summary>
    /// This class is used to extend all objects with Json serialization methods.
    /// </summary>
    public static class JsonExtensions
    {
        private static readonly object serializationLock = new object();

        /// <summary>
        /// Serializes an object into a JSON string.
        /// </summary>
        /// <typeparam name="T">Type of the object to serialize.</typeparam>
        /// <param name="obj">The object to serialize.</param>
        /// <returns>A string containing the JSON object.</returns>
        public static string ToJson<T>(this T obj) where T : class
        {
            Type type = typeof(T);
            if (type.IsArray)
            {
                lock (serializationLock)
                {
                    return JArray.FromObject(obj).ToString(Formatting.None);
                }
            }
            else if (type.IsGenericType && (
                type.GetGenericTypeDefinition() == typeof(Dictionary<,>) || 
                type.GetGenericTypeDefinition() == typeof(IDictionary<,>)))
            {
                lock (serializationLock)
                {
                    return JObject.FromObject(obj).ToString(Formatting.None);
                }
            }
            else if (type.Equals(typeof(JObject)))
            {
                return ((JObject)(object)obj).ToString();
            }
            else if (type.Equals(typeof(JArray)))
            {
                return ((JArray)(object)obj).ToString();
            }
            else if (type.IsGenericType && (
                type.GetGenericTypeDefinition() == typeof(IEnumerable<>) || 
                type.GetGenericTypeDefinition() == typeof(List<>)))
            {
                lock (serializationLock)
                {
                    return JArray.FromObject(obj).ToString(Formatting.None);
                }
            }
            else
            {
                lock (serializationLock)
                {
                    return JObject.FromObject(obj).ToString(Formatting.None);
                }
            }
        }

        /// <summary>
        /// Deserializes a JSON string to an object.
        /// </summary>
        /// <typeparam name="T">The type of the object to convert to.</typeparam>
        /// <param name="value">The object to con</param>
        /// <param name="json">The JSON string to deserialize.</param>
        /// <returns>The deserialized object.</returns>
        public static T FromJson<T>(this T value, string json) where T : class
        {
            if (value == null)
            {
                throw new ArgumentNullException("value");
            }

            Type type = typeof(T);
            if (type.IsArray)
            {
                lock (serializationLock)
                {
                    return JArray.Parse(json).ToObject<T>();
                }
            }
            else
            {
                lock (serializationLock)
                {
                    return JObject.Parse(json).ToObject<T>();
                }
            }
        }

        /// <summary>
        /// Helper method for converting a JArray or JObject into another object.
        /// </summary>
        /// <typeparam name="T">Type of the object into which to convert.</typeparam>
        /// <param name="value">Value to convert.</param>
        /// <returns>The converted object or null if conversion is not possible.</returns>
        public static T ToObject<T>(this object value) where T : class
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

            return null;
        }

        /// <summary>
        /// Converts an object into another type.
        /// If the object is a JObject or JArray, this method uses their respective methods for conversion.
        /// Otherwise, it uses Convert.ChangeType.
        /// </summary>
        /// <typeparam name="T">The type to convert to.</typeparam>
        /// <param name="value">The object to convert.</param>
        /// <returns>The converted object.</returns>
        public static T ToValue<T>(this object value)
        {
            JObject jObject = value as JObject;

            if (jObject != null)
            {
                return jObject.Value<T>();
            }

            JArray jArray = value as JArray;

            if (jArray != null)
            {
                return jArray.Value<T>();
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
