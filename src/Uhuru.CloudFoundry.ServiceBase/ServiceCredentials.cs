// -----------------------------------------------------------------------
// <copyright file="ServiceCredentials.cs" company="Uhuru Software, Inc.">
// Copyright (c) 2011 Uhuru Software, Inc., All Rights Reserved
// </copyright>
// -----------------------------------------------------------------------

namespace Uhuru.CloudFoundry.ServiceBase
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Uhuru.Utilities;
    using Uhuru.Utilities.Json;

    /// <summary>
    /// This class contains information about service credentials.
    /// </summary>
    public class ServiceCredentials : JsonConvertibleObject
    {
        /// <summary>
        /// Lock used to synchronize <see cref="servicesTaskFactories"/>.
        /// </summary>
        private static readonly object factoriesLock = new object();
        
        /// <summary>
        /// Contains task factories keyed by service names; factories are used to allow parallel work for services, while the work for one service is done on a single thread.
        /// </summary>
        private static Dictionary<string, TaskFactory> servicesTaskFactories;

        /// <summary>
        /// Contains weak references pointing to instances that have the same service name.
        /// </summary>
        private static Dictionary<string, HashSet<WeakReference>> serviceNamesInstances;

        /// <summary>
        /// Binding options for the service credentials settings.
        /// </summary>
        private Dictionary<string, object> bindOptions = new Dictionary<string, object>();

        /// <summary>
        /// The service name.
        /// </summary>
        private string name;

        /// <summary>
        /// A weak reference to this.
        /// </summary>
        private WeakReference weakThis;

        /// <summary>
        /// Initializes a new instance of the <see cref="ServiceCredentials"/> class.
        /// </summary>
        public ServiceCredentials()
        {
            this.weakThis = new WeakReference(this);
        }

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
            get
            {
                return this.name;
            }

            set
            {
                if (value == null)
                {
                    value = string.Empty;
                }

                lock (factoriesLock)
                {
                    this.name = value;

                    if (ServiceNamesInstances.ContainsKey(this.name))
                    {
                        ServiceNamesInstances[this.name].Remove(this.weakThis);
                    }

                    if (!ServiceNamesInstances.ContainsKey(this.name))
                    {
                        ServiceNamesInstances[this.name] = new HashSet<WeakReference>();
                    }

                    serviceNamesInstances[this.name].Add(this.weakThis);
                }
            }
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
        /// Gets a task factory that schedules work for the same service on the same thread.
        /// </summary>
        public TaskFactory ServiceWorkFactory
        {
            get
            {
                TaskFactory factory = null;

                if (ServicesTaskFactories.ContainsKey(this.Name))
                {
                    factory = servicesTaskFactories[this.Name];
                }
                else
                {
                    factory = new TaskFactory(TaskCreationOptions.LongRunning, TaskContinuationOptions.ExecuteSynchronously);
                    lock (factoriesLock)
                    {
                        servicesTaskFactories[this.Name] = factory;

                        // we schedule a garbage collector to run every 10 seconds, so it collects all factories that are no longer in use.
                        TimerHelper.RecurringLongCall(
                            10000,
                            () =>
                            {
                                lock (factoriesLock)
                                {
                                    string[] serviceNames = ServicesTaskFactories.Keys.ToArray();

                                    foreach (string name in serviceNames)
                                    {
                                        int usageCount = 0;
                                        if (ServiceNamesInstances.ContainsKey(name))
                                        {
                                            usageCount = ServiceNamesInstances[name].Count(weakReference => weakReference.IsAlive);
                                        }

                                        if (usageCount == 0)
                                        {
                                            ServiceNamesInstances.Remove(name);
                                            ServicesTaskFactories.Remove(name);
                                        }
                                    }
                                }
                            });
                    }
                }

                return factory;
            }
        }

        /// <summary>
        /// Gets or sets the bind options for the provisioned service.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly", Justification = "The setter is used by the JSON convertible object"), JsonName("bind_opts")]
        public Dictionary<string, object> BindOptions
        { 
            get
            {
                return this.bindOptions;
            }

            set
            {
                this.bindOptions = value;
            }
        }

        /// <summary>
        /// Gets a hash of task factories, keyed by service names.
        /// </summary>
        private static Dictionary<string, TaskFactory> ServicesTaskFactories
        {
            get
            {
                if (servicesTaskFactories != null)
                {
                    return servicesTaskFactories;
                }
                else
                {
                    lock (factoriesLock)
                    {
                        if (servicesTaskFactories == null)
                        {
                            servicesTaskFactories = new Dictionary<string, TaskFactory>();
                        }

                        return servicesTaskFactories;
                    }
                }
            }
        }

        /// <summary>
        /// Gets the service names instances.
        /// </summary>
        private static Dictionary<string, HashSet<WeakReference>> ServiceNamesInstances
        {
            get
            {
                if (serviceNamesInstances != null)
                {
                    return serviceNamesInstances;
                }
                else
                {
                    lock (factoriesLock)
                    {
                        if (serviceNamesInstances == null)
                        {
                            serviceNamesInstances = new Dictionary<string, HashSet<WeakReference>>();
                        }

                        return serviceNamesInstances;
                    }
                }
            }
        }
    }
}