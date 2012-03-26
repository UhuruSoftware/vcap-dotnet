// -----------------------------------------------------------------------
// <copyright file="ApplicationParsedData.cs" company="Uhuru Software, Inc.">
// Copyright (c) 2011 Uhuru Software, Inc., All Rights Reserved
// </copyright>
// -----------------------------------------------------------------------

namespace Uhuru.CloudFoundry.DEA.PluginBase
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// This class contains structured application information.
    /// </summary>
    public class ApplicationParsedData
    {
        /// <summary>
        /// ApplicationInfo structure containing structured information about an application
        /// </summary>
        private ApplicationInfo appInfo;

        /// <summary>
        /// The name of the runtime the application runs on
        /// </summary>
        private string runtime;

        /// <summary>
        /// Array of specific application variables
        /// </summary>
        private ApplicationVariable[] variables;

        /// <summary>
        /// Array of structures containing info about application services
        /// </summary>
        private ApplicationService[] services;

        /// <summary>
        /// The path of the file to which various web events will be redirected
        /// </summary>
        private string logFilePath;

        /// <summary>
        /// The path for the stderr log file
        /// </summary>
        private string errorLogFilePath;

        /// <summary>
        /// Application startup events log file path
        /// </summary>
        private string startupLogFilePath;

        /// <summary>
        /// A list of connection string templates for services
        /// </summary>
        private Dictionary<string, string> autoWireTemplates;

        /// <summary>
        /// A list of URLs that the app is mapped to
        /// </summary>
        private string[] appUrls;

        /// <summary>
        /// Initializes a new instance of the <see cref="ApplicationParsedData"/> class.
        /// </summary>
        /// <param name="appInfo">The app info.</param>
        /// <param name="runtime">The runtime.</param>
        /// <param name="variables">The variables.</param>
        /// <param name="services">The services.</param>
        /// <param name="urls">The URLs.</param>
        /// <param name="logFilePath">The log file path.</param>
        /// <param name="errorLogFilePath">The error log file path.</param>
        /// <param name="startupLogFilePath">The startup log file path.</param>
        /// <param name="autoWireTemplates">A list of connection string templates for services.</param>
        public ApplicationParsedData(
            ApplicationInfo appInfo, 
            string runtime, 
            ApplicationVariable[] variables, 
            ApplicationService[] services,
            string[] urls,
            string logFilePath, 
            string errorLogFilePath, 
            string startupLogFilePath, 
            Dictionary<string, string> autoWireTemplates)
        {
            this.appInfo = appInfo;
            this.runtime = runtime;
            this.variables = variables;
            this.services = services;
            this.logFilePath = logFilePath;
            this.errorLogFilePath = errorLogFilePath;
            this.startupLogFilePath = startupLogFilePath;
            this.autoWireTemplates = autoWireTemplates;
            this.appUrls = urls;
        }

        /// <summary>
        /// Gets the general application settings.
        /// </summary>
        public ApplicationInfo AppInfo
        {
            get
            {
                return this.appInfo;
            }
        }

        /// <summary>
        /// Gets the runtime for the application.
        /// </summary>
        public string Runtime
        {
            get
            {
                return this.runtime;
            }
        }

        /// <summary>
        /// Gets the warn/info/debug/trace log file path.
        /// </summary>
        public string LogFilePath
        {
            get
            {
                return this.logFilePath;
            }
        }

        /// <summary>
        /// Gets the fatal/error log file path.
        /// </summary>
        public string ErrorLogFilePath
        {
            get
            {
                return this.errorLogFilePath;
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
                return this.startupLogFilePath;
            }
        }

        /// <summary>
        /// Gets the auto wire templates.
        /// </summary>
        public Dictionary<string, string> AutoWireTemplates
        {
            get 
            {
                return this.autoWireTemplates; 
            }
        }

        /// <summary>
        /// Gets the application variables.
        /// </summary>
        /// <returns>An array of ApplicationVariable objects.</returns>
        public ApplicationVariable[] GetVariables()
        {
            return this.variables;
        }

        /// <summary>
        /// Gets the provisioned services for an app.
        /// </summary>
        /// <returns>An array of ApplicationService objects.</returns>
        public ApplicationService[] GetServices()
        {
            return this.services;
        }

        /// <summary>
        /// Gets the mapped URLs for an app.
        /// </summary>
        /// <returns>An array of strings.</returns>
        public string[] GetUrls()
        {
            return this.appUrls;
        }
    }
}
