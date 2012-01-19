// -----------------------------------------------------------------------
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
        /// sets the initial data for an application
        /// </summary>
        /// <param name="variables">All variables needed to run the application.</param>
        public void ConfigureApplication(ApplicationVariable[] variables)
        {
            try
            {
                ApplicationParsedData parsedData = PluginHelper.GetParsedData(variables);
                this.startupLogger = new FileLogger(parsedData.StartupLogFilePath);

                this.appName = RemoveSpecialCharacters(parsedData.AppInfo.Name) + parsedData.AppInfo.Port.ToString(CultureInfo.InvariantCulture);
                this.appPath = parsedData.AppInfo.Path;

                this.applicationInfo = parsedData.AppInfo;

                this.autoWireTemplates = parsedData.AutoWireTemplates;

                this.aspDotNetVersion = this.GetAppVersion(this.applicationInfo);

                this.AutowireApp(parsedData.AppInfo, variables, parsedData.GetServices(), parsedData.LogFilePath, parsedData.ErrorLogFilePath);

                this.cpuTarget = this.GetCpuTarget(this.applicationInfo);
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
                ApplicationParsedData parsedData = PluginHelper.GetParsedData(variables);
                this.startupLogger = new FileLogger(parsedData.StartupLogFilePath);
                this.appName = RemoveSpecialCharacters(parsedData.AppInfo.Name) + parsedData.AppInfo.Port.ToString(CultureInfo.InvariantCulture);
                this.appPath = parsedData.AppInfo.Path;
                this.applicationInfo = parsedData.AppInfo;
                this.autoWireTemplates = parsedData.AutoWireTemplates;
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
                        if (site.Bindings[0].EndPoint.Port == port)
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

            string[] allAssemblies = Directory.GetFiles(appInfo.Path, "*.dll", SearchOption.AllDirectories);

            DotNetVersion version = DotNetVersion.Two;

            if (allAssemblies.Length == 0)
            {
                version = NetFrameworkVersion.GetFrameworkFromConfig(Path.Combine(appInfo.Path, "web.config"));
            }

            foreach (string assembly in allAssemblies)
            {
                if (NetFrameworkVersion.GetVersion(assembly) == DotNetVersion.Four)
                {
                    version = DotNetVersion.Four;
                    break;
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

            string configFile = Path.Combine(appInfo.Path, "web.config");

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

                XmlDocument doc = this.SetApplicationVariables(configFileContents, variables, logFilePath, errorLogFilePath);

                doc.Save(configFile);
                this.startupLogger.Info(Strings.SavedConfigurationFile);

                this.startupLogger.Info(Strings.SettingUpLogging);

                string appDir = Path.GetDirectoryName(configFile);
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
        /// Autowires the application variables and the log file path in the web.config file.
        /// </summary>
        /// <param name="configFileContents">The config file contents.</param>
        /// <param name="variables">The variables.</param>
        /// <param name="logFilePath">The log file path.</param>
        /// <param name="errorLogFilePath">The error log file path.</param>
        /// <returns>An xml document ready containing the updated configuration file.</returns>
        private XmlDocument SetApplicationVariables(string configFileContents, ApplicationVariable[] variables, string logFilePath, string errorLogFilePath)
        {
            this.startupLogger.Info(Strings.SettingUpApplicationVariables);

            XmlDocument doc = new XmlDocument();
            doc.LoadXml(configFileContents);

            XmlNode appSettingsNode = doc.SelectSingleNode("configuration/appSettings");

            if (appSettingsNode == null)
            {
                appSettingsNode = doc.CreateNode(XmlNodeType.Element, "appSettings", string.Empty);

                doc.SelectSingleNode("configuration").PrependChild(appSettingsNode);
            }

            bool exists = false;
            bool hasUhuruLogFile = false;
            bool hasUhuruErrorLogFile = false;

            foreach (ApplicationVariable var in variables)
            {
                exists = false;
                if (var.Name == "UHURU_LOG_FILE")
                {
                    hasUhuruLogFile = true;
                }

                if (var.Name == "UHURU_ERROR_LOG_FILE")
                {
                    hasUhuruErrorLogFile = true;
                }

                XmlNode n = doc.CreateNode(XmlNodeType.Element, "add", string.Empty);

                XmlAttribute keyAttr = doc.CreateAttribute("key");
                keyAttr.Value = var.Name;

                XmlAttribute valueAttr = doc.CreateAttribute("value");
                valueAttr.Value = var.Value;

                n.Attributes.Append(keyAttr);
                n.Attributes.Append(valueAttr);

                XPathNodeIterator iter = appSettingsNode.CreateNavigator().Select("add");

                while (iter.MoveNext())
                {
                    string key = iter.Current.GetAttribute("key", string.Empty);
                    if (!string.IsNullOrEmpty(key) && key == var.Name)
                    {
                        exists = true;
                        iter.Current.ReplaceSelf(n.CreateNavigator());
                    }
                }

                if (!exists)
                {
                    appSettingsNode.AppendChild(n);
                }
            }

            if (!hasUhuruLogFile)
            {
                exists = false;
                XmlNode n = doc.CreateNode(XmlNodeType.Element, "add", string.Empty);

                XmlAttribute keyAttr = doc.CreateAttribute("key");
                keyAttr.Value = "UHURU_LOG_FILE";

                XmlAttribute valueAttr = doc.CreateAttribute("value");
                valueAttr.Value = logFilePath;

                n.Attributes.Append(keyAttr);
                n.Attributes.Append(valueAttr);

                XPathNodeIterator iter = appSettingsNode.CreateNavigator().Select("add");

                while (iter.MoveNext())
                {
                    string key = iter.Current.GetAttribute("key", string.Empty);
                    if (!string.IsNullOrEmpty(key) && key == "UHURU_LOG_FILE")
                    {
                        exists = true;
                        iter.Current.ReplaceSelf(n.CreateNavigator());
                    }
                }

                if (!exists)
                {
                    appSettingsNode.AppendChild(n);
                }
            }

            if (!hasUhuruErrorLogFile)
            {
                exists = false;
                XmlNode n = doc.CreateNode(XmlNodeType.Element, "add", string.Empty);

                XmlAttribute keyAttr = doc.CreateAttribute("key");
                keyAttr.Value = "UHURU_ERROR_LOG_FILE";

                XmlAttribute valueAttr = doc.CreateAttribute("value");
                valueAttr.Value = errorLogFilePath;

                n.Attributes.Append(keyAttr);
                n.Attributes.Append(valueAttr);

                XPathNodeIterator iter = appSettingsNode.CreateNavigator().Select("add");

                while (iter.MoveNext())
                {
                    string key = iter.Current.GetAttribute("key", string.Empty);
                    if (!string.IsNullOrEmpty(key) && key == "UHURU_ERROR_LOG_FILE")
                    {
                        exists = true;
                        iter.Current.ReplaceSelf(n.CreateNavigator());
                    }
                }

                if (!exists)
                {
                    appSettingsNode.AppendChild(n);
                }
            }

            this.startupLogger.Info(Strings.DoneSettingUpApplication);

            return doc;
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
