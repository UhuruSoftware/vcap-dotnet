using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using Uhuru.Isolation;
using Uhuru.Utilities;

namespace Uhuru.ProcessPrisonRepl
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("--- PrisonProcess REPL ---\n");
            Console.WriteLine("Use the following keys:");
            Console.WriteLine("\tc: Create a new cmd prison");
            Console.WriteLine("\tn: Create a new notepad prison");
            Console.WriteLine("\td: Destroy all prissons");
            Console.WriteLine("\tq: Quit");

            List<ProcessPrison> prisonss = new List<ProcessPrison>();

            DiskQuotaManager.StartQuotaInitialization();
            while (!DiskQuotaManager.IsQuotaInitialized())
            {
                Thread.Sleep(100);
            }

            
            var usersDesc = WindowsUsersAndGroups.GetUsersDescription();
            foreach (var desc in usersDesc.Values)
            {
                try
                {
                    var id = ProcessPrison.GetIdFromUserDescription(desc);

                    var ppci = new ProcessPrisonCreateInfo();
                    ppci.Id = id;
                    ppci.TotalPrivateMemoryLimit = 128 * 1024 * 1024;
                    ppci.DiskQuotaBytes = 128 * 1024 * 1024;
                    ppci.PrisonHomePath = @"C:\PrisonHome";
                    // Cannot impersonate the user to create new processes or access the user's env.
                    ppci.WindowsPassword = "DontHaveIt"; 

                    var pp = new ProcessPrison();
                    pp.Attach(ppci);

                    prisonss.Add(pp);
                }
                catch(ArgumentException)
                {
                }
            }

            while (true)
            {
                var key = Console.ReadKey();
                if (key.Key == ConsoleKey.Q && key.Modifiers == ConsoleModifiers.Shift) break;

                switch (key.Key)
                {
                    case ConsoleKey.C:
                        {
                            var ppci = new ProcessPrisonCreateInfo();
                            ppci.TotalPrivateMemoryLimit = 128 * 1000 * 1000;
                            ppci.DiskQuotaBytes = 128 * 1024 * 1024;
                            ppci.PrisonHomePath = @"C:\Users\Public";
                            ppci.NetworkOutboundRateLimitBitsPerSecond = 100 * 8 * 1024;
                            ppci.UrlPortAccess = 8811;
                            var pp = new ProcessPrison();
                            pp.Create(ppci);
                            pp.SetUsersEnvironmentVariable("prison", pp.Id);

                            var ri = new ProcessPrisonRunInfo();
                            ri.Interactive = true;
                            ri.FileName = @"C:\Windows\System32\cmd.exe";
                            ri.Arguments = String.Format(" /k  title {1} & echo Wedcome to prison {0}. & echo Running under user {1} & echo Private virtual memory limit: {2} B", pp.Id, pp.WindowsUsername, ppci.TotalPrivateMemoryLimit);
                            ri.Arguments += " & echo. & echo Cmd bomb for memory test: & echo 'set loop=cmd /k ^%loop^%' & echo 'cmd /k %loop%'";
                            ri.Arguments += " & echo. & echo Ruby file server for network test: & echo 'rackup -b 'run Rack::Directory.new(\"\")''";

                            pp.RunProcess(ri);

                            prisonss.Add(pp);
                        }
                        break;
                    case ConsoleKey.N:
                        {
                            var ppci = new ProcessPrisonCreateInfo();
                            ppci.TotalPrivateMemoryLimit = 128 * 1024 * 1024;
                            ppci.DiskQuotaBytes = 128 * 1024 * 1024;
                            ppci.PrisonHomePath = @"C:\Users\Public";

                            var pp = new ProcessPrison();
                            pp.Create(ppci);
                            pp.SetUsersEnvironmentVariable("prison", pp.Id);

                            var ri = new ProcessPrisonRunInfo();
                            ri.Interactive = true;
                            ri.FileName = @"C:\Windows\System32\notepad.exe";

                            pp.RunProcess(ri);

                            prisonss.Add(pp);
                        }
                        break;
                    case ConsoleKey.D:
                        foreach (var prison in prisonss)
                        {
                            prison.Destroy();
                        }
                        prisonss.Clear();
                        break;
                    case ConsoleKey.Q:
                        return;
                }

            }

            var createInfo = new ProcessPrisonCreateInfo();

            var p = new ProcessPrison();

            p.Create(createInfo);
            var envs = p.GetUsersEnvironmentVariables();

            var runInfo = new ProcessPrisonRunInfo();
            runInfo.Interactive = false;
            runInfo.FileName = @"C:\Windows\System32\cmd.exe";
            runInfo.FileName = @"C:\Windows\System32\PING.EXE";
            // runInfo.Arguments = @"/c echo %PATH% & ping 10.0.0.10" ;
            runInfo.Arguments = @" /k rackup -b ""run lambda {|env| [200, {'Content-Type'=>'text/html'}, 'Hello World']}"" -P 2345";

            runInfo.Arguments = " 10.0.0.10 -t";
            runInfo.WorkingDirectory = @"C:\Users\Public";
            runInfo.FileName = @"C:\Windows\System32\mspaint.exe";
            runInfo.Arguments = "";


            p.RunProcess(runInfo);

            //p.RunProcess(@"C:\Windows\System32\mspaint.exe");

            Console.ReadKey();

            p.Destroy();
        }
    }
}
