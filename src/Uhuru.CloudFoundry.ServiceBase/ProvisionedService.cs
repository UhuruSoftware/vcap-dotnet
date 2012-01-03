// -----------------------------------------------------------------------
// <copyright file="ProvisionedService.cs" company="Uhuru Software, Inc.">
// Copyright (c) 2011 Uhuru Software, Inc., All Rights Reserved
// </copyright>
// -----------------------------------------------------------------------

namespace Uhuru.CloudFoundry.ServiceBase
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Runtime.Serialization;
    using System.Xml.Serialization;
    using Uhuru.Utilities;
    using Uhuru.Utilities.Json;

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
        /// <summary>
        /// This is a lock object used to synchronize add/remove operations of this collection.
        /// </summary>
        private static readonly object collectionLock = new object();
        
        /// <summary>
        /// This is the internal list of provisioned services.
        /// </summary>
        private static List<ProvisionedService> services;
        
        /// <summary>
        /// The path to the file where the information about the provisioned services is saved.
        /// </summary>
        private static string filename = string.Empty;

        /// <summary>
        /// Name of the service.
        /// </summary>
        private string name;
        
        /// <summary>
        /// User used to connect to the service.
        /// </summary>
        private string user;
        
        /// <summary>
        /// Password for the service user.
        /// </summary>
        private string password;
        
        /// <summary>
        /// Billing plan for the service.
        /// </summary>
        private ProvisionedServicePlanType plan;
        
        /// <summary>
        /// Indicates whether quota has been exceeded.
        /// </summary>
        private bool quotaExceeded;

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
        /// Gets or sets the name of the provisioned service.
        /// </summary>
        [JsonName("name")]
        public string Name
        {
            get
            {
                return this.name;
            }

            set
            {
                this.name = value;
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
                return this.user;
            }

            set
            {
                this.user = value;
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
                return this.password;
            }

            set
            {
                this.password = value;
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
                return this.plan;
            }

            set
            {
                this.plan = value;
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
                return this.quotaExceeded;
            }

            set
            {
                this.quotaExceeded = value;
            }
        }

        /// <summary>
        /// Gets the available provisioned services.
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

        /// <summary>
        /// Initializes a local database for provisioned services.
        /// </summary>
        /// <param name="databaseFileName">Name of the db file.</param>
        public static void Initialize(string databaseFileName)
        {
            filename = databaseFileName;
            services = new List<ProvisionedService>();
            Load(databaseFileName);
        }

        /// <summary>
        /// Saves this instance into the local database.
        /// </summary>
        /// <returns>A boolean value indicating whther persistance was successful.</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "The exception is logged; caller is notified through return value.")]
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

        /// <summary>
        /// Removes this instance from the collection of provisioned services and saves the local database.
        /// </summary>
        /// <returns>A boolean value indicating whether the persistance was successful.</returns>
        public bool Destroy()
        {
            services.Remove(this);
            return Save();
        }

        /// <summary>
        /// Saves the provisioned services local database file.
        /// </summary>
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

        /// <summary>
        /// Loads saved provisioned information the specified filename.
        /// </summary>
        /// <param name="filename">The filename.</param>
        /// <returns>A value indicating whether the load was successful.</returns>
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
    }
}