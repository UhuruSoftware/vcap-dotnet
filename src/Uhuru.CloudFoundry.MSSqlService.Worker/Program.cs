using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.IO;
using System.Reflection;
using Uhuru.CloudFoundry.MSSqlWorker;
using Uhuru.CloudFoundry.MSSqlService.Job;

namespace Uhuru.CloudFoundry.MSSqlService.Worker
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        internal static void Main()
        {
            Directory.SetCurrentDirectory(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location));

            AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(CurrentDomain_UnhandledException);
            if (!Environment.UserInteractive)
            {
                System.ServiceProcess.ServiceBase[] servicesToRun;
                servicesToRun = new System.ServiceProcess.ServiceBase[] 
                {
                    new MSSqlWorkerWindowsService() 
                };
                System.ServiceProcess.ServiceBase.Run(servicesToRun);
            }
            else
            {
                using (MSSqlWorkerWindowsService sqlWorkerService = new MSSqlWorkerWindowsService())
                {
                    sqlWorkerService.Start();
                    Console.WriteLine("Press enter to stop service...");
                    Console.ReadLine();
                    sqlWorkerService.Stop();
                }
            }
        }

        /// <summary>
        /// Handles the UnhandledException event of the CurrentDomain control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.UnhandledExceptionEventArgs"/> instance containing the event data.</param>
        internal static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            Uhuru.Utilities.Logger.Fatal(e.ExceptionObject.ToString());
        }
    }
}
