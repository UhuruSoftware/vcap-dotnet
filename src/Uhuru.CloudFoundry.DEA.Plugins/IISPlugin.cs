﻿// -----------------------------------------------------------------------
// <copyright file="IISPlugin.cs" company="Uhuru Software, Inc.">
// Copyright (c) 2011 Uhuru Software, Inc., All Rights Reserved
// </copyright>
// -----------------------------------------------------------------------

namespace Uhuru.CloudFoundry.DEA.Plugins
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Runtime.InteropServices;
    using System.Security.AccessControl;
    using System.Text;
    using System.Threading;
    using System.Xml;
    using System.Xml.XPath;
    using Microsoft.Web.Administration;
    using Uhuru.CloudFoundry.DEA.AutoWiring;
    using Uhuru.CloudFoundry.DEA.PluginBase;
    using Uhuru.CloudFoundry.DEA.Plugins.AspDotNetLogging;
    using Uhuru.Utilities;

    /// <summary>
    /// Class implementing the IAgentPlugin interface
    /// Responsible for automatically deploying and managing an IIS .Net application
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Plugin", Justification = "The word is in the dictionary, but the warning is still generated.")]
    public class IISPlugin : MarshalByRefObject, IAgentPlugin
    {
        /// <summary>
        /// Mutex for protecting access to the ServerManager
        /// </summary>
        private static Mutex mut = new Mutex(false, "Global\\UhuruIIS");

        /// <summary>
        /// The application name
        /// </summary>
        private string appName = string.Empty;

        /// <summary>
        /// The application path
        /// </summary>
        private string appPath = string.Empty;

        /// <summary>
        /// The file logger instance
        /// </summary>
        private FileLogger startupLogger;

        /// <summary>
        /// A list of connection string templates for services
        /// </summary>
        private Dictionary<string, string> autoWireTemplates;

        /// <summary>
        /// The ApplicationInfo structure containing various info about the app ( name, path, port, etc )
        /// </summary>
        private ApplicationInfo applicationInfo = null;

        /// <summary>
        /// Asp .net version of the app
        /// </summary>
        private DotNetVersion aspDotNetVersion = DotNetVersion.Four;

        /// <summary>
        /// Cpu platform of the app
        /// </summary>
        private CpuTarget cpuTarget;

        /// <summary>
        /// Parsed data.
        /// </summary>
        private ApplicationParsedData parsedData;

        /// <summary>
        /// sets the initial data for an application
        /// </summary>
        /// <param name="variables">All variables needed to run the application.</param>
        public void ConfigureApplication(ApplicationVariable[] variables)
        {
            try
            {
                this.parsedData = PluginHelper.GetParsedData(variables);
                this.startupLogger = new FileLogger(this.parsedData.StartupLogFilePath);

                this.appName = RemoveSpecialCharacters(this.parsedData.AppInfo.Name) + this.parsedData.AppInfo.Port.ToString(CultureInfo.InvariantCulture);
                this.appPath = this.parsedData.AppInfo.Path;

                this.applicationInfo = this.parsedData.AppInfo;

                this.autoWireTemplates = this.parsedData.AutoWireTemplates;

                this.aspDotNetVersion = this.GetAppVersion(this.applicationInfo);

                this.cpuTarget = this.GetCpuTarget(this.applicationInfo);

                this.AutowireApp(this.parsedData.AppInfo, variables, this.parsedData.GetServices(), this.parsedData.LogFilePath, this.parsedData.ErrorLogFilePath);
                this.AutowireUhurufs(this.parsedData.AppInfo, variables, this.parsedData.GetServices(), this.parsedData.HomeAppPath);
            }
            catch (Exception ex)
            {
                this.startupLogger.Error(ex.ToString());
                throw;
            }
        }

        /// <summary>
        /// recovers a running application
        /// </summary>
        /// <param name="variables">All variables needed to run the application.</param>
        public void RecoverApplication(ApplicationVariable[] variables)
        {
            try
            {
                this.parsedData = PluginHelper.GetParsedData(variables);
                this.startupLogger = new FileLogger(this.parsedData.StartupLogFilePath);
                this.appName = RemoveSpecialCharacters(this.parsedData.AppInfo.Name) + this.parsedData.AppInfo.Port.ToString(CultureInfo.InvariantCulture);
                this.appPath = this.parsedData.AppInfo.Path;
                this.applicationInfo = this.parsedData.AppInfo;
                this.autoWireTemplates = this.parsedData.AutoWireTemplates;
            }
            catch (Exception ex)
            {
                this.startupLogger.Error(ex.ToString());
                throw;
            }
        }

        /// <summary>
        /// Starts the application
        /// </summary>
        public void StartApplication()
        {
            try
            {
                this.DeployApp(this.applicationInfo, this.aspDotNetVersion);

                this.StartApp();
            }
            catch (Exception ex)
            {
                this.startupLogger.Error(ex.ToString());
                throw;
            }
        }

        /// <summary>
        /// Returns the process ID of the worker process associated with the running application
        /// </summary>
        /// <returns>
        /// the ids of the processes, as an array
        /// </returns>
        public int GetApplicationProcessId()
        {
            try
            {
                mut.WaitOne();
                using (ServerManager serverMgr = new ServerManager())
                {
                    if (serverMgr.Sites[this.appName] == null)
                    {
                        return 0;
                    }

                    string appPoolName = serverMgr.Sites[this.appName].Applications["/"].ApplicationPoolName;

                    foreach (WorkerProcess process in serverMgr.WorkerProcesses)
                    {
                        if (process.AppPoolName == appPoolName)
                        {
                            return process.ProcessId;
                        }
                    }
                }
            }
            catch (COMException)
            {
                return 0;
            }
            finally
            {
                mut.ReleaseMutex();
            }

            return 0;
        }

        /// <summary>
        /// Shuts down the application
        /// </summary>
        public void StopApplication()
        {
            this.StopApp();

            Cleanup(this.appPath);
        }

        /// <summary>
        /// Cleans up the application.
        /// </summary>
        /// <param name="applicationPath">The path.</param>
        public void CleanupApplication(string applicationPath)
        {
            // Remove the uhurufs servers from the system host file.
            var services = this.parsedData.GetServices();
            foreach (ApplicationService serv in services)
            {
                if (serv.ServiceLabel.StartsWith("uhurufs", StringComparison.Ordinal))
                {
                    string shareHost = GenerateUhurufsHost(this.parsedData.AppInfo.InstanceId, serv.InstanceName, serv.User);
                    string remotePath = string.Format(CultureInfo.InvariantCulture, @"\\{0}\{1}", shareHost, serv.InstanceName);
                    SambaWindowsClient.Unmount(remotePath);
                    
                    SystemHosts.TryRemove(shareHost);
                }
            }

            Cleanup(applicationPath);
        }

        /// <summary>
        /// Implementation for MarshallByRefObject
        /// </summary>
        /// <returns>Allways return null, so the plugin is not collected.</returns>
        public override object InitializeLifetimeService()
        {
            return null;
        }

        /// <summary>
        /// Copies a directory recursively, without overwriting.
        /// </summary>
        /// <param name="source">Source folder to copy.</param>
        /// <param name="destination">Destination folder.</param>
        private static void CopyFolderRecursively(string source, string destination)
        {
            if (!Directory.Exists(destination))
            {
                Directory.CreateDirectory(destination);
            }

            string[] files = Directory.GetFiles(source);

            foreach (string file in files)
            {
                string name = Path.GetFileName(file);
                string dest = Path.Combine(destination, name);

                try
                {
                    File.Copy(file, dest, false);
                }
                catch (IOException)
                {
                }
            }

            string[] folders = Directory.GetDirectories(source);

            foreach (string folder in folders)
            {
                string name = Path.GetFileName(folder);
                string dest = Path.Combine(destination, name);
                CopyFolderRecursively(folder, dest);
            }
        }

        /// <summary>
        /// Cleans up everything associated with the application deployed at the specified path.
        /// </summary>
        /// <param name="path">The application path.</param>
        private static void Cleanup(string path)
        {
            mut.WaitOne();
            try
            {
                using (ServerManager serverMgr = new ServerManager())
                {
                    DirectoryInfo root = new DirectoryInfo(path);
                    DirectoryInfo[] childDirectories = root.GetDirectories("*", SearchOption.AllDirectories);

                    foreach (Site site in serverMgr.Sites)
                    {
                        string sitePath = site.Applications["/"].VirtualDirectories["/"].PhysicalPath;
                        string fullPath = Environment.ExpandEnvironmentVariables(sitePath);

                        if (!Directory.Exists(fullPath))
                        {
                            Delete(site.Bindings[0].EndPoint.Port);
                        }

                        if (fullPath.ToUpperInvariant() == root.FullName.ToUpperInvariant())
                        {
                            Delete(site.Bindings[0].EndPoint.Port);
                        }

                        foreach (DirectoryInfo di in childDirectories)
                        {
                            if (di.FullName.ToUpperInvariant() == fullPath.ToUpperInvariant())
                            {
                                Delete(site.Bindings[0].EndPoint.Port);
                                break;
                            }
                        }
                    }
                }
            }
            finally
            {
                mut.ReleaseMutex();
            }
        }

        /// <summary>
        /// Removes the application - reachable at the specified port - and its application pools from IIS.
        /// Note: Stops the application pools and the application if necessary
        /// </summary>
        /// <param name="port">The port.</param>
        private static void Delete(int port)
        {
            mut.WaitOne();

            try
            {
                using (ServerManager serverMgr = new ServerManager())
                {
                    Site currentSite = null;
                    foreach (Site site in serverMgr.Sites)
                    {
                        if (site.Bindings != null && site.Bindings[0] != null && site.Bindings[0].EndPoint != null && site.Bindings[0].EndPoint.Port == port)
                        {
                            currentSite = site;
                            break;
                        }
                    }

                    int retryCount = 20;
                    while (retryCount > 0)
                    {
                        try
                        {
                            serverMgr.Sites[currentSite.Name].Stop();
                            break;
                        }
                        catch (System.Runtime.InteropServices.COMException)
                        {
                            // todo log exception
                        }

                        retryCount--;
                    }

                    int time = 0;
                    while (serverMgr.Sites[currentSite.Name].State != ObjectState.Stopped && time < 300)
                    {
                        Thread.Sleep(100);
                        time++;
                    }

                    if (time == 300)
                    {
                        KillApplicationProcesses(currentSite.Applications["/"].ApplicationPoolName);
                    }

                    serverMgr.Sites.Remove(currentSite);
                    serverMgr.CommitChanges();
                    FirewallTools.ClosePort(port);
                    ApplicationPool applicationPool = serverMgr.ApplicationPools[currentSite.Applications["/"].ApplicationPoolName];
                    serverMgr.ApplicationPools[applicationPool.Name].Stop();
                    time = 0;
                    while (serverMgr.ApplicationPools[applicationPool.Name].State != ObjectState.Stopped && time < 300)
                    {
                        Thread.Sleep(100);
                        time++;
                    }

                    if (serverMgr.ApplicationPools[applicationPool.Name].State != ObjectState.Stopped && time == 300)
                    {
                        KillApplicationProcesses(applicationPool.Name);
                    }

                    serverMgr.ApplicationPools.Remove(applicationPool);
                    serverMgr.CommitChanges();
                    string username = null;
                    username = applicationPool.ProcessModel.UserName;
                    if (username != null)
                    {
                        string path = currentSite.Applications["/"].VirtualDirectories["/"].PhysicalPath;
                        if (Directory.Exists(path))
                        {
                            DirectoryInfo deploymentDir = new DirectoryInfo(path);
                            DirectorySecurity deploymentDirSecurity = deploymentDir.GetAccessControl();
                            deploymentDirSecurity.RemoveAccessRuleAll(new FileSystemAccessRule(username, FileSystemRights.Write | FileSystemRights.Read | FileSystemRights.Delete | FileSystemRights.Modify, AccessControlType.Allow));
                            deploymentDir.SetAccessControl(deploymentDirSecurity);
                        }
                    }
                }
            }
            finally
            {
                mut.ReleaseMutex();
            }
        }

        /// <summary>
        /// Forcefully kills the application processes.
        /// </summary>
        /// <param name="appPoolName">Name of the app pool associated with the application.</param>
        private static void KillApplicationProcesses(string appPoolName)
        {
            using (ServerManager serverMgr = new ServerManager())
            {
                foreach (WorkerProcess process in serverMgr.WorkerProcesses)
                {
                    if (process.AppPoolName == appPoolName)
                    {
                        Process p = Process.GetProcessById(process.ProcessId);
                        if (p != null)
                        {
                            p.Kill();
                            p.WaitForExit();
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Gets the ASP dot net version in string format from the dot net framework version
        /// </summary>
        /// <param name="version">The dot net framework version.</param>
        /// <returns>Asp.NET version in string format. Returns null if version is not supported</returns>
        private static string GetAspDotNetVersion(DotNetVersion version)
        {
            string dotNetVersion = null;
            switch (version)
            {
                case DotNetVersion.Two:
                    {
                        dotNetVersion = "v2.0";
                        break;
                    }

                case DotNetVersion.Four:
                    {
                        dotNetVersion = "v4.0";
                        break;
                    }
            }

            return dotNetVersion;
        }

        /// <summary>
        /// Removes special characters from an input string.
        /// Note: special characters are considered the ones illegal in a Windows account name
        /// </summary>
        /// <param name="input">The input string.</param>
        /// <returns>A copy of the input string, with special characters removed</returns>
        private static string RemoveSpecialCharacters(string input)
        {
            if (string.IsNullOrEmpty(input))
            {
                throw new ArgumentException(Strings.ArgumentNullOrEmpty, "input");
            }

            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < input.Length; i++)
            {
                if (
                    (input[i] != '/') &&
                    (input[i] != '\\') &&
                    (input[i] != '[') &&
                    (input[i] != ']') &&
                    (input[i] != ':') &&
                    (input[i] != ';') &&
                    (input[i] != '|') &&
                    (input[i] != '=') &&
                    (input[i] != ',') &&
                    (input[i] != '+') &&
                    (input[i] != '*') &&
                    (input[i] != '?') &&
                    (input[i] != '>') &&
                    (input[i] != '<') &&
                    (input[i] != '@'))
                {
                    sb.Append(input[i]);
                }
            }

            return sb.ToString();
        }

        /// <summary>
        /// Generates a host for the uhurufs service to be autowired.
        /// </summary>
        /// <param name="appInstanceId">Application Instance ID.</param>
        /// <param name="serviceInstanceName">Service Instance Name.</param>
        /// <param name="serviceUsername">Service Username credential.</param>
        /// <returns>The generate host.</returns>
        private static string GenerateUhurufsHost(string appInstanceId, string serviceInstanceName, string serviceUsername)
        {
            // N.B. Max length for host is 64 character
            appInstanceId = appInstanceId.Substring(0, Math.Min(appInstanceId.Length, 15));
            serviceInstanceName = serviceInstanceName.Substring(0, Math.Min(serviceInstanceName.Length, 20));
            serviceUsername = serviceUsername.Substring(0, Math.Min(serviceUsername.Length, 15));

            return "DEA-" + appInstanceId + "-" + serviceInstanceName + "-" + serviceUsername;
        }

        /// <summary>
        /// Generates the path where the uhurufs instance to be mounted to.
        /// </summary>
        /// <param name="homeAppPath">Path to the app directory.</param>
        /// <param name="serviceName">Service name.</param>
        /// <returns>Uhurufs service mount path.</returns>
        private static string GenerateMountPath(string homeAppPath, string serviceName)
        {
            return Path.Combine(homeAppPath, "uhurufs", serviceName);
        }

        /// <summary>
        /// Blocks until the application is in the specified state or until the timeout expires
        /// Note: If the timeout expires without the state condition being true, the method throws a TimeoutException
        /// </summary>
        /// <param name="waitForState">State to wait on.</param>
        /// <param name="milliseconds">Timeout in milliseconds.</param>
        private void WaitApp(ObjectState waitForState, int milliseconds)
        {
            using (ServerManager serverMgr = new ServerManager())
            {
                Site site = serverMgr.Sites[this.appName];

                int timeout = 0;
                while (timeout < milliseconds)
                {
                    try
                    {
                        if (site.State == waitForState)
                        {
                            return;
                        }
                    }
                    catch (System.Runtime.InteropServices.COMException)
                    {
                        // TODO log the exception as warning
                    }

                    Thread.Sleep(25);
                    timeout += 25;
                }

                if (site.State != waitForState)
                {
                    throw new TimeoutException(Strings.AppStartOperationExceeded);
                }
            }
        }

        /// <summary>
        /// Gets the dot net version that the application runs on.
        /// </summary>
        /// <param name="appInfo">The application info structure.</param>
        /// <returns>The .net version supported by the application</returns>
        private DotNetVersion GetAppVersion(ApplicationInfo appInfo)
        {
            this.startupLogger.Info(Strings.DeterminingApplication);

            DotNetVersion version = NetFrameworkVersion.GetFrameworkFromConfig(Path.Combine(appInfo.Path, "web.config"));

            if (version == DotNetVersion.Two)
            {
                string[] allAssemblies = Directory.GetFiles(appInfo.Path, "*.dll", SearchOption.AllDirectories);

                foreach (string assembly in allAssemblies)
                {
                    if (NetFrameworkVersion.GetVersion(assembly) == DotNetVersion.Four)
                    {
                        version = DotNetVersion.Four;
                        break;
                    }
                }
            }

            this.startupLogger.Info(Strings.DetectedNet + GetAspDotNetVersion(version));

            return version;
        }

        /// <summary>
        /// Gets the cpu target for the application.
        /// </summary>
        /// <param name="appInfo">The application info structure.</param>
        /// <returns>CPU target</returns>
        private CpuTarget GetCpuTarget(ApplicationInfo appInfo)
        {
            this.startupLogger.Info(Strings.DetectingCpuTarget);

            string[] allAssemblies = Directory.GetFiles(appInfo.Path, "*.dll", SearchOption.AllDirectories);

            CpuTarget target = CpuTarget.X64;

            foreach (string assembly in allAssemblies)
            {
                target = PlatformTarget.DetectPlatform(assembly);
                if (target == CpuTarget.X86)
                {
                    break;
                }
            }

            this.startupLogger.Info(Strings.DetectedCpuTarget, target.ToString());

            return target;
        }

        /// <summary>
        /// Creates a per application user, sets security access rules for the application deployment directory
        /// and adds a new site to IIS without starting it
        /// </summary>
        /// <param name="appInfo">Structure that contains parameters required for deploying the application.</param>
        /// <param name="version">The dot net framework version supported by the application.</param>
        private void DeployApp(ApplicationInfo appInfo, DotNetVersion version)
        {
            this.startupLogger.Info(Strings.DeployingAppOnIis);

            string aspNetVersion = GetAspDotNetVersion(version);
            string password = appInfo.WindowsPassword;
            string userName = appInfo.WindowsUserName;

            try
            {
                mut.WaitOne();
                using (ServerManager serverMgr = new ServerManager())
                {
                    DirectoryInfo deploymentDir = new DirectoryInfo(appInfo.Path);

                    DirectorySecurity deploymentDirSecurity = deploymentDir.GetAccessControl();

                    deploymentDirSecurity.SetAccessRule(
                        new FileSystemAccessRule(
                            userName,
                            FileSystemRights.Write | FileSystemRights.Read | FileSystemRights.Delete | FileSystemRights.Modify,
                            InheritanceFlags.ContainerInherit | InheritanceFlags.ObjectInherit,
                            PropagationFlags.None,
                            AccessControlType.Allow));

                    deploymentDir.SetAccessControl(deploymentDirSecurity);

                    Site mySite = serverMgr.Sites.Add(this.appName, appInfo.Path, appInfo.Port);
                    mySite.ServerAutoStart = false;

                    ApplicationPool applicationPool = serverMgr.ApplicationPools[this.appName];
                    if (applicationPool == null)
                    {
                        serverMgr.ApplicationPools.Add(this.appName);
                        applicationPool = serverMgr.ApplicationPools[this.appName];
                        applicationPool.ManagedRuntimeVersion = aspNetVersion;
                        applicationPool.ProcessModel.IdentityType = ProcessModelIdentityType.SpecificUser;
                        applicationPool.ProcessModel.UserName = userName;
                        applicationPool.ProcessModel.Password = password;
                        applicationPool.ProcessModel.LoadUserProfile = true;
                        if (this.cpuTarget == CpuTarget.X86)
                        {
                            applicationPool.Enable32BitAppOnWin64 = true;
                        }
                        else
                        {
                            applicationPool.Enable32BitAppOnWin64 = false;
                        }
                    }

                    mySite.Applications["/"].ApplicationPoolName = this.appName;
                    FirewallTools.OpenPort(appInfo.Port, appInfo.Name);
                    serverMgr.CommitChanges();
                }
            }
            finally
            {
                mut.ReleaseMutex();
                this.startupLogger.Info(Strings.FinishedAppDeploymentOnIis);
            }
        }

        /// <summary>
        /// Autowires the service connections and ASP.NET health monitoring in the application's web.config
        /// </summary>
        /// <param name="appInfo">The application info structure.</param>
        /// <param name="variables">All application variables.</param>
        /// <param name="services">The services.</param>
        /// <param name="logFilePath">The ASP.NET "Heartbeat" and "Lifetime Events" log file path.</param>
        /// <param name="errorLogFilePath">The ASP.NET "All Errors" events log file path.</param>
        private void AutowireApp(ApplicationInfo appInfo, ApplicationVariable[] variables, ApplicationService[] services, string logFilePath, string errorLogFilePath)
        {
            this.startupLogger.Info(Strings.StartingApplicationAutoWiring);

            // get all config files
            string[] allConfigFiles = Directory.GetFiles(appInfo.Path, "*.config", SearchOption.AllDirectories);

            foreach (string configFile in allConfigFiles)
            {
                if (File.Exists(configFile))
                {
                    string configFileContents = File.ReadAllText(configFile);

                    if (services != null)
                    {
                        Dictionary<string, string> connections = new Dictionary<string, string>();
                        Dictionary<string, string> connValues = new Dictionary<string, string>();

                        foreach (ApplicationService service in services)
                        {
                            string key = service.ServiceLabel;
                            string template = string.Empty;

                            if (this.autoWireTemplates.TryGetValue(key, out template))
                            {
                                template = template.Replace(Strings.Host, service.Host);
                                template = template.Replace(Strings.Port, service.Port.ToString(CultureInfo.InvariantCulture));
                                template = template.Replace(Strings.Name, service.InstanceName);
                                template = template.Replace(Strings.User, service.User);
                                template = template.Replace(Strings.Password, service.Password);

                                connections[string.Format(CultureInfo.InvariantCulture, "{{{0}#{1}}}", key, service.Name)] = template;
                            }

                            char[] charsToTrim = { '{', '}' };
                            connValues.Add(string.Format(CultureInfo.InvariantCulture, "{{{0}#{1}}}", service.Name, Strings.User.Trim(charsToTrim)), service.User);
                            connValues.Add(string.Format(CultureInfo.InvariantCulture, "{{{0}#{1}}}", service.Name, Strings.Host.Trim(charsToTrim)), service.Host);
                            connValues.Add(string.Format(CultureInfo.InvariantCulture, "{{{0}#{1}}}", service.Name, Strings.Port.Trim(charsToTrim)), service.Port.ToString(CultureInfo.InvariantCulture));
                            connValues.Add(string.Format(CultureInfo.InvariantCulture, "{{{0}#{1}}}", service.Name, Strings.Password.Trim(charsToTrim)), service.Password);
                            connValues.Add(string.Format(CultureInfo.InvariantCulture, "{{{0}#{1}}}", service.Name, Strings.Name.Trim(charsToTrim)), service.InstanceName);
                        }

                        foreach (string con in connections.Keys)
                        {
                            this.startupLogger.Info(Strings.ConfiguringService + con);
                            configFileContents = configFileContents.Replace(con, connections[con]);
                        }

                        foreach (string key in connValues.Keys)
                        {
                            this.startupLogger.Info(string.Format(CultureInfo.InvariantCulture, Strings.ConfiguringServiceValue, key));
                            configFileContents = configFileContents.Replace(key, connValues[key]);
                        }
                    }

                    File.WriteAllText(configFile, configFileContents);
                }
            }

            string webConfigFile = Path.Combine(appInfo.Path, "web.config");
            if (File.Exists(webConfigFile))
            {
                this.SetApplicationVariables(webConfigFile, variables, logFilePath, errorLogFilePath);

                this.startupLogger.Info(Strings.SavedConfigurationFile);

                this.startupLogger.Info(Strings.SettingUpLogging);

                string appDir = Path.GetDirectoryName(webConfigFile);
                string binDir = Path.Combine(appDir, "bin");
                string assemblyFile = typeof(LogFileWebEventProvider).Assembly.Location;
                string destinationAssemblyFile = Path.Combine(binDir, Path.GetFileName(assemblyFile));

                Directory.CreateDirectory(binDir);

                File.Copy(assemblyFile, destinationAssemblyFile, true);

                this.startupLogger.Info(Strings.CopiedLoggingBinariesToBin);

                SiteConfig siteConfiguration = new SiteConfig(appDir, true);
                HealthMonRewire healthMon = new HealthMonRewire();
                healthMon.Register(siteConfiguration);

                siteConfiguration.Rewire(false);
                siteConfiguration.CommitChanges();

                this.startupLogger.Info(Strings.UpdatedLoggingConfiguration);

                DirectoryInfo errorLogDir = new DirectoryInfo(Path.GetDirectoryName(errorLogFilePath));
                DirectoryInfo logDir = new DirectoryInfo(Path.GetDirectoryName(logFilePath));

                DirectorySecurity errorLogDirSecurity = errorLogDir.GetAccessControl();
                DirectorySecurity logDirSecurity = logDir.GetAccessControl();

                errorLogDirSecurity.SetAccessRule(
                    new FileSystemAccessRule(
                        appInfo.WindowsUserName,
                        FileSystemRights.Write | FileSystemRights.Read | FileSystemRights.Delete | FileSystemRights.Modify | FileSystemRights.CreateFiles,
                        InheritanceFlags.ContainerInherit | InheritanceFlags.ObjectInherit,
                        PropagationFlags.None,
                        AccessControlType.Allow));

                logDirSecurity.SetAccessRule(
                    new FileSystemAccessRule(
                        appInfo.WindowsUserName,
                        FileSystemRights.Write | FileSystemRights.Read | FileSystemRights.Delete | FileSystemRights.Modify | FileSystemRights.CreateFiles,
                        InheritanceFlags.ContainerInherit | InheritanceFlags.ObjectInherit,
                        PropagationFlags.None,
                        AccessControlType.Allow));

                errorLogDir.SetAccessControl(errorLogDirSecurity);
                logDir.SetAccessControl(logDirSecurity);
            }
        }

        /// <summary>
        /// Autowires the service connections and ASP.NET health monitoring in the application's web.config
        /// </summary>
        /// <param name="appInfo">The application info structure.</param>
        /// <param name="variables">All application variables.</param>
        /// <param name="services">The services.</param>
        /// <param name="homeAppPath">The home application path.</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1303:Do not pass literals as localized parameters", Justification = "More clear.", MessageId = "Uhuru.Utilities.FileLogger.Error(System.String,System.Object[])"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "Exception is logged inside startup log")]
        private void AutowireUhurufs(ApplicationInfo appInfo, ApplicationVariable[] variables, ApplicationService[] services, string homeAppPath)
        {
            this.startupLogger.Info(Strings.StartingApplicationAutoWiring);

            Dictionary<string, HashSet<string>> persistentFiles = new Dictionary<string, HashSet<string>>();

            foreach (ApplicationVariable var in variables)
            {
                if (var.Name.StartsWith("uhurufs_", StringComparison.Ordinal))
                {
                    string serviceName = var.Name.Split(new string[] { "uhurufs_" }, 2, StringSplitOptions.RemoveEmptyEntries)[0];
                    if (!persistentFiles.ContainsKey(serviceName))
                    {
                        persistentFiles[serviceName] = new HashSet<string>();
                    }

                    string[] persistedItems = var.Value.Trim(new char[] { '"' }).Split(new char[] { ';', ',', ':' });

                    foreach (string item in persistedItems)
                    {
                        persistentFiles[serviceName].Add(item.Replace('/', '\\'));
                    }
                }
            }

            foreach (ApplicationService serv in services)
            {
                if (serv.ServiceLabel.StartsWith("uhurufs", StringComparison.Ordinal))
                {
                    string shareHost = GenerateUhurufsHost(appInfo.InstanceId, serv.InstanceName, serv.User);
                    
                    try
                    {
                        if (!SystemHosts.Exists(shareHost))
                        {
                            // Note: delete the host after the app is deleted
                            SystemHosts.Add(shareHost, serv.Host);
                        }
                    }
                    catch (ArgumentException)
                    {
                        // If the service host cannot be added to hosts connect
                        shareHost = serv.Host;
                    }

                    string remotePath = string.Format(CultureInfo.InvariantCulture, @"\\{0}\{1}", shareHost, serv.InstanceName);
                    string mountPath = GenerateMountPath(homeAppPath, serv.Name);
                    Directory.CreateDirectory(Path.Combine(mountPath, @".."));

                    // Add the share users credentials to the application user.
                    // This way the application can use the share directly without invoking `net use` with the credentials.
                    using (new UserImpersonator(appInfo.WindowsUserName, ".", appInfo.WindowsPassword, true))
                    {
                        SaveCredentials.AddDomainUserCredential(shareHost, serv.User, serv.Password);
                    }

                    // The impersonated user cannot create links 
                    // Note: unmount the share after the app is deleted
                    SambaWindowsClient.Mount(remotePath, serv.User, serv.Password);
                    SambaWindowsClient.LinkDirectory(remotePath, mountPath);

                    if (persistentFiles.ContainsKey(serv.Name))
                    {
                        foreach (string fileSystemItem in persistentFiles[serv.Name])
                        {
                            try
                            {
                                this.PersistFileSystemItem(appInfo.Path, fileSystemItem, Path.Combine(mountPath, appInfo.Name));
                            }
                            catch (Exception ex)
                            {
                                this.startupLogger.Error("Failed linking file/directory: {0}. Exception: {1}", fileSystemItem, ex.ToString());
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Persists a resource on a mounted share, and then links it.
        /// This method will make sure the folder and file structure remains the same on the local file system, while also persisting data on a share.
        /// </summary>
        /// <param name="instancePath">The directory considered to be the "root" of the resources that have to be persisted.</param>
        /// <param name="persistentItem">The directory or file that has to be persisted.</param>
        /// <param name="mountPath">The mounted directory that points to a share.</param>
        private void PersistFileSystemItem(string instancePath, string persistentItem, string mountPath)
        {
            if (string.IsNullOrEmpty(instancePath))
            {
                throw new ArgumentNullException("instancePath");
            }

            if (string.IsNullOrEmpty(persistentItem))
            {
                throw new ArgumentNullException("instancePath");
            }

            if (string.IsNullOrEmpty(mountPath))
            {
                throw new ArgumentNullException("instancePath");
            }

            string mountItem = Path.Combine(mountPath, persistentItem);
            string instanceItem = Path.Combine(instancePath, persistentItem);

            bool isDirectory, isFile;

            using (new UserImpersonator(this.parsedData.AppInfo.WindowsUserName, ".", this.parsedData.AppInfo.WindowsPassword, true))
            {
                isDirectory = Directory.Exists(mountItem) || Directory.Exists(instanceItem);

                if (isDirectory)
                {
                    Directory.CreateDirectory(mountItem);
                    Directory.CreateDirectory(instanceItem);

                    CopyFolderRecursively(instanceItem, mountItem);

                    try
                    {
                        Directory.Delete(instanceItem, true);
                    }
                    catch (DirectoryNotFoundException)
                    {
                    }
                }
            }

            if (isDirectory)
            {
                // Creating links without an admin user is not allowed by default
                // ExecuteCommand("mklink" + " /d " + instanceItem + " " + mountItem);
                SambaWindowsClient.CreateDirectorySymbolicLink(instanceItem, mountItem);
            }

            using (new UserImpersonator(this.parsedData.AppInfo.WindowsUserName, ".", this.parsedData.AppInfo.WindowsPassword, true))
            {
                isFile = File.Exists(mountItem) || File.Exists(instanceItem);

                if (isFile)
                {
                    Directory.CreateDirectory(new DirectoryInfo(mountItem).Parent.FullName);
                    Directory.CreateDirectory(new DirectoryInfo(instanceItem).Parent.FullName);
                
                    try
                    {
                        File.Copy(instanceItem, mountItem);
                    }
                    catch (IOException)
                    {
                    }

                    try
                    {
                        File.Delete(instanceItem);
                    }
                    catch (DirectoryNotFoundException)
                    {
                    }
                }
            }

            if (isFile)
            {
                // Creating links without an admin user is not allowed by default
                // ExecuteCommand("mklink" + " " + instanceItem + " " + mountItem);
                SambaWindowsClient.CreateFileSymbolicLink(instanceItem, mountItem);
            }
        }

        /// <summary>
        /// Auto-wires the application variables and the log file path in the web.config file.
        /// </summary>
        /// <param name="configPath">The config file path.</param>
        /// <param name="variables">The variables.</param>
        /// <param name="logFilePath">The log file path.</param>
        /// <param name="errorLogFilePath">The error log file path.</param>
        private void SetApplicationVariables(string configPath, ApplicationVariable[] variables, string logFilePath, string errorLogFilePath)
        {
            this.startupLogger.Info(Strings.SettingUpApplicationVariables);

            var configFile = new FileInfo(configPath);
            var vdm = new System.Web.Configuration.VirtualDirectoryMapping(configFile.DirectoryName, true, configFile.Name);
            var wcfm = new System.Web.Configuration.WebConfigurationFileMap();
            wcfm.VirtualDirectories.Add("/", vdm);
            System.Configuration.Configuration webConfig = System.Web.Configuration.WebConfigurationManager.OpenMappedWebConfiguration(wcfm, "/");

            bool hasUhuruLogFile = false;
            bool hasUhuruErrorLogFile = false;

            foreach (ApplicationVariable var in variables)
            {
                if (var.Name == "UHURU_LOG_FILE")
                {
                    hasUhuruLogFile = true;
                }

                if (var.Name == "UHURU_ERROR_LOG_FILE")
                {
                    hasUhuruErrorLogFile = true;
                }

                if (webConfig.AppSettings.Settings[var.Name] == null)
                {
                    webConfig.AppSettings.Settings.Add(var.Name, var.Value);
                }
                else
                {
                    webConfig.AppSettings.Settings[var.Name].Value = var.Value;
                }
            }

            if (!hasUhuruLogFile)
            {
                if (webConfig.AppSettings.Settings["UHURU_LOG_FILE"] == null)
                {
                    webConfig.AppSettings.Settings.Add("UHURU_LOG_FILE", logFilePath);
                }
                else
                {
                    webConfig.AppSettings.Settings["UHURU_LOG_FILE"].Value = logFilePath;
                }
            }

            if (!hasUhuruErrorLogFile)
            {
                if (webConfig.AppSettings.Settings["UHURU_ERROR_LOG_FILE"] == null)
                {
                    webConfig.AppSettings.Settings.Add("UHURU_ERROR_LOG_FILE", errorLogFilePath);
                }
                else
                {
                    webConfig.AppSettings.Settings["UHURU_ERROR_LOG_FILE"].Value = errorLogFilePath;
                }
            }

            this.startupLogger.Info(Strings.DoneSettingUpApplication);

            webConfig.Save();
        }

        /// <summary>
        /// Starts the application and blocks until the application is in the started state.
        /// </summary>
        private void StartApp()
        {
            try
            {
                this.startupLogger.Info(Strings.StartingIisSite);

                mut.WaitOne();
                using (ServerManager serverMgr = new ServerManager())
                {
                    Site site = serverMgr.Sites[this.appName];

                    this.WaitApp(ObjectState.Stopped, 5000);

                    if (site.State == ObjectState.Started)
                    {
                        return;
                    }
                    else
                    {
                        if (site.State == ObjectState.Stopping)
                        {
                            this.WaitApp(ObjectState.Stopped, 5000);
                        }

                        if (site.State != ObjectState.Starting)
                        {
                            site.Start();
                        }
                    }

                    // ToDo: add configuration for timeout
                    this.WaitApp(ObjectState.Started, 20000);
                }
            }
            finally
            {
                mut.ReleaseMutex();
                this.startupLogger.Info(Strings.FinishedStartingIisSite);
            }
        }

        /// <summary>
        /// Stops the application and blocks until the application is in the stopped state.
        /// </summary>
        private void StopApp()
        {
            try
            {
                mut.WaitOne();
                using (ServerManager serverMgr = new ServerManager())
                {
                    ObjectState state = serverMgr.Sites[this.appName].State;

                    if (state == ObjectState.Stopped)
                    {
                        return;
                    }
                    else if (state == ObjectState.Starting || state == ObjectState.Started)
                    {
                        this.WaitApp(ObjectState.Started, 5000);
                        serverMgr.Sites[this.appName].Stop();
                    }

                    this.WaitApp(ObjectState.Stopped, 5000);
                }
            }
            finally
            {
                mut.ReleaseMutex();
            }
        }
    }
}
