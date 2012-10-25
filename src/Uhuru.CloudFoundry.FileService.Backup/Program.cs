using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Uhuru.CloudFoundry.FileService.Backup
{
    class Program
    {
        private static FileServiceBackup backup;

        static void Main(string[] args)
        {
            Console.CancelKeyPress += new ConsoleCancelEventHandler(Console_CancelKeyPress);

            backup = new FileServiceBackup();

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
