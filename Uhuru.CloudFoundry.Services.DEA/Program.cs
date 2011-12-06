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
			bool debug = args.Contains("debug");

			Directory.SetCurrentDirectory(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location));

			if (!debug)
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
				Console.WriteLine("Press enter to stop service...");
				DeaWindowsService deaService = new DeaWindowsService();
				deaService.Start(new string[0]);
				
				Console.ReadLine();
				deaService.Stop();
				//System.Threading.Thread.Sleep(System.Threading.Timeout.Infinite);
			}
		}
	}
}
