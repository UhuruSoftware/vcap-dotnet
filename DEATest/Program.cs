using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CloudFoundry.Net.DEA;

namespace DEATest
{
    class Program
    {
        static void Main(string[] args)
        {
            Agent agent = new Agent();

            agent.Run();

            Console.WriteLine("Press the any key to initiate shutdown!");
            Console.ReadLine();

            agent.Shutdown();

            Console.WriteLine("Shutting down... Press enter after the shutdown is complete... P.S. Yeah, you find out if it's compleated, I'm not doing that anymore after I'm closed");
            Console.ReadLine();

        }
    }
}
