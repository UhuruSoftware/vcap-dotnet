// -----------------------------------------------------------------------
// <copyright file="PluginHost.cs" company="Uhuru Software, Inc.">
// Copyright (c) 2011 Uhuru Software, Inc., All Rights Reserved
// </copyright>
// -----------------------------------------------------------------------

namespace Uhuru.CloudFoundry.DEA
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Security.Cryptography;
    using Uhuru.CloudFoundry.DEA.PluginBase;
    
    /// <summary>
    /// the class through which the plugins are consumed
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Plugin", Justification = "Word is in dictionary, but warning is still generated.")]
    public static class PluginHost
    {
        /// <summary>
        /// Concurrency lock for types cache.
        /// </summary>
        private static readonly object pluginLock = new object();
        
        /// <summary>
        /// offers an easier way to access a filePath/className pair later on
        /// </summary>
        private static Dictionary<Guid, PluginData> knownPluginData = new Dictionary<Guid, PluginData>();

        /// <summary>
        /// Keeps track of plugin instances and their associated IDs
        /// </summary>
        private static Dictionary<IAgentPlugin, Guid> pluginInstanceToPluginId = new Dictionary<IAgentPlugin, Guid>();

        /// <summary>
        /// Usage count for a Type
        /// </summary>
        private static Dictionary<Guid, int> typeChacheUsage = new Dictionary<Guid, int>();

        /// <summary>
        /// creates a new instance of the plugin
        /// </summary>
        /// <param name="pluginId">the unique key used to retrieve previously saved plugin information</param>
        /// <returns>a plugin object</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Appdomain", Justification = "Not a typo."), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "plugin", Justification = "Word is in dictionary, but warning is still generated.")]
        public static IAgentPlugin CreateInstance(Guid pluginId)
        {
            lock (pluginLock)
            {
                PluginData data = GetPluginData(pluginId);
                if (data.Equals(default(PluginData)))
                {
                    throw new KeyNotFoundException("There is no data associated with the given unique key");
                }
                
                int usage = 0;
                typeChacheUsage.TryGetValue(pluginId, out usage);
                typeChacheUsage[pluginId] = usage + 1;
                
                return (IAgentPlugin)data.PluginConstructor.Invoke(null);
            }
        }

        /// <summary>
        /// stops a running application and frees the resources used by it 
        /// </summary>
        /// <param name="agent">the plugin running the app</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "The stop call is just for cleanup.")]
        public static void RemoveInstance(IAgentPlugin agent)
        {
            lock (pluginLock)
            {
                if (agent == null)
                {
                    throw new ArgumentNullException("agent");
                }

                Guid pluginId = Guid.Empty;
                if (!pluginInstanceToPluginId.TryGetValue(agent, out pluginId))
                {
                    return;
                }

                try
                {
                    agent.StopApplication();
                }
                catch (Exception)
                {
                }

                pluginInstanceToPluginId.Remove(agent);

                int usage = 0;
                typeChacheUsage.TryGetValue(pluginId, out usage);
                if (usage != 0)
                {
                    if (usage == 1)
                    {
                        PluginData pluginData = knownPluginData[pluginId];
                        AppDomain.Unload(pluginData.PluginDomain);
                        typeChacheUsage.Remove(pluginId);
                        knownPluginData.Remove(pluginId);
                    }
                    else
                    {
                        typeChacheUsage[pluginId] = usage - 1;
                    }
                }
            }
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
            lock (pluginLock)
            {
                // check if the path & className have a guid already assigned
                if (knownPluginData.Any(kvp => kvp.Value.ClassName == className && kvp.Value.FilePath == pathToPlugin))
                {
                    KeyValuePair<Guid, PluginData> keyValue = knownPluginData.Where(kvp => kvp.Value.ClassName == className && kvp.Value.FilePath == pathToPlugin).FirstOrDefault();

                    // if something was found, return it
                    if (!keyValue.Equals(default(KeyValuePair<Guid, PluginData>)))
                    {
                        return keyValue.Key;
                    }
                }

                Guid guid = Guid.NewGuid();
                AppDomain newDomain = null;

                string assemblyHash = string.Empty;

                // Create hash for assembly
                using (FileStream fs = File.OpenRead(pathToPlugin))
                {
                    using (SHA1CryptoServiceProvider cryptoProvider = new SHA1CryptoServiceProvider())
                    {
                        assemblyHash = BitConverter.ToString(cryptoProvider.ComputeHash(fs)).Replace("-", string.Empty);
                    }
                }

                string appPath = Path.Combine(Path.GetDirectoryName(Assembly.GetAssembly(typeof(PluginHost)).Location), assemblyHash);

                Directory.CreateDirectory(appPath);

                // Copy assembly to destination (no overwrite)
                string assemblyFileName = Path.GetFileName(pathToPlugin);
                string finalAssemblyPath = Path.Combine(appPath, assemblyFileName);
                File.Copy(pathToPlugin, finalAssemblyPath, true);

                ConstructorInfo constructor = GetPluginTypeBuilder(finalAssemblyPath, className, out newDomain);

                PluginData data = new PluginData() 
                { 
                    ClassName = className,
                    FilePath = pathToPlugin,
                    PluginConstructor = constructor,
                    PluginDomain = newDomain
                };

                knownPluginData[guid] = data;

                return guid;
            }
        }

        /// <summary>
        /// Gets the plugin data based on a plugin GUID.
        /// </summary>
        /// <param name="guid">The plugin GUID.</param>
        /// <returns>An instance of PluginData that contains the assembly name and the class name associated with the GUID.</returns>
        private static PluginData GetPluginData(Guid guid)
        {
            lock (pluginLock)
            {
                PluginData result = default(PluginData);
                if (knownPluginData.ContainsKey(guid))
                {
                    if (knownPluginData.ContainsKey(guid))
                    {
                        result = knownPluginData[guid];
                    }
                }

                return result;
            }
        }

        /// <summary>
        /// Helper method that gets a plugin's constructor, and caches it for future reference. 
        /// </summary>
        /// <param name="assemblyPath">Location of assembly.</param>
        /// <param name="className">Fully qualified name of the plugin class.</param>
        /// <param name="domain">Out parameter that will contain the app domain created for the plugin.</param>
        /// <returns>A ConstructorInfo object that can be used to instantiate the plugin.</returns>
        private static ConstructorInfo GetPluginTypeBuilder(string assemblyPath, string className, out AppDomain domain)
        {
            AppDomain newDomain = AppDomain.CreateDomain(assemblyPath + ";" + className);
            Assembly assembly = newDomain.Load(File.ReadAllBytes(assemblyPath));
            domain = newDomain;
            return assembly.GetType(className).GetConstructor(System.Type.EmptyTypes);
        }
    }
}
