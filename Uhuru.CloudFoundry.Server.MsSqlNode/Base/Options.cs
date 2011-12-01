using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Uhuru.CloudFoundry.Server.MsSqlNode.Base
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

        public string MigrationNfs
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

        public MsSqlOptions MsSql
        {
            get;
            set;
        }

        public int MaxDbSize
        {
            get;
            set;
        }

        public int MaxLongQuery
        {
            get;
            set;
        }

        public int MaxLongTx
        {
            get;
            set;
        }

        public string MsSqlDumpBin
        {
            get;
            set;
        }

        public string GZipBin
        {
            get;
            set;
        }

        public string MsSqlBin
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

        public string LocalDb { get; set; }
    }
}
