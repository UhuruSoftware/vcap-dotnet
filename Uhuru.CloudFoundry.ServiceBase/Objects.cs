using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Uhuru.Utilities;
using System.Xml.Serialization;
using System.IO;
using System.Runtime.Serialization;

namespace Uhuru.CloudFoundry.ServiceBase
{
    public class ServiceCredentials
    {
        public string NodeId
        {
            get;
            set;
        }

        public string Name
        {
            get;
            set;
        }

        public string Username
        {
            get;
            set;
        }

        public string User
        { 
            get; 
            set; 
        }

        public string Password
        { 
            get; 
            set; 
        }

        public string HostName
        {
            get;
            set;
        }

        public int Port
        { 
            get; 
            set; 
        }

        public Dictionary<string, object> BindOptions { get; set; }

        public string ToJson()
        {
            return ToDictionary().ToJson();
        }

        public void FromJson(string json)
        {
            Dictionary<string, object> jsonObject = new Dictionary<string, object>();
            jsonObject = jsonObject.FromJson(json);

            NodeId = jsonObject["node_id"].ToValue<string>();
            Name = jsonObject["name"].ToValue<string>();
            Username = jsonObject["username"].ToValue<string>();
            User = jsonObject["user"].ToValue<string>();
            Password = jsonObject["password"].ToValue<string>();
            HostName = jsonObject["hostname"].ToValue<string>();
            Port = jsonObject["hostname"].ToValue<int>();
            BindOptions = jsonObject["bind_opts"].ToObject<Dictionary<string, object>>();
        }

        public Dictionary<string, object> ToDictionary()
        {
            return new Dictionary<string, object>()
              {
                  {"node_id", NodeId},
                  {"name", Name},
                  {"username", Username},
                  {"user", User},
                  {"password", Password},
                  {"hostname", HostName},
                  {"port", Port},
                  {"bind_opts", BindOptions}
              };
        }

    }

    class Handle
    {
        public string ServiceId
        {
            get;
            set;
        }

        public ServiceCredentials Credentials
        {
            get;
            set;
        }

        public string ToJson()
        {
            return ToDictionary().ToJson();
        }

        public void FromJson(string json)
        {
            Dictionary<string, object> jsonObject = new Dictionary<string, object>();
            jsonObject = jsonObject.FromJson(json);

            ServiceId = jsonObject["service_id"].ToValue<string>();
            Credentials = new ServiceCredentials();
            Credentials.FromJson(jsonObject["credentials"].ToJson());
        }

        public Dictionary<string, object> ToDictionary()
        {
            return new Dictionary<string, object>()
              {
                  {"service_id", ServiceId},
                  {"credentials", Credentials.ToDictionary()}
              };
        }
    }

    public class Announcement
    {
        public string Id
        {
            get;
            set;
        }

        public int AvailableStorage
        {
            get;
            set;
        }

        public string ToJson()
        {
            return ToDictionary().ToJson();
        }

        public void FromJson(string json)
        {
            Dictionary<string, object> jsonObject = new Dictionary<string, object>();
            jsonObject = jsonObject.FromJson(json);

            Id = jsonObject["id"].ToValue<string>();
            AvailableStorage = jsonObject["available_storage"].ToValue<int>();
        }

        public Dictionary<string, object> ToDictionary()
        {
            return new Dictionary<string, object>()
              {
                  {"id", Id},
                  {"available_storage", AvailableStorage}
              };
        }

    }

    public enum ProvisionedServicePlanType
    {
        FREE
    }

    [Serializable]
    public class ProvisionedService
    {
        private static readonly object collectionLock = new object();
        private static List<ProvisionedService> services;
        private static string filename = String.Empty;

        public static void Initialize(string dbFilename)
        {
            filename = dbFilename;
            services = new List<ProvisionedService>();
            Load(dbFilename);
        }

        public ProvisionedService()
        {
            lock (collectionLock)
            {
                if (services == null)
                {
                    services = new List<ProvisionedService>();
                }
                services.Add(this);
            }
        }

        public bool Destroy()
        {
            services.Remove(this);
            return Save();
        }

        string name;
        string user;
        string password;
        ProvisionedServicePlanType plan;
        bool quotaExceeded;

        public string Name
        {
            get
            {
                return name;
            }
            set
            {
                name = value;
            }
        }

        public string User
        {
            get
            {
                return user;
            }
            set
            {
                user = value;
            }
        }

        public string Password
        {
            get
            {
                return password;
            }
            set
            {
                password = value;
            }
        }

        public ProvisionedServicePlanType Plan
        {
            get
            {
                return plan;
            }
            set
            {
                plan = value;
            }
        }

        public bool QuotaExceeded
        {
            get
            {
                return quotaExceeded;
            }
            set
            {
                quotaExceeded = value;
            }
        }

        public bool Save()
        {
            try
            {
                ProvisionedService.SaveFile();
                return true;
            }
            catch (Exception ex)
            {
               Logger.Error("Could not save ProvisionedService list {0}", ex.ToString());
                return false;
            }
        }

        private static void SaveFile()
        {
            lock (collectionLock)
            {
                XmlSerializer serializer = new XmlSerializer(typeof(List<ProvisionedService>));
                using (FileStream file = File.Open(filename, FileMode.Create, FileAccess.Write))
                {
                    serializer.Serialize(file, services);
                }
            }
        }

        private static bool Load(string filename)
        {
            lock (collectionLock)
            {
                try
                {
                    XmlSerializer serializer = new XmlSerializer(typeof(List<ProvisionedService>));
                    using (FileStream file = File.OpenRead(filename))
                    {
                        services = (List<ProvisionedService>)serializer.Deserialize(file);
                    }
                    return true;
                }
                catch (FileNotFoundException)
                {
                    return false;
                }
                catch (SerializationException)
                {
                    return false;
                }
            }
        }

        public static ProvisionedService[] Instances
        {
            get
            {
                return services.ToArray();
            }
        }

        public static ProvisionedService GetService(string name)
        {
            return Instances.FirstOrDefault(instance => instance.name == name);
        }
    }
}