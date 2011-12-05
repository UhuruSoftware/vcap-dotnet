using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Reflection;

namespace Uhuru.CloudFoundry.DEA.WindowsService
{
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
