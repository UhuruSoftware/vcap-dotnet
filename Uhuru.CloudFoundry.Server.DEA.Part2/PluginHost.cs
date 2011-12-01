using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Uhuru.CloudFoundry.Server.DEA.PluginBase;
using System.Runtime.Remoting;
using System.Reflection;
using System.IO;
using System.Threading;

namespace Uhuru.CloudFoundry.Server.DEA
{
    struct PluginData
    {
        public string ClassName;
        public string FilePath;
    }

    public static class PluginHost
    {
        //offers an easier way to access a filePath/className pair later on
        private static Dictionary<Guid, PluginData> knownPluginData = new Dictionary<Guid, PluginData>();
        private static Dictionary<int, AppDomain> runningInstances = new Dictionary<int, AppDomain>();
        
        private static Mutex mutexPluginData = new Mutex();
        private static Mutex mutexInstanceData = new Mutex();

        //initializes a placeholder where the data will be placed after creation
        public static Guid LoadPlugin(string pathToPlugin, string className)
        {
            //check if the path & className have a guid already assigned
            if (knownPluginData.Any(kvp => kvp.Value.ClassName == className && kvp.Value.FilePath == pathToPlugin))
            {
                mutexPluginData.WaitOne();
                KeyValuePair<Guid, PluginData> keyValue = knownPluginData.Where(kvp => kvp.Value.ClassName == className && kvp.Value.FilePath == pathToPlugin).FirstOrDefault();
                //if something was found, return it
                if (!keyValue.Equals(default(KeyValuePair<Guid, PluginData>)))
                {
                    mutexPluginData.ReleaseMutex();
                    return keyValue.Key;
                }

                mutexPluginData.ReleaseMutex();
            }
                        
            Guid guid = Guid.NewGuid();
            PluginData data = new PluginData() { ClassName = className, FilePath = pathToPlugin};

            mutexPluginData.WaitOne();
            knownPluginData[guid] = data;
            mutexPluginData.ReleaseMutex();

            return guid;
        }

        private static PluginData GetPluginData(Guid guid)
        {
            PluginData result = default(PluginData);
            if (knownPluginData.ContainsKey(guid))
            {
                mutexPluginData.WaitOne();
                if (knownPluginData.ContainsKey(guid))
                    result = knownPluginData[guid];
                mutexPluginData.ReleaseMutex();
            }

            return result;
        }

        private static AppDomain GetInstanceData(int hash)
        {
            AppDomain result = default(AppDomain);
            if (runningInstances.ContainsKey(hash))
            {
                mutexInstanceData.WaitOne();
                if (runningInstances.ContainsKey(hash))
                    result = runningInstances[hash];
                mutexInstanceData.ReleaseMutex();
            }

            return result;
        }


        public static IAgentPlugin CreateInstance(Guid pluginGuid)
        {
            PluginData data = GetPluginData(pluginGuid);
            if (data.Equals(default(PluginData)))
                throw new KeyNotFoundException("There is no data associated with the given unique key");

            AppDomain domain = AppDomain.CreateDomain(pluginGuid.ToString());
            domain.AssemblyResolve +=new ResolveEventHandler(delegate(object sender, ResolveEventArgs args)
                {

                    return null;
                });
            IAgentPlugin agentPlugin = (IAgentPlugin)domain.CreateInstanceFromAndUnwrap(data.FilePath, "TheDLLToLoad.TestClass");//typeof(IAgentPlugin).FullName);
            
            //save data to the dictionary
            mutexInstanceData.WaitOne();
            runningInstances[agentPlugin.GetHashCode()] = domain;
            mutexInstanceData.ReleaseMutex();

            return agentPlugin;
        }

        public static void RemoveInstance(IAgentPlugin agent)
        {
            int hash = agent.GetHashCode();
            AppDomain domain = GetInstanceData(hash);
            if (domain.Equals(default(AppDomain))) return; //looks like the data has already been removed
                //throw new KeyNotFoundException("There is no data associated with the given key");

            agent.StopApplication();
            AppDomain.Unload(domain);

            mutexInstanceData.WaitOne();
            runningInstances.Remove(hash);
            mutexInstanceData.ReleaseMutex();
        }
    }
}
