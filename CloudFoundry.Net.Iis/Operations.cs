using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Web.Administration;
using System.IO;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Threading;
using System.Diagnostics;
using NetFwTypeLib;
using System.Collections;
using System.Xml;
using CloudFoundry.Net.IIS;
using System.Globalization;
using System.DirectoryServices;


namespace CloudFoundry.Net.IIS
{
    public static class Operations
    {
        private static Mutex mut = new Mutex(false, "Global\\UhuruIIS");

        public static void Create(string name, int port, string path)
        {
            Create(name, port, path, false);
        }

        public static void Create(string name, int port, string path, bool autoWire)
        {
            string[] allAssemblies = Directory.GetFiles(path, "*.dll", SearchOption.AllDirectories);

            DotNetVersion version = DotNetVersion.Two;

            foreach (string assembly in allAssemblies)
            {
                if (NetFrameworkVersion.GetVersion(assembly) == DotNetVersion.Four)
                {
                    version = DotNetVersion.Four;
                    break;
                }
            }

            Deploy(name, path, port, version);

            if (autoWire)
            {
                Hashtable envVariables = new Hashtable();
                envVariables = (Hashtable)Environment.GetEnvironmentVariables(EnvironmentVariableTarget.Process);

                string configFile = Path.Combine(path, "web.config");

                if (File.Exists(configFile))
                {
                    if (envVariables.ContainsKey("VCAP_SERVICES"))
                    {
                        string jsonVariable = (envVariables["VCAP_SERVICES"] as string).Trim('\'');

                        XmlDocument services = (XmlDocument)JsonTools.JsonToXml(jsonVariable);
                        XmlNodeList serviceList = services.SelectNodes("/root/mssql-2008/item/name");

                        Dictionary<string, string> connections = new Dictionary<string, string>();

                        foreach (XmlNode node in serviceList)
                        {
                            string serviceName = node.InnerText;
                            string selectQuery = String.Format(CultureInfo.InvariantCulture, "/root/mssql-2008/item[name=\"{0}\"]/credentials/", serviceName);
                            string databaseName = services.SelectSingleNode(selectQuery + "name").InnerText;
                            string host = services.SelectSingleNode(selectQuery + "host").InnerText;
                            string serverPort = services.SelectSingleNode(selectQuery + "port").InnerText;
                            string username = services.SelectSingleNode(selectQuery + "username").InnerText;
                            string password = services.SelectSingleNode(selectQuery + "password").InnerText;

                            connections.Add(serviceName,
                                String.Format(CultureInfo.InvariantCulture, "Data Source={0},{1};Initial Catalog={2};User Id={3};Password={4};",
                                host, serverPort, databaseName, username, password));
                        }

                        string configFileContents = File.ReadAllText(configFile);

                        foreach (string con in connections.Keys)
                        {
                            configFileContents = configFileContents.Replace(
                                String.Format(CultureInfo.InvariantCulture, "{{mssql#{0}}}", con),
                                connections[con]);
                        }

                        File.WriteAllText(configFile, configFileContents);

                    }

                    SiteConfig siteConfig = new SiteConfig(path, true);
                    INodeConfigRewireBase hmRewire = new HealthMonRewire();
                    hmRewire.Register(siteConfig);
                    siteConfig.Rewire(false);
                    siteConfig.CommitChanges();
                }
            }
        }

        private static void Deploy(string name, string path, int port, DotNetVersion version)
        {
            string appName = RemoveSpecialCharacters(name) + port.ToString(CultureInfo.InvariantCulture);
            string dotNetVersion = GetAspDotNetVersion(version);
            string password = Guid.NewGuid().ToString();
            string username = CreateUser(name, password);
            
            mut.WaitOne();
            try
            {
                using (ServerManager serverMgr = new ServerManager())
                {
                    DirectoryInfo deploymentDir = new DirectoryInfo(path);
                    DirectorySecurity deploymentDirSecurity = deploymentDir.GetAccessControl();

                    deploymentDirSecurity.SetAccessRule(new FileSystemAccessRule(username, FileSystemRights.Write | FileSystemRights.Read | FileSystemRights.Delete | FileSystemRights.Modify, InheritanceFlags.ContainerInherit | InheritanceFlags.ObjectInherit, PropagationFlags.None, AccessControlType.Allow));

                    deploymentDir.SetAccessControl(deploymentDirSecurity);

                    Site mySite = serverMgr.Sites.Add(appName, path, port);
                    ApplicationPool applicationPool = serverMgr.ApplicationPools[appName];
                    if (applicationPool == null)
                    {
                        serverMgr.ApplicationPools.Add(appName);
                        applicationPool = serverMgr.ApplicationPools[appName];
                        applicationPool.ManagedRuntimeVersion = dotNetVersion;
                        applicationPool.ProcessModel.IdentityType = ProcessModelIdentityType.SpecificUser;
                        applicationPool.ProcessModel.UserName = username;
                        applicationPool.ProcessModel.Password = password;
                        applicationPool.Enable32BitAppOnWin64 = true;
                    }

                    mySite.Applications["/"].ApplicationPoolName = appName;
                    FirewallTools.OpenPort(port, name);
                    serverMgr.CommitChanges();
                }
            }
            finally
            {
                mut.ReleaseMutex();
            }

            //ToDo: add configuration for timeout
            WaitApp(appName, ObjectState.Started, 20000);
        }

        public static void Delete(int port)
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
                        if(Directory.Exists(path))
                        {
                            DirectoryInfo deploymentDir = new DirectoryInfo(path);
                            DirectorySecurity deploymentDirSecurity = deploymentDir.GetAccessControl();
                            deploymentDirSecurity.RemoveAccessRuleAll(new FileSystemAccessRule(username, FileSystemRights.Write | FileSystemRights.Read | FileSystemRights.Delete | FileSystemRights.Modify, AccessControlType.Allow));
                            deploymentDir.SetAccessControl(deploymentDirSecurity);
                        }
                        DeleteUser(username);
                    }
                }
            }
            finally
            {
                mut.ReleaseMutex();
            }
        }

        public static void Cleanup(string path)
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

            CleanupUser("Uhuru_");
        }

        private static String GetAspDotNetVersion(DotNetVersion version)
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

        private static string CreateUser(String appName, String password)
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

        private static void DeleteUser(String username)
        {
            DirectoryEntry localDirectory = new DirectoryEntry("WinNT://" + Environment.MachineName.ToString());
            DirectoryEntries users = localDirectory.Children;
            DirectoryEntry user = users.Find(username);
            users.Remove(user);
        }

        private static void CleanupUser(string prefix)
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

        public static string RemoveSpecialCharacters(string input)
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

        private static void KillApplicationProcesses(string applicationPoolName)
        {
            using (ServerManager serverMgr = new ServerManager())
            {
                foreach (WorkerProcess process in serverMgr.WorkerProcesses)
                {
                    if (process.AppPoolName == applicationPoolName)
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

        private static void WaitApp(string appName, ObjectState waitForState, int miliseconds)
        {
            using (ServerManager serverMgr = new ServerManager())
            {
                Site site = serverMgr.Sites[appName];
                int timeout = 0;
                while (timeout < miliseconds)
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
    }
}
