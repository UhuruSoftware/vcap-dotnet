using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Uhuru.Utilities;

namespace ConsoleApplication1
{
    class Program
    {
        static void Main(string[] args)
        {
            FileServer fs = new FileServer(9856, @"h:\chess",
                "chess");
            fs.Start();

            Console.ReadLine();
        }
    }
}
