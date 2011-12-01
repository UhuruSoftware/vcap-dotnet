using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Uhuru.CloudFoundry.Server.MsSqlNode;


namespace mssqlnodetestapp
{
    class Program
    {
        static void Main(string[] args)
        {
            Uhuru.CloudFoundry.Server.MsSqlNode.Base.Options opts = new Uhuru.CloudFoundry.Server.MsSqlNode.Base.Options();
            opts.AvailableStorage = 1024;
            opts.BaseDir = @"c:\mssqlnode\";
            opts.GZipBin = "";
            opts.Index = 0;
            opts.LocalDb = @"f:\localdb.xml";
            opts.MaxDbSize = 1024;
            opts.MaxLongQuery = 100000;
            opts.MaxLongTx = 100000;
            opts.MigrationNfs = @"\\192.168.1.4\public\";
            opts.MsSql = new MsSqlOptions();
            opts.MsSql.Host = "192.168.1.4";
            opts.MsSql.User = "sa";
            opts.MsSql.Port = 1433;
            opts.MsSql.Pass = "Password1234!";
            opts.MsSqlBin = "";
            opts.MsSqlDumpBin = "";
            opts.NodeId = "mssql_node_2";
            opts.Uri = "nats://nats:nats@192.168.1.160:4222/";
            opts.ZInterval = 30000;

            Node node = new Node();
            node.Start(opts);

            Console.ReadLine();
        }
    }
}
