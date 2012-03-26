// -----------------------------------------------------------------------
// <copyright file="PluginHelper.cs" company="Uhuru Software, Inc.">
// Copyright (c) 2011 Uhuru Software, Inc., All Rights Reserved
// </copyright>
// -----------------------------------------------------------------------

namespace Uhuru.CloudFoundry.DEA.PluginBase
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using Uhuru.CloudFoundry.DEA.PluginBase.Resources;
    using Uhuru.Utilities;
    using Uhuru.Utilities.Json;

    /// <summary>
    /// This is a helper class that provides easy methods to extract application settings from the variables that are given to a plugin.
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Plugin", Justification = "The word is in the dictionary, but the warning is still generated.")]
    public static class PluginHelper
    {
        /// <summary>
        /// Gets the parsed data for an application.
        /// </summary>
        /// <param name="appVariables">The unstructured app variables.</param>
        /// <returns>An ApplicationParsedData object, that contains structured information about the application.</returns>
        public static ApplicationParsedData GetParsedData(ApplicationVariable[] appVariables)
        {
            if (appVariables == null)
            {
                throw new ArgumentNullException("appVariables");
            }

            Dictionary<string, string> variablesHash = new Dictionary<string, string>(appVariables.Length);

            foreach (ApplicationVariable variable in appVariables)
            {
                variablesHash[variable.Name] = variable.Value;
            }

            ApplicationVariable[] variables = appVariables;

            ApplicationInfo appInfo = new ApplicationInfo();

            VcapApplication vcapApplication = new VcapApplication();
            vcapApplication.FromJsonIntermediateObject(JsonConvertibleObject.DeserializeFromJson(variablesHash[PluginBaseRes.VcapApplicationVariable]));
            
            appInfo.InstanceId = vcapApplication.InstanceId;
            appInfo.LocalIP = variablesHash[PluginBaseRes.VcapAppHostVariable];
            appInfo.Name = vcapApplication.Name;
            appInfo.Path = Path.Combine(variablesHash[PluginBaseRes.HomeVariable], "app");
            appInfo.Port = int.Parse(variablesHash[PluginBaseRes.VcapAppPortVariable], CultureInfo.InvariantCulture);
            appInfo.WindowsPassword = variablesHash[PluginBaseRes.VcapWindowsUserPasswordVariable];
            appInfo.WindowsUserName = variablesHash[PluginBaseRes.VcapWindowsUserVariable];

            string runtime = vcapApplication.Runtime;

            string servicesJson = variablesHash[PluginBaseRes.VcapServicesVariable];
            Dictionary<string, object[]> vcapProvisionedServices = new Dictionary<string, object[]>();
            List<ApplicationService> services = new List<ApplicationService>();
            vcapProvisionedServices = JsonConvertibleObject.ObjectToValue<Dictionary<string, object[]>>(JsonConvertibleObject.DeserializeFromJson(servicesJson));

            foreach (string serviceLabel in vcapProvisionedServices.Keys)
            {   
                foreach (object provisionedService in vcapProvisionedServices[serviceLabel])
                {
                    VcapProvisionedService service = new VcapProvisionedService();
                    service.FromJsonIntermediateObject(provisionedService);

                    ApplicationService appService = new ApplicationService(
                        service.Name, 
                        string.IsNullOrEmpty(service.Credentials.User) ? service.Credentials.Username : service.Credentials.User,
                        service.Credentials.Password, 
                        service.Credentials.Port, 
                        service.Plan, 
                        service.PlanOptions, 
                        string.IsNullOrEmpty(service.Credentials.Hostname) ? service.Credentials.Host : service.Credentials.Hostname,
                        service.Credentials.InstanceName, 
                        service.Label, 
                        service.Tags);

                    services.Add(appService);
                }
            }

            VcapPluginStagingInfo vcapPluginStagingInfo = new VcapPluginStagingInfo();
            vcapPluginStagingInfo.FromJsonIntermediateObject(JsonConvertibleObject.DeserializeFromJson(variablesHash[PluginBaseRes.VcapPluginStagingInfoVariable]));

            string logFilePath = Path.Combine(variablesHash[PluginBaseRes.HomeVariable], vcapPluginStagingInfo.Logs.AppLog);
            string errorLogFilePath = Path.Combine(variablesHash[PluginBaseRes.HomeVariable], vcapPluginStagingInfo.Logs.AppErrorLog);
            string startupLogFilePath = Path.Combine(variablesHash[PluginBaseRes.HomeVariable], vcapPluginStagingInfo.Logs.StartupLog);

            return new ApplicationParsedData(
                appInfo, 
                runtime, 
                variables, 
                services.ToArray(),
                vcapApplication.Urls,
                logFilePath, 
                errorLogFilePath, 
                startupLogFilePath, 
                vcapPluginStagingInfo.AutoWireTemplates);
        }

        /// <summary>
        /// Class for the vcap provisioned service
        /// </summary>
        private class VcapProvisionedService : JsonConvertibleObject
        {
            /// <summary>
            /// Gets or sets the name.
            /// </summary>
            /// <value>
            /// The name.
            /// </value>
            [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "Used for serialization"), JsonName("name")]
            public string Name
            {
                get;
                set;
            }

            /// <summary>
            /// Gets or sets the label.
            /// </summary>
            /// <value>
            /// The label.
            /// </value>
            [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "Used for serialization"), JsonName("label")]
            public string Label
            {
                get;
                set;
            }

            /// <summary>
            /// Gets or sets the plan.
            /// </summary>
            /// <value>
            /// The plan.
            /// </value>
            [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "Used for serialization"), JsonName("plan")]
            public string Plan
            {
                get;
                set;
            }

            /// <summary>
            /// Gets or sets the tags.
            /// </summary>
            /// <value>
            /// The tags.
            /// </value>
            [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "Used for serialization"), JsonName("tags")]
            public string[] Tags
            {
                get;
                set;
            }

            /// <summary>
            /// Gets or sets the vcap provisioned service credentials.
            /// </summary>
            /// <value>
            /// The credentials for the vcap provisioned service
            /// </value>
            [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "Used for serialization"), JsonName("credentials")]
            public VcapProvisionedServiceCredentials Credentials
            {
                get;
                set;
            }

            /// <summary>
            /// Gets or sets the plan options.
            /// </summary>
            /// <value>
            /// Plan options.
            /// </value>
            [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "Used for serialization"), JsonName("plan_options")]
            public Dictionary<string, object> PlanOptions 
            { 
                get; 
                set; 
            }
        }

        /// <summary>
        /// Class for the provisioned vcap service credentials
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1812:AvoidUninstantiatedInternalClasses", Justification = "Used for serialization")]
        private class VcapProvisionedServiceCredentials : JsonConvertibleObject
        {
            /// <summary>
            /// Gets or sets the name of the instance.
            /// </summary>
            /// <value>
            /// The name of the instance.
            /// </value>
            [JsonName("name")]
            public string InstanceName { get; set; }

            /// <summary>
            /// Gets or sets the hostname.
            /// </summary>
            /// <value>
            /// Hostname for the vcap provisioned service.
            /// </value>
            [JsonName("hostname")]
            public string Hostname { get; set; }

            /// <summary>
            /// Gets or sets the host.
            /// </summary>
            /// <value>
            /// Host of the vcap provisioned service.
            /// </value>
            [JsonName("host")]
            public string Host { get; set; }

            /// <summary>
            /// Gets or sets the port.
            /// </summary>
            /// <value>
            /// Port for the vcap provisioned service.
            /// </value>
            [JsonName("port")]
            public int Port { get; set; }

            /// <summary>
            /// Gets or sets the user.
            /// </summary>
            /// <value>
            /// User for the vcap provisioned service.
            /// </value>
            [JsonName("user")]
            public string User { get; set; }

            /// <summary>
            /// Gets or sets the username.
            /// </summary>
            /// <value>
            /// Username for the vcap provisioned service.
            /// </value>
            [JsonName("username")]
            public string Username { get; set; }

            /// <summary>
            /// Gets or sets the password.
            /// </summary>
            /// <value>
            /// Password for the vcap provisioned service.
            /// </value>
            [JsonName("password")]
            public string Password { get; set; }
        }

        /// <summary>
        /// Class for vcap plug-in staging info logs
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1812:AvoidUninstantiatedInternalClasses", Justification = "Used for serialization")]
        private class VcapPluginStagingInfoLogs : JsonConvertibleObject
        {
            /// <summary>
            /// Gets or sets the app error log.
            /// </summary>
            /// <value>
            /// The app error log.
            /// </value>
            [JsonName("app_error")]
            public string AppErrorLog
            {
                get;
                set;
            }

            /// <summary>
            /// Gets or sets the startup log.
            /// </summary>
            /// <value>
            /// The startup log.
            /// </value>
            [JsonName("startup")]
            public string StartupLog
            {
                get;
                set;
            }

            /// <summary>
            /// Gets or sets the application log.
            /// </summary>
            /// <value>
            /// The app log.
            /// </value>
            [JsonName("app")]
            public string AppLog
            {
                get;
                set;
            }
        }

        /// <summary>
        /// Class for vcap plug-in staging information
        /// </summary>
        private class VcapPluginStagingInfo : JsonConvertibleObject
        {
            /// <summary>
            /// Gets or sets the vcap plug-in staging info logs.
            /// </summary>
            /// <value>
            /// The logs.
            /// </value>
            [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "Used for serialization"), JsonName("logs")]
            public VcapPluginStagingInfoLogs Logs
            {
                get;
                set;
            }

            /// <summary>
            /// Gets or sets the auto wire templates.
            /// </summary>
            /// <value>
            /// The auto wire templates.
            /// </value>
            [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "Used for serialization"), JsonName("auto_wire_templates")]
            public Dictionary<string, string> AutoWireTemplates { get; set; }
        }

        /// <summary>
        /// The vcap application that is going to be installed
        /// </summary>
        private class VcapApplication : JsonConvertibleObject
        {
            /// <summary>
            /// Gets or sets the instance id.
            /// </summary>
            /// <value>
            /// The instance id.
            /// </value>
            [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "Used for serialization"), 
            JsonName("instance_id")]
            public string InstanceId
            {
                get;
                set;
            }

            /// <summary>
            /// Gets or sets the name.
            /// </summary>
            /// <value>
            /// The name.
            /// </value>
            [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "Used for serialization"), 
            JsonName("name")]
            public string Name
            {
                get;
                set;
            }

            /// <summary>
            /// Gets or sets the runtime.
            /// </summary>
            /// <value>
            /// The runtime.
            /// </value>
            [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "Used for serialization"), 
            JsonName("runtime")]
            public string Runtime
            {
                get;
                set;
            }

            /// <summary>
            /// Gets or sets the URLs of the app.
            /// </summary>
            /// <value>
            /// An array of strings.
            /// </value>
            [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "Used for serialization"), 
            JsonName("uris")]
            public string[] Urls
            {
                get;
                set;
            }
        }
    }
}
