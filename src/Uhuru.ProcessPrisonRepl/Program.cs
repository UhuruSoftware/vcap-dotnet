using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Uhuru.Isolation;

namespace Uhuru.ProcessPrisonRepl
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("--- PrisonProcess REPL ---\n");
            Console.WriteLine("Use the following keys:");
            Console.WriteLine("\tc: Create a new prison");
            Console.WriteLine("\td: Destroy all prissons");
            Console.WriteLine("\tq: Quit");

            List<ProcessPrison> prisonss = new List<ProcessPrison>();

            while (true)
            {
                var key = Console.ReadKey();
                if (key.Key == ConsoleKey.Q && key.Modifiers == ConsoleModifiers.Shift) break;

                switch (key.Key)
                {
                    case ConsoleKey.C:
                        var ppci = new ProcessPrisonCreateInfo();
                        // ppci.WindowsPassword = "password1234!";
                        ppci.TotalPrivateMemoryLimit = 128 * 1024 * 1024;

                        var pp = new ProcessPrison();
                        pp.Create(ppci);

                        var ri = new ProcessPrisonRunInfo();
                        ri.FileName = @"C:\Windows\System32\cmd.exe";
                        ri.Arguments = String.Format(" /k echo Wedcome to prisson {0}", pp.Id);

                        pp.RunProcess(ri);

                        prisonss.Add(pp);

                        break;
                    case ConsoleKey.D:
                        foreach (var prison in prisonss)
                        {
                            prison.Destroy();
                        }

                        break;
                    case ConsoleKey.Q:
                        return;
                }

            }

            var createInfo = new ProcessPrisonCreateInfo();
            // createInfo.WindowsPassword = "password1234!";

            var p = new ProcessPrison();

            p.Create(createInfo);
            var envs = p.GetUsersEnvironmentVariables();

            var runInfo = new ProcessPrisonRunInfo();
            runInfo.CreateWindow = false;
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
