// -----------------------------------------------------------------------
// <copyright file="IAgentPlugin.cs" company="Uhuru Software">
// Copyright (c) 2011 Uhuru Software, Inc., All Rights Reserved
// </copyright>
// -----------------------------------------------------------------------

namespace Uhuru.CloudFoundry.Server.DEA.PluginBase
{
    /// <summary>
    /// the interface to be implemented by a CF plugin
    /// </summary>
    public interface IAgentPlugin
    {
        /// <summary>
        /// sets the initial data for an application
        /// </summary>
        /// <param name="variables">All variables needed to run the application.</param>
        void ConfigureApplication(ApplicationVariable[] variables);

        /// <summary>
        /// recovers a running application
        /// </summary>
        /// <param name="applicationPath">the path where the app resides</param>
        /// <param name="processId">the id of the processes of the currenly running app</param>
        void RecoverApplication(string applicationPath, int processId);

        /// <summary>
        /// sets the data necessary for debugging the app remotely
        /// </summary>
        /// <param name="debugPort">the port used to reach the app remotely</param>
        /// <param name="debugIp">the ip where the app cand be reached for debug</param>
        /// <param name="debugVariables">the variables necessary for debug, if any</param>
        void ConfigureDebug(string debugPort, string debugIp, ApplicationVariable[] debugVariables);

        /// <summary>
        /// starts the application
        /// </summary>
        void StartApplication();

        /// <summary>
        /// reads the ids of the processes currently used by the running app
        /// </summary>
        /// <returns>the ids of the processes, as an array</returns>
        int GetApplicationProcessID();

        /// <summary>
        /// shuts down the application
        /// </summary>
        void StopApplication();

        /// <summary>
        /// kills all application processes
        /// </summary>
        void KillApplication();

        /// <summary>
        /// Cleans up an orphan application.
        /// </summary>
        void CleanupApplication(string applicationPath);
    }
}
