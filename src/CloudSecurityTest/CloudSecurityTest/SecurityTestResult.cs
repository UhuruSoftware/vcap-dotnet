using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CloudSecurityTest
{
    class SecurityTest
    {
        public void Message(string message, params object[] parameters)
        {
            Console.WriteLine("Message: {0}", string.Format(message, parameters));
        }

        public void Score(double score)
        {            
            Console.WriteLine("Score: {0}", score);
        }
    }
}
