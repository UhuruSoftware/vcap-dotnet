using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.IO;
using System.Reflection;
using System.Diagnostics;

namespace Uhuru.CloudFoundry.MsSqlService.WindowsService
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
            EventLog.WriteEntry("MsSQL", e.ExceptionObject.ToString(), EventLogEntryType.Error);
        }

    }
}
