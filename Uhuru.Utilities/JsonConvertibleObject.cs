// -----------------------------------------------------------------------
// <copyright file="JsonConvertibleObject.cs" company="Uhuru Software">
// </copyright>
// -----------------------------------------------------------------------

namespace Uhuru.Utilities
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;

    public class JsonNameAttribute : Attribute
    {
        public string Name
        {
            get;
            set;
        }

        public JsonNameAttribute(string name)
        {
            Name = name;
        }
    }

    public class JsonConvertibleObject
    {
        public static List<object> DeserializeFromJsonArray(string json)
        {
            return ((JArray)DeserializeFromJson(json)).ToObject<List<object>>();
        }

        public static object DeserializeFromJson(string json)
        {
            return JsonConvert.DeserializeObject(json);
        }

        public static string SerializeToJson(object intermediateObject)
        {
            return JsonConvert.SerializeObject(intermediateObject);
        }

        public string SerializeToJson()
        {
            return JsonConvert.SerializeObject(this.ToJsonIntermediateObject());
        }

        //todo: use in the future for deep transformation
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
                //throw new Exception("Unsuported Intermediate Format");
            }

            return result;
        }

        //todo: use in the future for deap trandormation
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
                        else
                        {
                            if (property.GetValue(this, null) != null) result.Add(jsonPropertyName, property.GetValue(this, null));
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
                        else
                        {
                            if (field.GetValue(this) != null) result.Add(jsonPropertyName, field.GetValue(this));
                        }
                    }
                }
            }

            return result;
        }

        public void FromJsonIntermediateObject(object obj)
        {
            Type type = this.GetType();
            
            PropertyInfo[] propertyInfos = type.GetProperties();

            // we do this here so there's no need to repeatedly cast the object within each pass of each for loop
            JObject objectAsJObject = obj as JObject;
            Dictionary<string, object> objectAsDictionary = obj as Dictionary<string, object>;

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
                                JsonConvertibleObject finalValue = (JsonConvertibleObject)propertyType.GetConstructor(new Type[0]).Invoke(null);

                                if (obj.GetType() == typeof(JObject))
                                {
                                    finalValue.FromJsonIntermediateObject(objectAsJObject[jsonPropertyName]);
                                }
                                else if (obj.GetType() == typeof(Dictionary<string, object>))
                                {
                                    finalValue.FromJsonIntermediateObject(objectAsDictionary[jsonPropertyName]);
                                }
                                else
                                {
                                    throw new Exception("Unsupported intermediate format");
                                }

                                property.SetValue(this, finalValue, null);
                            }
                            else //if (propertyType.IsPrimitive) //we are a primitive type
                            {
                                if (obj.GetType() == typeof(JObject))
                                {
                                    MethodInfo method = obj.GetType().GetMethod("ToObject", new Type[] { });
                                    MethodInfo genericMethod = method.MakeGenericMethod(new Type[] { propertyType });
                                    property.SetValue(this, genericMethod.Invoke(objectAsJObject[jsonPropertyName], null), null);
                                }
                                else if (obj.GetType() == typeof(Dictionary<string, object>))
                                {
                                    property.SetValue(this, objectAsDictionary[jsonPropertyName], null);
                                }
                                else
                                {
                                    throw new Exception("Unsupported intermediate format");
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
                                JsonConvertibleObject finalValue = (JsonConvertibleObject)propertyType.GetConstructor(new Type[0]).Invoke(null);
                                
                                if (obj.GetType() ==  typeof(JObject))
                                {
                                    finalValue.FromJsonIntermediateObject(objectAsJObject[jsonPropertyName]);
                                }
                                else if (obj.GetType() == typeof(Dictionary<string, object>))
                                {
                                    finalValue.FromJsonIntermediateObject(objectAsDictionary[jsonPropertyName]);
                                }
                                else
                                {
                                    throw new Exception("Unsupported intermediate format");
                                }

                                field.SetValue(this, finalValue);
                            }
                            else //if (propertyType.IsPrimitive) //we are a primitive type
                            {
                                if (obj.GetType() == typeof(JObject))
                                {
                                    MethodInfo method = obj.GetType().GetMethod("ToObject", new Type[] { });
                                    MethodInfo genericMethod = method.MakeGenericMethod(new Type[] { propertyType });
                                    field.SetValue(this, genericMethod.Invoke(objectAsJObject[jsonPropertyName], null));
                                }
                                else if (obj.GetType() == typeof(Dictionary<string, object>))
                                {
                                    field.SetValue(this, objectAsDictionary[jsonPropertyName]);
                                }
                                else
                                {
                                    throw new Exception("Unsupported intermediate format");
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
    }
}

