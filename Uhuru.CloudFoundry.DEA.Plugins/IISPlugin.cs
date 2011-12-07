// -----------------------------------------------------------------------
// <copyright file="IISPlugin.cs" company="Uhuru Software">
// Copyright (c) 2011 Uhuru Software, Inc., All Rights Reserved
// </copyright>
// -----------------------------------------------------------------------

namespace Uhuru.CloudFoundry.DEA.Plugins
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.DirectoryServices;
    using System.Globalization;
    using System.IO;
    using System.Security.AccessControl;
    using System.Text;
    using System.Threading;
    using Microsoft.Web.Administration;
    using Uhuru.CloudFoundry.Server.DEA.PluginBase;
    using Uhuru.Utilities;
    using System.Configuration;


    /// <summary>
    /// Class implementing the IAgentPlugin interface
    /// Responsible for automatically deploying and managing an IIS .Net application
    /// </summary>
    public class IISPlugin : MarshalByRefObject, IAgentPlugin
    {
        #region Class Members

        private string appName = String.Empty;
        private string appPath = String.Empty;
        private static Mutex mut = new Mutex(false, "Global\\UhuruIIS");
        
        //private ServerManager serverMgr = new ServerManager();
        private ApplicationInfo applicationInfo = null;

        #endregion

        #region Public Interface Methods

        /// <summary>
        /// Sets the initial data for an IIS based application and auto-wires the web.config file with the required configuration settings
        /// 
        /// </summary>
        /// <param name="appInfo">Basic information about the app ( deployment dir, app name, port , etc )</param>
        /// <param name="runtime">app's runtime</param>
        /// <param name="variables">some other variables, if necessary</param>
        /// <param name="services">the services the app may want to use</param>
        /// <param name="logFilePath">the file path where the logs will be saved</param>
        public void ConfigureApplication(ApplicationInfo appInfo, Runtime runtime, ApplicationVariable[] variables, ApplicationService[] services, string logFilePath)
        {
            appName = removeSpecialCharacters(appInfo.Name) + appInfo.Port.ToString(CultureInfo.InvariantCulture);
            appPath = appInfo.Path;

            applicationInfo = appInfo;

            autowireApp(appInfo, services, logFilePath);
        }

        public void ConfigureApplication(ApplicationInfo appInfo, Runtime runtime, ApplicationVariable[] variables, ApplicationService[] services, string logFilePath, int[] processIds)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// a delegate used to devise a way to cope with a (potential) application crash
        /// </summary>
        public event ApplicationCrashDelegate OnApplicationCrash;

        /// <summary>
        /// sets the data necessary for debugging the application remotely
        /// </summary>
        /// <param name="debugPort">the port used to reach the app remotely</param>
        /// <param name="debugIp">the ip where the app cand be reached for debug</param>
        /// <param name="debugVariables">the variables necessary for debug, if any</param>
        public void ConfigureDebug(string debugPort, string debugIp, ApplicationVariable[] debugVariables)
        {
            throw new NotImplementedException();
        }


        /// <summary>
        /// Starts the application
        /// </summary>
        public void StartApplication()
        {
            DotNetVersion version = getAppVersion(applicationInfo);

            deployApp(applicationInfo, version);

            startApp(); 
        }

        /// <summary>
        /// Returns the process ID of the worker process associated with the running application
        /// </summary>
        /// <returns>
        /// the ids of the processes, as an array
        /// </returns>
        public int GetApplicationProcessID()
        {
            try
            {
                mut.WaitOne();
                using (ServerManager serverMgr = new ServerManager())
                {
                    if (serverMgr.Sites[appName] == null)
                    {
                        return 0;
                    }
                    string appPoolName = serverMgr.Sites[appName].Applications["/"].ApplicationPoolName;
                    
                    foreach (WorkerProcess process in serverMgr.WorkerProcesses)
                    {
                        if (process.AppPoolName == appPoolName)
                        {
                            return process.ProcessId;
                        }
                    }
                }
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
            stopApp();

            cleanup(appPath);
        }

        /// <summary>
        /// Cleans up the application.
        /// </summary>
        /// <param name="path">The path.</param>
        public void CleanupApplication(string path)
        {
            cleanup(path);
        }


        /// <summary>
        /// Kills all application processes
        /// </summary>
        public void KillApplication()
        {
            try
            {
                mut.WaitOne();
                using (ServerManager serverMgr = new ServerManager())
                {
                    killApplicationProcesses(serverMgr.Sites[appName].Applications["/"].ApplicationPoolName);
                }
            }
            finally
            {
                mut.ReleaseMutex();
            }
        }

        #endregion

        #region Private Helper Methods


        /// <summary>
        /// Creates a per application user, sets security access rules for the application deployment directory
        /// and adds a new site to IIS without starting it
        /// </summary>
        /// <param name="appInfo">Structure that contains parameters required for deploying the application.</param>
        /// <param name="version">The dot net framework version supported by the application.</param>
        private void deployApp(ApplicationInfo appInfo, DotNetVersion version)
        {
            string aspNetVersion = getAspDotNetVersion(version);
            string password = appInfo.WindowsPassword;
            string userName = appInfo.WindowsUsername;

            try
            {
                mut.WaitOne();
                using (ServerManager serverMgr = new ServerManager())
                {
                    DirectoryInfo deploymentDir = new DirectoryInfo(appInfo.Path);
                    DirectorySecurity deploymentDirSecurity = deploymentDir.GetAccessControl();

                    deploymentDirSecurity.SetAccessRule(new FileSystemAccessRule(userName, FileSystemRights.Write | FileSystemRights.Read | FileSystemRights.Delete | FileSystemRights.Modify, InheritanceFlags.ContainerInherit | InheritanceFlags.ObjectInherit, PropagationFlags.None, AccessControlType.Allow));

                    deploymentDir.SetAccessControl(deploymentDirSecurity);


                    Site mySite = serverMgr.Sites.Add(appName, appInfo.Path, appInfo.Port);
                    mySite.ServerAutoStart = false;
                    
                    ApplicationPool applicationPool = serverMgr.ApplicationPools[appName];
                    if (applicationPool == null)
                    {
                        serverMgr.ApplicationPools.Add(appName);
                        applicationPool = serverMgr.ApplicationPools[appName];
                        applicationPool.ManagedRuntimeVersion = aspNetVersion;
                        applicationPool.ProcessModel.IdentityType = ProcessModelIdentityType.SpecificUser;
                        applicationPool.ProcessModel.UserName = userName;
                        applicationPool.ProcessModel.Password = password;
                        applicationPool.Enable32BitAppOnWin64 = true;
                    }

                    mySite.Applications["/"].ApplicationPoolName = appName;
                    FirewallTools.OpenPort(appInfo.Port, appInfo.Name);
                    serverMgr.CommitChanges();
                }
            }
            finally
            {
                mut.ReleaseMutex();
            }
        }



        /// <summary>
        /// Autowires the service connections and ASP.NET health monitoring in the application's web.config
        /// </summary>
        /// <param name="appInfo">The application info structure.</param>
        /// <param name="services">The services.</param>
        /// <param name="logFilePath">The ASP.NET events log file path.</param>
        private void autowireApp(ApplicationInfo appInfo, ApplicationService[] services, string logFilePath)
        {
            string configFile = Path.Combine(appInfo.Path, "web.config");

            if (File.Exists(configFile))
            {
                if (services != null)
                {
                    Dictionary<string, string> connections = new Dictionary<string, string>();

                    foreach (ApplicationService service in services)
                    {
                        string key = service.ServiceName;
                        string connectionString = String.Format(CultureInfo.InvariantCulture,
                            "Data Source={0},{1};Initial Catalog={2},User Id={3},Password={4};",
                            service.Host,
                            service.Port,
                            service.Name,
                            service.User,
                            service.Password);

                        connections.Add(key, connectionString);
                    }

                    string configFileContents = File.ReadAllText(configFile);

                    foreach (string con in connections.Keys)
                    {
                        string conToReplace = String.Format(CultureInfo.InvariantCulture, "{{mssql#{0}}}", con);
                        configFileContents = configFileContents.Replace(conToReplace, connections[con]);
                    }

                  //  System.Configuration.Configuration dllConfig = ConfigurationManager.OpenExeConfiguration(
                    

                    File.WriteAllText(configFile, configFileContents);
                }
            }
        }


        /// <summary>
        /// Starts the application and blocks until the application is in the started state.
        /// </summary>
        private void startApp()
        {
            try
            {
                mut.WaitOne();
                using (ServerManager serverMgr = new ServerManager())
                {
                    Site site = serverMgr.Sites[appName];

                    waitApp(ObjectState.Stopped, 5000);

                    if (site.State == ObjectState.Started)
                    {
                        return;
                    }
                    else
                    {
                        if (site.State == ObjectState.Stopping)
                        {
                            waitApp(ObjectState.Stopped, 5000);
                        }
                        if (site.State != ObjectState.Starting)
                        {
                            site.Start();
                        }
                    }
                    //ToDo: add configuration for timeout
                    waitApp(ObjectState.Started, 20000);
                }
            }
            finally
            {
                mut.ReleaseMutex();
            }
        }

        /// <summary>
        /// Stops the application and blocks until the application is in the stopped state.
        /// </summary>
        private void stopApp()
        {
            try
            {
                mut.WaitOne();
                using (ServerManager serverMgr = new ServerManager())
                {
                    ObjectState state = serverMgr.Sites[appName].State;

                    if (state == ObjectState.Stopped)
                    {
                        return;
                    }
                    else if (state == ObjectState.Starting || state == ObjectState.Started)
                    {
                        waitApp(ObjectState.Started, 5000);
                        serverMgr.Sites[appName].Stop();
                    }
                    waitApp(ObjectState.Stopped, 5000);
                }
            }
            finally
            {
                mut.ReleaseMutex();
            }
        }


        /// <summary>
        /// Cleans up everything associated with the application deployed at the specified path.
        /// </summary>
        /// <param name="path">The application path.</param>
        private void cleanup(string path)
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
                            delete(site.Bindings[0].EndPoint.Port);
                        }
                        if (fullPath.ToUpperInvariant() == root.FullName.ToUpperInvariant())
                        {
                            delete(site.Bindings[0].EndPoint.Port);
                        }
                        foreach (DirectoryInfo di in childDirectories)
                        {
                            if (di.FullName.ToUpperInvariant() == fullPath.ToUpperInvariant())
                            {
                                delete(site.Bindings[0].EndPoint.Port);
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
        public void delete(int port)
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

                    bool retry = true;
                    while (retry)
                    {
                        try
                        {
                            serverMgr.Sites[currentSite.Name].Stop();
                            retry = false;
                        }
                        catch (System.Runtime.InteropServices.COMException)
                        {
                            // todo log exception
                        }
                    }
                    int time = 0;
                    while (serverMgr.Sites[currentSite.Name].State != ObjectState.Stopped && time < 300)
                    {
                        Thread.Sleep(100);
                        time++;
                    }

                    if (time == 300)
                    {
                        killApplicationProcesses(currentSite.Applications["/"].ApplicationPoolName);
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
                        killApplicationProcesses(applicationPool.Name);
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
        /// Blocks until the application is in the specified state or until the timeout expires
        /// Note: If the timeout expires without the state condition being true, the method throws a TimeoutException
        /// </summary>
        /// <param name="waitForState">State to wait on.</param>
        /// <param name="milliseconds">Timeout in milliseconds.</param>
        private void waitApp(ObjectState waitForState, int milliseconds)
        {
            using (ServerManager serverMgr = new ServerManager())
            {
                Site site = serverMgr.Sites[appName];
                
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
                        //TODO log the exception as warning
                    }
                    Thread.Sleep(25);
                    timeout += 25;
                }

                if (site.State != waitForState)
                {
                    throw new TimeoutException("App start operation exceeded maximum time");
                }
            }
        }


        /// <summary>
        /// Forcefully kills the application processes.
        /// </summary>
        /// <param name="appPoolName">Name of the app pool associated with the application.</param>
        private void killApplicationProcesses(string appPoolName)
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
        /// <returns></returns>
        private String getAspDotNetVersion(DotNetVersion version)
        {
            string dotNetVersion = null;
            switch (version)
            {
                case (DotNetVersion.Two):
                    {
                        dotNetVersion = "v2.0";
                        break;
                    }
                case (DotNetVersion.Four):
                    {
                        dotNetVersion = "v4.0";
                        break;
                    }
            }

            return dotNetVersion;
        }


        /// <summary>
        /// Gets the dot net version that the application runs on.
        /// </summary>
        /// <param name="appInfo">The application info structure.</param>
        /// <returns></returns>
        private DotNetVersion getAppVersion(ApplicationInfo appInfo)
        {
            string[] allAssemblies = Directory.GetFiles(appInfo.Path, "*.dll", SearchOption.AllDirectories);

            DotNetVersion version = DotNetVersion.Two;

            foreach (string assembly in allAssemblies)
            {
                if (NetFrameworkVersion.GetVersion(assembly) == DotNetVersion.Four)
                {
                    version = DotNetVersion.Four;
                    break;
                }
            }

            return version;
        }

        /// <summary>
        /// Removes special characters from an input string.
        /// Note: special characters are considered the ones illegal in a Windows account name
        /// </summary>
        /// <param name="input">The input string.</param>
        /// <returns></returns>
        private string removeSpecialCharacters(string input)
        {
            if (String.IsNullOrEmpty(input))
            {
                throw new ArgumentException("Argument null or empty", "input");
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
                    (input[i] != '@')
                    )
                {
                    sb.Append(input[i]);
                }
            }
            return sb.ToString();
        }

        #endregion
    }
}
