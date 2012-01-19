// -----------------------------------------------------------------------
// <copyright file="PluginHost.cs" company="Uhuru Software, Inc.">
// Copyright (c) 2011 Uhuru Software, Inc., All Rights Reserved
// </copyright>
// -----------------------------------------------------------------------

namespace Uhuru.CloudFoundry.DEA
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Threading;
    using Uhuru.CloudFoundry.DEA.PluginBase;
    
    /// <summary>
    /// the class through which the plugins are consumed
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Plugin", Justification = "Word is in dictionary, but warning is still generated.")]
    public static class PluginHost
    {
        /// <summary>
        /// offers an easier way to access a filePath/className pair later on
        /// </summary>
        private static Dictionary<Guid, PluginData> knownPluginData = new Dictionary<Guid, PluginData>();
        
        /// <summary>
        /// Contains all the app domains that have been registered. The key is an IAgentPlugin's hash.
        /// </summary>
        private static Dictionary<int, AppDomain> runningInstances = new Dictionary<int, AppDomain>();
        
        /// <summary>
        /// A mutex used to synchronize access to plugin data across multiple app domains.
        /// </summary>
        private static Mutex mutexPluginData = new Mutex();
        
        /// <summary>
        /// A mutex used to synchronize access to instance data across multiple app domains.
        /// </summary>
        private static Mutex mutexInstanceData = new Mutex();

        /// <summary>
        /// creates a new instance of the plugin
        /// </summary>
        /// <param name="pluginId">the unique key used to retrieve previously saved plugin information</param>
        /// <param name="separateAppdomain">true if the plugin should be loaded into another appdomain</param>
        /// <returns>a plugin object</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Appdomain", Justification = "Not a typo."), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "plugin", Justification = "Word is in dictionary, but warning is still generated.")]
        public static IAgentPlugin CreateInstance(Guid pluginId, bool separateAppdomain)
        {
            PluginData data = GetPluginData(pluginId);
            if (data.Equals(default(PluginData)))
            {
                throw new KeyNotFoundException("There is no data associated with the given unique key");
            }

            IAgentPlugin agentPlugin;

            if (separateAppdomain)
            {
                AppDomain domain = AppDomain.CreateDomain(pluginId.ToString());
                agentPlugin = (IAgentPlugin)domain.CreateInstanceFromAndUnwrap(data.FilePath, data.ClassName);

                // save data to the dictionary
                mutexInstanceData.WaitOne();
                runningInstances[agentPlugin.GetHashCode()] = domain;
                mutexInstanceData.ReleaseMutex();
            }
            else
            {
                // Assembly pluginAssembly = Assembly.Load(data.FilePath);
                // agentPlugin = (IAgentPlugin)pluginAssembly.CreateInstance(data.ClassName);
                agentPlugin = (IAgentPlugin)AppDomain.CurrentDomain.CreateInstanceFromAndUnwrap(data.FilePath, data.ClassName);
            }

            return agentPlugin;
        }

        /// <summary>
        /// stops a running application and frees the resources used by it 
        /// </summary>
        /// <param name="agent">the plugin running the app</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "The stop call is just for cleanup.")]
        public static void RemoveInstance(IAgentPlugin agent)
        {
            if (agent == null)
            {
                throw new ArgumentNullException("agent");
            }

            int hash = agent.GetHashCode();
            AppDomain domain = GetInstanceData(hash);
            if (domain.Equals(default(AppDomain)))
            {
                return; // looks like the data has already been removed
            }

            // throw new KeyNotFoundException("There is no data associated with the given key");
            try
            {
                agent.StopApplication();
            }
            catch (Exception)
            {
            }

            AppDomain.Unload(domain);

            mutexInstanceData.WaitOne();
            runningInstances.Remove(hash);
            mutexInstanceData.ReleaseMutex();
        }

        /// <summary>
        /// saves the plugin data for later reference
        /// </summary>
        /// <param name="pathToPlugin">the path to the plugin</param>
        /// <param name="className">the name of the class implementing the IAgentPlugin interface</param>
        /// <returns>an unique key used later to retrieve the saved data</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Plugin", Justification = "Word is in dictionary, but warning is still generated.")]
        public static Guid LoadPlugin(string pathToPlugin, string className)
        {
            // check if the path & className have a guid already assigned
            if (knownPluginData.Any(kvp => kvp.Value.ClassName == className && kvp.Value.FilePath == pathToPlugin))
            {
                mutexPluginData.WaitOne();
                KeyValuePair<Guid, PluginData> keyValue = knownPluginData.Where(kvp => kvp.Value.ClassName == className && kvp.Value.FilePath == pathToPlugin).FirstOrDefault();
               
                // if something was found, return it
                if (!keyValue.Equals(default(KeyValuePair<Guid, PluginData>)))
                {
                    mutexPluginData.ReleaseMutex();
                    return keyValue.Key;
                }

                mutexPluginData.ReleaseMutex();
            }
                        
            Guid guid = Guid.NewGuid();
            PluginData data = new PluginData() { ClassName = className, FilePath = pathToPlugin };

            mutexPluginData.WaitOne();
            knownPluginData[guid] = data;
            mutexPluginData.ReleaseMutex();

            return guid;
        }

        /// <summary>
        /// Gets the plugin data based on a plugin GUID.
        /// </summary>
        /// <param name="guid">The plugin GUID.</param>
        /// <returns>An instance of PluginData that contains the assembly name and the class name associated with the GUID.</returns>
        private static PluginData GetPluginData(Guid guid)
        {
            PluginData result = default(PluginData);
            if (knownPluginData.ContainsKey(guid))
            {
                mutexPluginData.WaitOne();
                if (knownPluginData.ContainsKey(guid))
                {
                    result = knownPluginData[guid];
                }

                mutexPluginData.ReleaseMutex();
            }

            return result;
        }

        /// <summary>
        /// Gets the app domain of a plugin instance.
        /// </summary>
        /// <param name="hash">The hash of the plugin instance.</param>
        /// <returns>The app domain that is used to sandbox the plugin instance.</returns>
        private static AppDomain GetInstanceData(int hash)
        {
            AppDomain result = default(AppDomain);
            if (runningInstances.ContainsKey(hash))
            {
                mutexInstanceData.WaitOne();
                if (runningInstances.ContainsKey(hash))
                {
                    result = runningInstances[hash];
                }

                mutexInstanceData.ReleaseMutex();
            }

            return result;
        }
    }
}
