
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
        /// <param name="appInfo">basic information about the app</param>
        /// <param name="runtime">app's runtime</param>
        /// <param name="variables">some other variables, if necessary</param>
        /// <param name="services">the services the app may want to use</param>
        /// <param name="logFilePath">the file path where the logs will be saved</param>
        void ConfigureApplication(ApplicationInfo appInfo, Runtime runtime, ApplicationVariable[] variables, ApplicationService[] services, string logFilePath);

        /// <summary>
        /// updates the data of a running application
        /// </summary>
        /// <param name="appInfo">basic information about the app</param>
        /// <param name="runtime">app's runtime</param>
        /// <param name="variables">other app variables, if necessary</param>
        /// <param name="services">the services to be used by the app</param>
        /// <param name="logFilePath">the file path where the logs will be saved</param>
        /// <param name="processIds">the ids of the processes of the currenly running app</param>
        void ConfigureApplication(ApplicationInfo appInfo, Runtime runtime, ApplicationVariable[] variables, ApplicationService[] services, string logFilePath, int[] processIds);

        /// <summary>
        /// a delegate used to devise a way to cope with a (potential) application crash
        /// </summary>
        event ApplicationCrashDelegate OnApplicationCrash;

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
        int[] GetApplicationProcessIDs();

        /// <summary>
        /// shuts down the application
        /// </summary>
        void StopApplication();

        /// <summary>
        /// kills all application processes
        /// </summary>
        void KillApplication();
    }
}
