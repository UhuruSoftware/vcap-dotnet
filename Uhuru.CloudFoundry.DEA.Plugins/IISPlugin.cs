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

        public event ApplicationCrashDelegate OnApplicationCrash;

        public void ConfigureDebug(string debugPort, string debugIp, ApplicationVariable[] debugVariables)
        {
            throw new NotImplementedException();
        }

        public void StartApplication()
        {
            DotNetVersion version = getAppVersion(applicationInfo);

            deployApp(applicationInfo, version);

            startApp(); 
        }

        public int GetApplicationProcessID()
        {
            try
            {
                mut.WaitOne();
                using (ServerManager serverMgr = new ServerManager())
                {
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

        public void StopApplication()
        {
            stopApp();

            cleanup(appPath);
        }

        public void CleanupApplication(string path)
        {
            cleanup(appPath);
        }

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

        private void deployApp(ApplicationInfo appInfo, DotNetVersion version)
        {
            string aspNetVersion = getAspDotNetVersion(version);
            string password = Guid.NewGuid().ToString();
            string userName = createUser(appInfo.Name, password);

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

                    File.WriteAllText(configFile, configFileContents);
                }
            }
        }

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

            cleanupUser("Uhuru_");
        }

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
                        deleteUser(username);
                    }
                }
            }
            finally
            {
                mut.ReleaseMutex();
            }
        }

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

        private string createUser(string appName, string password)
        {
            string userName = "Uhuru_" + appName.Substring(0, 3) + Guid.NewGuid().ToString().Substring(0, 10);
            DirectoryEntry obDirEntry = new DirectoryEntry("WinNT://" + Environment.MachineName.ToString());
            DirectoryEntries entries = obDirEntry.Children;
            DirectoryEntry obUser = entries.Add(userName, "User");
            obUser.Properties["FullName"].Add("Uhuru " + appName + " user");
            object obRet = obUser.Invoke("SetPassword", password);
            obUser.CommitChanges();
            return userName;
        }

        private void deleteUser(string userName)
        {
            DirectoryEntry localDirectory = new DirectoryEntry("WinNT://" + Environment.MachineName.ToString());
            DirectoryEntries users = localDirectory.Children;
            DirectoryEntry user = users.Find(userName);
            users.Remove(user);
        }

        private void cleanupUser(string prefix)
        {
            DirectoryEntry localDirectory = new DirectoryEntry("WinNT://" + Environment.MachineName.ToString());
            DirectoryEntries users = localDirectory.Children;
            List<string> markedForDeletion = new List<string>();
            foreach (DirectoryEntry user in users)
            {
                if (user.Name.StartsWith(prefix))
                {
                    markedForDeletion.Add(user.Name);
                }
            }

            using (ServerManager mgr = new ServerManager())
            {
                foreach (Site site in mgr.Sites)
                {
                    ApplicationPool currentAppPool = mgr.ApplicationPools[site.Applications["/"].ApplicationPoolName];
                    if (currentAppPool != null)
                    {
                        if (currentAppPool.ProcessModel.IdentityType == ProcessModelIdentityType.SpecificUser)
                        {
                            string username = currentAppPool.ProcessModel.UserName;
                            if (markedForDeletion.Contains(username))
                            {
                                markedForDeletion.Remove(username);
                            }
                        }
                    }
                }
            }

            foreach (string username in markedForDeletion)
            {
                DirectoryEntry user = users.Find(username);
                users.Remove(user);
            }
        }
        
        #endregion
    }
}
