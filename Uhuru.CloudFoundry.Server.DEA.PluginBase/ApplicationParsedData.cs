// -----------------------------------------------------------------------
// <copyright file="ApplicationParsedData.cs" company="Uhuru Software">
// Copyright (c) 2011 Uhuru Software, Inc., All Rights Reserved
// </copyright>
// -----------------------------------------------------------------------

namespace Uhuru.CloudFoundry.Server.DEA.PluginBase
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    /// <summary>
    /// This class contains structured application information.
    /// </summary>
    public class ApplicationParsedData
    {
        private ApplicationInfo appInfo;
        private string runtime;
        private ApplicationVariable[] variables;
        private ApplicationService[] services;
        private string logFilePath;
        private string errorLogFilePath;
        private string startupLogFilePath;
        private Dictionary<string, string> autoWireTemplates;

        /// <summary>
        /// Initializes a new instance of the <see cref="ApplicationParsedData"/> class.
        /// </summary>
        /// <param name="appInfo">The app info.</param>
        /// <param name="runtime">The runtime.</param>
        /// <param name="variables">The variables.</param>
        /// <param name="services">The services.</param>
        /// <param name="logFilePath">The log file path.</param>
        /// <param name="errorLogFilePath">The error log file path.</param>
        /// <param name="startupLogFilePath">The startup log file path.</param>
        /// <param name="autoWireTemplates">A list of connection string templates for services.</param>
        public ApplicationParsedData(ApplicationInfo appInfo, string runtime, ApplicationVariable[] variables, ApplicationService[] services,
            string logFilePath, string errorLogFilePath, string startupLogFilePath, Dictionary<string, string> autoWireTemplates)
        {
            this.appInfo = appInfo;
            this.runtime = runtime;
            this.variables = variables;
            this.services = services;
            this.logFilePath = logFilePath;
            this.errorLogFilePath = errorLogFilePath;
            this.startupLogFilePath = startupLogFilePath;
            this.autoWireTemplates = autoWireTemplates;
        }

        /// <summary>
        /// Gets the general application settings.
        /// </summary>
        public ApplicationInfo AppInfo
        {
            get
            {
                return appInfo;
            }
        }

        /// <summary>
        /// Gets the runtime for the application.
        /// </summary>
        public string Runtime
        {
            get
            {
                return runtime;
            }
        }

        /// <summary>
        /// Gets the application variables.
        /// </summary>
        /// <returns>An array of ApplicationVariable objects.</returns>
        public ApplicationVariable[] GetVariables()
        {
            return variables;
        }

        /// <summary>
        /// Gets the provisioned services for an app.
        /// </summary>
        /// <returns>An array of ApplicationService objects.</returns>
        public ApplicationService[] GetServices()
        {
            return services;
        }

        /// <summary>
        /// Gets the warn/info/debug/trace log file path.
        /// </summary>
        public string LogFilePath
        {
            get
            {
                return logFilePath;
            }
        }

        /// <summary>
        /// Gets the fatal/error log file path.
        /// </summary>
        public string ErrorLogFilePath
        {
            get
            {
                return errorLogFilePath;
            }
        }

        /// <summary>
        /// Gets the startup log file path.
        /// A plugin should write information in this file while it's starting the app.
        /// </summary>
        public string StartupLogFilePath
        {
            get
            {
                return startupLogFilePath;
            }
        }

        /// <summary>
        /// Gets the auto wire templates.
        /// </summary>
        public Dictionary<string, string> AutoWireTemplates
        {
            get 
            { 
                return autoWireTemplates; 
            }
        }
    }
}
