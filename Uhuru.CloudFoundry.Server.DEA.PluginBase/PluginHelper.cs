// -----------------------------------------------------------------------
// <copyright file="PluginHelper.cs" company="Uhuru Software">
// Copyright (c) 2011 Uhuru Software, Inc., All Rights Reserved
// </copyright>
// -----------------------------------------------------------------------

namespace Uhuru.CloudFoundry.Server.DEA.PluginBase
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using Uhuru.Utilities;
    using System.IO;

    /// <summary>
    /// This is a helper class that provides easy methods to extract application settings from the variables that are given to a plugin.
    /// </summary>
    public static class PluginHelper
    {
        private const string HomeVariable = "HOME";
        private const string VcapApplicationVariable = "VCAP_APPLICATION";
        private const string VcapServicesVariable = "VCAP_SERVICES";
        private const string VcapAppHostVariable = "VCAP_APP_HOST";
        private const string VcapAppPortVariable = "VCAP_APP_PORT";
        private const string VcapAppDebugIpVariable = "VCAP_DEBUG_IP";
        private const string VcapAppDebugPortVariable = "VCAP_DEBUG_PORT";
        private const string VcapPluginStagingInfoVariable = "VCAP_PLUGIN_STAGING_INFO";
        private const string VcapWindowsUserVariable = "VCAP_WINDOWS_USER";
        private const string VcapWindowsUserPasswordVariable = "VCAP_WINDOWS_USER_PASSWORD";

        /// <summary>
        /// Gets the parsed data for an application.
        /// </summary>
        /// <param name="appVariables">The unstructured app variables.</param>
        /// <returns>An ApplicationParsedData object, that contains structured information about the application.</returns>
        public static ApplicationParsedData GetParsedData(ApplicationVariable[] appVariables)
        {
            Dictionary<string, string> variablesHash = new Dictionary<string, string>(appVariables.Length);

            foreach (ApplicationVariable variable in appVariables)
            {
                variablesHash[variable.Name] = variable.Value;
            }

            ApplicationVariable[] variables = appVariables;

            ApplicationInfo appInfo = new ApplicationInfo();

            VcapApplication vcapApplication = new VcapApplication();
            vcapApplication.FromJsonIntermediateObject(JsonConvertibleObject.DeserializeFromJson(variablesHash[VcapApplicationVariable]));

            appInfo.InstanceId = vcapApplication.InstanceId;
            appInfo.LocalIp = variablesHash[VcapAppHostVariable];
            appInfo.Name = vcapApplication.Name;
            appInfo.Path = Path.Combine(variablesHash[HomeVariable], "app");
            appInfo.Port = Int32.Parse(variablesHash[VcapAppPortVariable]);
            appInfo.WindowsPassword = variablesHash[VcapWindowsUserPasswordVariable];
            appInfo.WindowsUsername = variablesHash[VcapWindowsUserVariable];

            string runtime = vcapApplication.Runtime;

            string servicesJson = variablesHash[VcapServicesVariable];
            Dictionary<string, object[]> vcapProvisionedServices = new Dictionary<string, object[]>();
            List<ApplicationService> services = new List<ApplicationService>();
            vcapProvisionedServices = JsonConvertibleObject.ObjectToValue<Dictionary<string, object[]>>( JsonConvertibleObject.DeserializeFromJson(servicesJson));

            foreach (string serviceLabel in vcapProvisionedServices.Keys)
            {
                
                foreach (object provisionedService in vcapProvisionedServices[serviceLabel])
                {
                    VcapProvisionedService service = new VcapProvisionedService();
                    service.FromJsonIntermediateObject(provisionedService);

                    ApplicationService appService = new ApplicationService();

                    appService.Name = service.Name;
                    appService.InstanceName = service.Credentials.InstanceName;
                    appService.Plan = service.Plan;
                    appService.PlanOptions = service.PlanOptions;
                    appService.Host = String.IsNullOrEmpty(service.Credentials.Hostname) ? service.Credentials.Host : service.Credentials.Hostname;
                    appService.User = String.IsNullOrEmpty(service.Credentials.User) ? service.Credentials.Username : service.Credentials.User;
                    appService.Password = service.Credentials.Password;
                    appService.Port = service.Credentials.Port;                                    
                    appService.ServiceLabel = service.Label;
                    appService.ServiceTags = service.Tags;
                }
            }


            VcapPluginStagingInfo vcapPluginStagingInfo = new VcapPluginStagingInfo();
            vcapPluginStagingInfo.FromJsonIntermediateObject(JsonConvertibleObject.DeserializeFromJson(variablesHash[VcapPluginStagingInfoVariable]));

            string logFilePath = Path.Combine(variablesHash[HomeVariable], vcapPluginStagingInfo.Logs.AppLog); ;
            string errorLogFilePath = Path.Combine(variablesHash[HomeVariable], vcapPluginStagingInfo.Logs.AppErrorLog); ;
            string startupLogFilePath = Path.Combine(variablesHash[HomeVariable], vcapPluginStagingInfo.Logs.StartupLog); ;


            return new ApplicationParsedData(appInfo, runtime, variables, services.ToArray(), logFilePath, 
                errorLogFilePath, startupLogFilePath, vcapPluginStagingInfo.AutoWireTemplates);
        }



        private class VcapProvisionedService : JsonConvertibleObject
        {
            [JsonName("name")]
            public string Name
            {
                get;
                set;
            }

            [JsonName("label")]
            public string Label
            {
                get;
                set;
            }

            [JsonName("plan")]
            public string Plan
            {
                get;
                set;
            }

            [JsonName("tags")]
            public string[] Tags
            {
                get;
                set;
            }

            [JsonName("credentials")]
            public VcapProvisionedServiceCredentials Credentials
            {
                get;
                set;
            }

            [JsonName("plan_options")]
            public Dictionary<string, object> PlanOptions 
            { 
                get; 
                set; 
            }
        }

        private class VcapProvisionedServiceCredentials : JsonConvertibleObject
        {
            [JsonName("name")]
            public string InstanceName { get; set; }

            [JsonName("hostname")]
            public string Hostname { get; set; }

            [JsonName("host")]
            public string Host { get; set; }

            [JsonName("port")]
            public int Port { get; set; }

            [JsonName("user")]
            public string User { get; set; }

            [JsonName("username")]
            public string Username { get; set; }

            [JsonName("password")]
            public string Password { get; set; }
        }

        private class VcapPluginStagingInfoLogs : JsonConvertibleObject
        {
            [JsonName("app_error")]
            public string AppErrorLog
            {
                get;
                set;
            }

            [JsonName("startup")]
            public string StartupLog
            {
                get;
                set;
            }

            [JsonName("app")]
            public string AppLog
            {
                get;
                set;
            }

        }

        private class VcapPluginStagingInfo : JsonConvertibleObject
        {
            [JsonName("logs")]
            public VcapPluginStagingInfoLogs Logs
            {
                get;
                set;
            }

            [JsonName("auto_wire_templates")]
            public Dictionary<string, string> AutoWireTemplates { get; set; }
        }

        private class VcapApplication : JsonConvertibleObject
        {
            [JsonName("instance_id")]
            public string InstanceId
            {
                get;
                set;
            }

            [JsonName("name")]
            public string Name
            {
                get;
                set;
            }

            [JsonName("runtime")]
            public string Runtime
            {
                get;
                set;
            }
        }
    }
}
