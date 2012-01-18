// -----------------------------------------------------------------------
// <copyright file="Program.cs" company="Uhuru Software, Inc.">
// Copyright (c) 2011 Uhuru Software, Inc., All Rights Reserved
// </copyright>
// -----------------------------------------------------------------------

namespace Uhuru.CloudFoundry.DEA.WindowsService
{
    using System;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Reflection;

    /// <summary>
    /// This is where it all starts for the DEA.
    /// </summary>
    internal static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        internal static void Main()
        {
            // sets the DEA priority higher.
            Process.GetCurrentProcess().PriorityClass = ProcessPriorityClass.AboveNormal;

            Directory.SetCurrentDirectory(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location));

            AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(CurrentDomain_UnhandledException);
            if (!Environment.UserInteractive)
            {
                System.ServiceProcess.ServiceBase[] servicesToRun;
                servicesToRun = new System.ServiceProcess.ServiceBase[] 
                { 
                    new DeaWindowsService() 
                };
                System.ServiceProcess.ServiceBase.Run(servicesToRun);
            }
            else
            {
                using (DeaWindowsService deaService = new DeaWindowsService())
                {
                    deaService.Start();
                    Console.WriteLine(Strings.PressEnterToStopConsoleMessage);
                    Console.ReadLine();
                    deaService.Stop();
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
