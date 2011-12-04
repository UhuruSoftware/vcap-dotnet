using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Uhuru.CloudFoundry.ServiceBase
{
    public class Options
    {
        private int index = 0;
        private int zInterval = 30000;

        public string NodeId
        {
            get;
            set;
        }

        public string MigrationNFS
        {
            get;
            set;
        }

        public string Uri
        {
            get;
            set;
        }

        public int Index 
        { 
            get
            {
                return index;
            }
            set
            {
                index = value;
            }
        }

        public int ZInterval
        {
            get
            {
                return zInterval;
            }
            set
            {
                zInterval = value;
            }
        }

        public int MaxDBSize
        {
            get;
            set;
        }

        public int MaxLengthyQuery
        {
            get;
            set;
        }

        public int MaxLengthyTX
        {
            get;
            set;
        }

        public string BaseDir
        {
            get;
            set;
        }

        public int AvailableStorage
        {
            get;
            set;
        }

        public string LocalDB { get; set; }

        public string LocalRoute { get; set; }
    }
}
