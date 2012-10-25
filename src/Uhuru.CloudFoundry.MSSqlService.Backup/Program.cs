using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Uhuru.CloudFoundry.MSSqlService.Backup
{
    class Program
    {
        private static MSSqlBackup backup;

        static void Main(string[] args)
        {
            Console.CancelKeyPress += new ConsoleCancelEventHandler(Console_CancelKeyPress);

            backup = new MSSqlBackup();

            if (args.Contains("-t"))
            {
                backup.IsTolerant = true;
            }
            else
            {
                backup.IsTolerant = false;
            }

            backup.Start();
        }

        static void Console_CancelKeyPress(object sender, ConsoleCancelEventArgs e)
        {
            backup.ShutdownJob = true;
            e.Cancel = true;
        }
    }
}
