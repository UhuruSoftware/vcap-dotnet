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
    /// <summary>
    /// This class contains information about service credentials.
    /// </summary>
    public class ServiceCredentials
    {
        private Dictionary<string, object> bindOptions = new Dictionary<string, object>();

        /// <summary>
        /// Gets or sets the Node ID.
        /// </summary>
        public string NodeId
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the provisioned service name.
        /// </summary>
        public string Name
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the username.
        /// </summary>
        public string UserName
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the username.
        /// </summary>
        public string User
        { 
            get; 
            set; 
        }

        /// <summary>
        /// Gets or sets the password.
        /// </summary>
        public string Password
        { 
            get; 
            set; 
        }

        /// <summary>
        /// Gets or sets the hostname of the provisioned service.
        /// </summary>
        public string HostName
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the port of the provisioned service.
        /// </summary>
        public int Port
        { 
            get; 
            set; 
        }

        /// <summary>
        /// Gets the bind options for the provisioned service.
        /// </summary>
        public Dictionary<string, object> BindOptions
        { 
            get
            {
                return bindOptions;
            }
        }

        /// <summary>
        /// Converts this object to a JSON-serialized string.
        /// </summary>
        /// <returns>A string containing the JSON object.</returns>
        public string ToJson()
        {
            return ToDictionary().ToJson();
        }

        /// <summary>
        /// Deserializes a JSON string and updates the members of this instance.
        /// </summary>
        /// <param name="json">The JSON string.</param>
        public void FromJson(string json)
        {
            Dictionary<string, object> jsonObject = new Dictionary<string, object>();
            jsonObject = jsonObject.FromJson(json);

            NodeId = jsonObject["node_id"].ToValue<string>();
            Name = jsonObject["name"].ToValue<string>();
            UserName = jsonObject["username"].ToValue<string>();
            User = jsonObject["user"].ToValue<string>();
            Password = jsonObject["password"].ToValue<string>();
            HostName = jsonObject["hostname"].ToValue<string>();
            Port = jsonObject["hostname"].ToValue<int>();
            bindOptions = jsonObject["bind_opts"].ToObject<Dictionary<string, object>>();
        }

        /// <summary>
        /// Converts this object to a JSON serializable object.
        /// </summary>
        /// <returns></returns>
        public Dictionary<string, object> ToDictionary()
        {
            return new Dictionary<string, object>()
              {
                  {"node_id", NodeId},
                  {"name", Name},
                  {"username", UserName},
                  {"user", User},
                  {"password", Password},
                  {"hostname", HostName},
                  {"port", Port},
                  {"bind_opts", BindOptions}
              };
        }

    }

    /// <summary>
    /// This is a class containing information about a provisioned service.
    /// </summary>
    class Handle
    {
        /// <summary>
        /// Gets or sets the service ID.
        /// </summary>
        public string ServiceId
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the service credentials.
        /// </summary>
        public ServiceCredentials Credentials
        {
            get;
            set;
        }

        public void FromJson(string json)
        {
            Dictionary<string, object> jsonObject = new Dictionary<string, object>();
            jsonObject = jsonObject.FromJson(json);

            ServiceId = jsonObject["service_id"].ToValue<string>();
            Credentials = new ServiceCredentials();
            Credentials.FromJson(jsonObject["credentials"].ToJson());
        }
    }

    /// <summary>
    /// This class contains announcement information for a service.
    /// </summary>
    public class Announcement
    {
        /// <summary>
        /// Gets or sets the id of the service.
        /// </summary>
        public string Id
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the available storage for the service.
        /// </summary>
        public int AvailableStorage
        {
            get;
            set;
        }

        /// <summary>
        /// Serializes the Announcement object to JSON.
        /// </summary>
        /// <returns>A string containing the JSON object.</returns>
        public string ToJson()
        {
            return ToDictionary().ToJson();
        }

        /// <summary>
        /// Deserializes a JSON string and updates the instance members.
        /// </summary>
        /// <param name="json">The JSON string.</param>
        public void FromJson(string json)
        {
            Dictionary<string, object> jsonObject = new Dictionary<string, object>();
            jsonObject = jsonObject.FromJson(json);

            Id = jsonObject["id"].ToValue<string>();
            AvailableStorage = jsonObject["available_storage"].ToValue<int>();
        }

        /// <summary>
        /// Converts the Announcement to a JSON serializable dictionary.
        /// </summary>
        /// <returns>A dictionary ready to be serialized to JSON.</returns>
        public Dictionary<string, object> ToDictionary()
        {
            return new Dictionary<string, object>()
              {
                  {"id", Id},
                  {"available_storage", AvailableStorage}
              };
        }

    }

    /// <summary>
    /// Enum detailing service plan types.
    /// </summary>
    public enum ProvisionedServicePlanType
    {
        /// <summary>
        /// Free plan.
        /// </summary>
        Free
    }

    /// <summary>
    /// Class containing information about a provisioned service.
    /// </summary>
    [Serializable]
    public class ProvisionedService
    {
        private static readonly object collectionLock = new object();
        private static List<ProvisionedService> services;
        private static string filename = String.Empty;

        /// <summary>
        /// Initializes a local database for provisioned services.
        /// </summary>
        /// <param name="dbFileName">Name of the db file.</param>
        public static void Initialize(string dbFileName)
        {
            filename = dbFileName;
            services = new List<ProvisionedService>();
            Load(dbFileName);
        }


        /// <summary>
        /// Initializes a new instance of the <see cref="ProvisionedService"/> class.
        /// Creates a new instance and adds it to a local collection, that is persisted.
        /// </summary>
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

        /// <summary>
        /// Removes this instance from the collection of provisioned services and saves the local database.
        /// </summary>
        /// <returns>A boolean value indicating whether the persistance was successful.</returns>
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


        /// <summary>
        /// Gets or sets the name of the provisioned service.
        /// </summary>
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

        /// <summary>
        /// Gets or sets the user of the provisioned service.
        /// </summary>
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

        /// <summary>
        /// Gets or sets the password for the provisioned service.
        /// </summary>
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

        /// <summary>
        /// Gets or sets the payment plan for the provisioned service.
        /// </summary>
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

        /// <summary>
        /// Gets or sets a value indicating whether quota was exceeded for the provsioned service.
        /// </summary>
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

        /// <summary>
        /// Saves this instance into the local database.
        /// </summary>
        /// <returns>A boolean value indicating whther persistance was successful.</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        public static bool Save()
        {
            try
            {
                ProvisionedService.SaveFile();
                return true;
            }
            catch (Exception ex)
            {
                Logger.Error(Strings.ProvisionedServiceListSaveErrorLogMessage, ex.ToString());
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

        /// <summary>
        /// Gets the avilable provisioned services.
        /// </summary>
        /// <returns>An array containing provisioned services.</returns>
        public static ProvisionedService[] GetInstances()
        {
            return services.ToArray();
        }

        /// <summary>
        /// Gets a provisioned service by its name.
        /// </summary>
        /// <param name="name">A service name.</param>
        /// <returns>The provisioned service, or null if the specified service name does not exist.</returns>
        public static ProvisionedService GetService(string name)
        {
            return GetInstances().FirstOrDefault(instance => instance.name == name);
        }
    }
}