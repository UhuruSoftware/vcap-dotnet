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
        /// <remarks>
        /// Note that along with all the variables used in the original call to "ConfigureApplication", a "VCAP_APP_PID" value is added to the variables.
        /// This is the process id of the lingering process that you are trying to recover.
        /// </remarks>
        /// <param name="variables">All variables needed to run the application.</param>
        void RecoverApplication(ApplicationVariable[] variables);

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
        /// Cleans up an orphan application.
        /// </summary>
        /// <remarks>
        /// This method needs to make sure that everything is cleaned up.
        /// i.e. Kills processes that are not responding and releases any other used resources.
        /// </remarks>
        void CleanupApplication(string applicationPath);
    }
}
