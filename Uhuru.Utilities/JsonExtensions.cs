using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.IO;
using System.Text;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;

namespace Uhuru.Utilities
{
    public static class JsonExtensions
    {
        private static readonly object serializationLock = new object();

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

        public static T FromJson<T>(this T obj, string json) where T : class
        {
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

        public static T ToObject<T>(this object obj) where T : class
        {
            if (obj is JObject)
            {
                return ((JObject)obj).ToObject<T>();
            }

            if (obj is JArray)
            {
                return ((JArray)obj).ToObject<T>();
            }

            return null;
        }

        public static T ToValue<T>(this object obj)
        {
            if (obj is JObject)
            {
                return ((JObject)obj).Value<T>();
            }

            if (obj is JArray)
            {
                return ((JArray)obj).Value<T>();
            }

           
            if (obj is T)
            {
                return (T)obj;
            }
            else
            {
                return (T)Convert.ChangeType(obj, typeof(T));
            }
        }
    }
}
