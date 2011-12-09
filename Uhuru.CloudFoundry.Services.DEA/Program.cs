// -----------------------------------------------------------------------
// <copyright file="Program.cs" company="Uhuru Software">
// Copyright (c) 2011 Uhuru Software, Inc., All Rights Reserved
// </copyright>
// -----------------------------------------------------------------------

namespace Uhuru.CloudFoundry.DEA.WindowsService
{
	using System;
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
			Directory.SetCurrentDirectory(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location));

            AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(CurrentDomain_UnhandledException);
            if (!Environment.UserInteractive)
			{
				System.ServiceProcess.ServiceBase[] ServicesToRun;
				ServicesToRun = new System.ServiceProcess.ServiceBase[] 
				{ 
					new DeaWindowsService() 
				};
				System.ServiceProcess.ServiceBase.Run(ServicesToRun);
			}
			else
			{
				using (DeaWindowsService deaService = new DeaWindowsService())
				{
					deaService.Start(new string[0]);
                    Console.WriteLine(Strings.PressEnterToStopConsoleMessage);
					Console.ReadLine();
					deaService.Stop();
				}
			}
		}


        static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            Uhuru.Utilities.Logger.Fatal(e.ExceptionObject.ToString());
        }

	}
}
