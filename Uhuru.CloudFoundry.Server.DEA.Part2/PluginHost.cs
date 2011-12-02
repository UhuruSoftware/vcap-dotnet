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
    //the basic data related to a plugin
    struct PluginData
    {
        /// <summary>
        /// the name of the class implementing the IAgentPlugin interface
        /// </summary>
        public string ClassName;

        //the path of the library containing the class that implements the IAgentPlugin interface
        public string FilePath;
    }

    /// <summary>
    /// the class through which the plugins are consumed
    /// </summary>
    public static class PluginHost
    {
        //offers an easier way to access a filePath/className pair later on
        private static Dictionary<Guid, PluginData> knownPluginData = new Dictionary<Guid, PluginData>();
        private static Dictionary<int, AppDomain> runningInstances = new Dictionary<int, AppDomain>();
        
        private static Mutex mutexPluginData = new Mutex();
        private static Mutex mutexInstanceData = new Mutex();

        /// <summary>
        /// saves the plugin data for later reference
        /// </summary>
        /// <param name="pathToPlugin">the path to the plugin</param>
        /// <param name="className">the name of the class implementing the IAgentPlugin interface</param>
        /// <returns>an unique key used later to retrieve the saved data</returns>
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

        /// <summary>
        /// creates a new instance of the plugin
        /// </summary>
        /// <param name="pluginGuid">the unique key used to retrieve previously saved plugin information</param>
        /// <returns>a plugin object</returns>
        public static IAgentPlugin CreateInstance(Guid pluginGuid)
        {
            PluginData data = GetPluginData(pluginGuid);
            if (data.Equals(default(PluginData)))
                throw new KeyNotFoundException("There is no data associated with the given unique key");

            AppDomain domain = AppDomain.CreateDomain(pluginGuid.ToString());
            IAgentPlugin agentPlugin = (IAgentPlugin)domain.CreateInstanceFromAndUnwrap(data.FilePath, data.ClassName);//typeof(IAgentPlugin).FullName);
            
            //save data to the dictionary
            mutexInstanceData.WaitOne();
            runningInstances[agentPlugin.GetHashCode()] = domain;
            mutexInstanceData.ReleaseMutex();

            return agentPlugin;
        }

        /// <summary>
        /// stops a running application and frees the resources used by it 
        /// </summary>
        /// <param name="agent">the plugin running the app</param>
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
