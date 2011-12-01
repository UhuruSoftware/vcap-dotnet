using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceProcess;
using System.Threading;

namespace mssqlnode
{
    class Program
    {
        public static string RubyServiceType = String.Empty;

        static void Main(string[] args)
        {
            ServiceBase[] ServicesToRun = new ServiceBase[0];

            RubyServiceType = args[0];

            switch (args[0])
            {
                case "mssqlNodeCmd":
                    ServicesToRun = new ServiceBase[] 
			        {
                        new MssqlNodeService()
                    };
                    break;
            }

            if (args.Length == 2 && args[1] == "debug")
            {
                switch (args[0])
                {
                    case "mssqlNodeCmd":
                        MssqlNodeService mssqlNodeService = new MssqlNodeService();
                        mssqlNodeService.Start(new string[0]);

                        Console.WriteLine("Press Enter to stop.");

                        Console.ReadLine();

                        Console.WriteLine("Service terminated.");

                        mssqlNodeService.Stop();
                        break;
                }
                System.Threading.Thread.Sleep(5000);

            }
            else
            {
                ServiceBase.Run(ServicesToRun);
            }
        }
    }
}
