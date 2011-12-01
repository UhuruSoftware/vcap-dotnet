using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using System.Threading;
using System.Collections;
using System.Xml;
using System.IO;
using System.Diagnostics;
using CloudFoundry.Net.IIS.Utilities;
using CloudFoundry.Net.IIS;

namespace netiis
{
    class Program
    {
        static AutoResetEvent closing = new AutoResetEvent(false);
        static ConsoleTools consoleTools = new ConsoleTools(closing);

        static void Main(string[] args)
        {
            CmdArguments arguments = new CmdArguments(args);
            if (arguments.HasParameter("?"))
            {
                Console.WriteLine(@"
-add                Creates a new application
-name=[app name]    Name of new application (site) to be created
-port=[port nr]     Port to deploy the new application on
-path=[path]        Physical path of application
-watcher            Watcher mode; when application is closed, the site will be deleted
-autowire           Replace connection strings in web.config file

-delete             Deletes an application
-port               Port of application (site) to be deleted

-stop               Stops a running watcher netiis instance
-pid=[process id]   Process ID of the running netiis instance

-cleanup=[path]     Cleans up all apps that have the root directory in the specified path
");
                return;
            }

            if (arguments.HasParameter("add"))
            {
                string appName = arguments["name"];
                string port = arguments["port"];
                string path = arguments["path"];

                bool watch = arguments.HasParameter("watcher");

                Operations.Create(appName, Convert.ToInt32(port), path, arguments.HasParameter("autowire"));

                if (watch)
                {
                    ConsoleTools.SetConsoleCtrlHandler(
                        new ConsoleTools.HandlerRoutine(consoleTools.ConsoleCtrlCheck), true);

                    closing.WaitOne();

                    Operations.Delete(Convert.ToInt32(port));
                }
            }
            else if (arguments.HasParameter("delete"))
            {
                string port = arguments["port"];
                Operations.Delete(Convert.ToInt32(port));
            }
            else if (arguments.HasParameter("stop"))
            {
                uint pid = Convert.ToUInt32(arguments["pid"]);
                ConsoleTools.FreeConsole();
                ConsoleTools.AttachConsole(pid);
                ConsoleTools.GenerateConsoleCtrlEvent(ConsoleTools.Ctrl_Break_Event, 0);

                try
                {
                    Process runningInstance = Process.GetProcessById((int)pid);
                    runningInstance.WaitForExit();
                }
                catch (ArgumentException) // no process for that PID
                { }
            }
            else if (arguments.HasParameter("cleanup"))
            {
                string path = arguments["cleanup"];
                Operations.Cleanup(path);
            }
        }
    }
}
