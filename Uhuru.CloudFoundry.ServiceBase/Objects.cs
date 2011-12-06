using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using Uhuru.Utilities;

namespace Uhuru.CloudFoundry.ServiceBase
{
    /// <summary>
    /// This class contains information about service credentials.
    /// </summary>
    public class ServiceCredentials : JsonConvertibleObject
    {
        private Dictionary<string, object> bindOptions = new Dictionary<string, object>();

        /// <summary>
        /// Gets or sets the Node ID.
        /// </summary>
        [JsonName("node_id")]
        public string NodeId
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the provisioned service name.
        /// </summary>
        [JsonName("name")]
        public string Name
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the username.
        /// </summary>
        [JsonName("username")]
        public string UserName
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the username.
        /// </summary>
        [JsonName("user")]
        public string User
        { 
            get; 
            set; 
        }

        /// <summary>
        /// Gets or sets the password.
        /// </summary>
        [JsonName("password")]
        public string Password
        { 
            get; 
            set; 
        }

        /// <summary>
        /// Gets or sets the hostname of the provisioned service.
        /// </summary>
        [JsonName("hostname")]
        public string HostName
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the port of the provisioned service.
        /// </summary>
        [JsonName("port")]
        public int Port
        { 
            get; 
            set; 
        }

        /// <summary>
        /// Gets the bind options for the provisioned service.
        /// </summary>
        [JsonName("bind_opts")]
        public Dictionary<string, object> BindOptions
        { 
            get
            {
                return bindOptions;
            }
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
        [JsonName("service_id")]
        public string ServiceId
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the service credentials.
        /// </summary>
        [JsonName("credentials")]
        public ServiceCredentials Credentials
        {
            get;
            set;
        }
    }

    /// <summary>
    /// This class contains announcement information for a service.
    /// </summary>
    public class Announcement : JsonConvertibleObject
    {
        /// <summary>
        /// Gets or sets the id of the service.
        /// </summary>
        [JsonName("id")]
        public string Id
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the available storage for the service.
        /// </summary>
        [JsonName("available_storage")]
        public int AvailableStorage
        {
            get;
            set;
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
    public class ProvisionedService : JsonConvertibleObject
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
        [JsonName("name")]
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
        [JsonName("user")]
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
        [JsonName("password")]
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
        [JsonName("plan")]
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
        [JsonName("quota_exceeded")]
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