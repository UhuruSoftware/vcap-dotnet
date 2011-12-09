// -----------------------------------------------------------------------
// <copyright file="Program.cs" company="">
// Copyright (c) 2011 Uhuru Software, Inc., All Rights Reserved
// </copyright>
// -----------------------------------------------------------------------

namespace Uhuru.CloudFoundry.MsSqlService.WindowsService
{
    using System;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Reflection;

    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main(string[] args)
        {
            bool debug = args.Contains("debug");

            Directory.SetCurrentDirectory(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location));
            AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(CurrentDomain_UnhandledException);
            if (!debug)
            {
                System.ServiceProcess.ServiceBase[] ServicesToRun;
                ServicesToRun = new System.ServiceProcess.ServiceBase[] 
			    { 
				    new MsSqlWindowsService() 
			    };
                System.ServiceProcess.ServiceBase.Run(ServicesToRun);
            }
            else
            {
                MsSqlWindowsService msSqlService = new MsSqlWindowsService();
                msSqlService.Start(new string[0]);
                System.Threading.Thread.Sleep(System.Threading.Timeout.Infinite);
            }
        }

        static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            Uhuru.Utilities.Logger.Fatal(e.ExceptionObject.ToString());
        }

    }
}
