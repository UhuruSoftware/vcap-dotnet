using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Uhuru.CloudFoundry.Server.MsSqlNode
{
    public class MsSqlOptions
    {
        public string Host
        {
            get;
            set;
        }

        public string User
        {
            get;
            set;
        }
        
        public string Pass
        {
            get;
            set;
        }

        public int Port
        {
            get;
            set;
        }

    }
}
