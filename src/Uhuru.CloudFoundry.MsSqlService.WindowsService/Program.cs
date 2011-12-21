// -----------------------------------------------------------------------
// <copyright file="Program.cs" company="Uhuru Software, Inc.">
// Copyright (c) 2011 Uhuru Software, Inc., All Rights Reserved
// </copyright>
// -----------------------------------------------------------------------

namespace Uhuru.CloudFoundry.MSSqlService.WindowsService
{
    using System;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Reflection;

    /// <summary>
    /// This is where it all starts.
    /// </summary>
    internal static class Program
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
                    new MSSqlWindowsService() 
                };
                System.ServiceProcess.ServiceBase.Run(servicesToRun);
            }
            else
            {
                using (MSSqlWindowsService sqlService = new MSSqlWindowsService())
                {
                    sqlService.Start();
                    Console.WriteLine(Strings.PressEnterToStopConsoleMessage);
                    Console.ReadLine();
                    sqlService.Stop();
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
