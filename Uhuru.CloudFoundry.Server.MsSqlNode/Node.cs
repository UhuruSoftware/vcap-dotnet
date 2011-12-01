using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Uhuru.CloudFoundry.Server.MsSqlNode.Base;
using System.Data.SqlClient;
using System.Threading;
using System.Diagnostics;
using Uhuru.CloudFoundry.Server;
using System.IO;
using Uhuru.Utilities;
using System.Transactions;
using System.Data;

namespace Uhuru.CloudFoundry.Server.MsSqlNode
{
    public partial class Node : Base.Node
    {
        const int KEEP_ALIVE_INTERVAL = 15000;
        const int LONG_QUERY_INTERVAL = 1;
        const int STORAGE_QUOTA_INTERVAL = 1;
        private MsSqlOptions mssql_config;
        private int max_db_size;
        private int max_long_query;
        private int max_long_tx;
        private string mssqldump_bin;
        private string mssql_bin;
        SqlConnection connection;
        private string base_dir;
        private int available_storage;
        private int node_capacity;
        private int queries_served;
        private DateTime qps_last_updated;
        private int long_queries_killed;
        private int long_tx_killed;
        private int provision_served;
        private int binding_served;
        private string local_ip;


        protected override string service_name()
        {
            return "MssqlaaS";
        }

        public void Start(Options options)
        {
            mssql_config = options.MsSql;
            max_db_size = options.MaxDbSize * 1024 * 1024;
            max_long_query = options.MaxLongQuery;
            max_long_tx = options.MaxLongTx;
            mssqldump_bin = options.MsSqlDumpBin;
            gzip_bin = options.GZipBin;
            mssql_bin = options.MsSqlBin;


            connection = mssql_connect();

            TimerHelper.RecurringCall(KEEP_ALIVE_INTERVAL, delegate()
            {
                mssql_keep_alive();
            });

            if (max_long_query > 0)
            {
                TimerHelper.RecurringCall(max_long_query / 2, delegate()
                {
                    kill_long_queries();
                });
            }

            if (max_long_tx > 0)
            {
                TimerHelper.RecurringCall(max_long_tx / 2, delegate()
                {
                    mssql_keep_alive();
                });
            }

            TimerHelper.RecurringCall(STORAGE_QUOTA_INTERVAL, delegate()
            {
                enforce_storage_quota();
            });

            base_dir = options.BaseDir;
            if (!String.IsNullOrEmpty(base_dir))
            {
                Directory.CreateDirectory(base_dir);
            }

            ProvisionedService.Initialize(options.LocalDb);

            check_db_consistency();

            available_storage = options.AvailableStorage * 1024 * 1024;
            node_capacity = available_storage;

            foreach (ProvisionedService provisioned_service in ProvisionedService.Instances)
            {
                available_storage -= storage_for_service(provisioned_service);
            }

            queries_served = 0;
            qps_last_updated = DateTime.Now;
            // initialize qps counter
            get_qps();
            long_queries_killed = 0;
            long_tx_killed = 0;
            provision_served = 0;
            binding_served = 0;
            base.Start(options);

        }

        protected override Announcement announcement()
        {
            Announcement a = new Announcement();
            a.AvailableStorage = available_storage;
            return a;
        }

        private void check_db_consistency()
        {
            // method present in mysql and postgresql
            //todo: vladi: this should be replaced with ms sql server code
        }

        private int storage_for_service(ProvisionedService provisioned_service)
        {
            switch (provisioned_service.Plan)
            {
                case ProvisionedServicePlanType.FREE: return max_db_size;
                default: throw new MsSqlError(MsSqlError.MSSQL_INVALID_PLAN, provisioned_service.Plan.ToString());
            }
        }

        private string ConnectionString
        {
            get
            {
                return String.Format("Data Source={0},{1};User Id={2};Password={3};MultipleActiveResultSets=true", mssql_config.Host, mssql_config.Port, mssql_config.User, mssql_config.Pass);
            }
        }

        private SqlConnection mssql_connect()
        {
            for (int i = 0; i < 5; i++)
            {
                connection = new SqlConnection(ConnectionString);

                try
                {
                    connection.Open();
                    return connection;
                }
                catch (InvalidOperationException)
                {
                }
                catch (SqlException)
                {
                }
                Thread.Sleep(5000);
            }

            Logger.fatal("MsSql connection unrecoverable");
            shutdown();
            Process.GetCurrentProcess().Kill();
            return null;
        }

        //keep connection alive, and check db liveness
        private void mssql_keep_alive()
        {
            // present in both mysql and postgresql
            try
            {
                SqlCommand cmd = new SqlCommand("select CURRENT_TIMESTAMP", connection);
                cmd.ExecuteScalar();
            }
            catch (Exception ex)
            {
                Logger.warn(String.Format("MsSql connection lost: {0}", ex.ToString()));
                connection = mssql_connect();
            }
        }

        private void kill_long_queries()
        {
            //present in both mysql and postgresql
            //todo: vladi: Replace with code for odbc object for SQL Server
        }

        private void kill_long_transaction()
        {
            //present in both mysql and postgresql
            //todo: vladi: Replace with code for odbc object for SQL Server
        }

        protected override ServiceCredentials provision(ProvisionedServicePlanType plan, ServiceCredentials credential = null)
        {
            ProvisionedService provisioned_service = new ProvisionedService();
            try
            {
                if (credential != null)
                {
                    string name = credential.Name;
                    string user = credential.User;
                    string password = credential.Password;
                    provisioned_service.Name = name;
                    provisioned_service.User = user;
                    provisioned_service.Password = password;
                }
                else
                {
                    // mssql database name should start with alphabet character
                    provisioned_service.Name = "D4Ta" + Guid.NewGuid().ToString("N");
                    provisioned_service.User = "US3r" + Util.generate_credential();
                    provisioned_service.Password = "P4Ss" + Util.generate_credential();

                }
                provisioned_service.Plan = plan;

                create_database(provisioned_service);

                if (!provisioned_service.Save())
                {
                    Logger.error(String.Format("Could not save entry: {0}", provisioned_service.ToJson()));
                    throw new MsSqlError(MsSqlError.MSSQL_LOCAL_DB_ERROR);
                }

                ServiceCredentials response = gen_credential(provisioned_service.Name, provisioned_service.User, provisioned_service.Password);
                provision_served += 1;
                return response;
            }
            catch (Exception ex)
            {
                delete_database(provisioned_service);
                throw ex;
            }
        }

        protected override bool unprovision(string name, ServiceCredentials[] credentials)
        {
            if (String.IsNullOrEmpty(name))
            {
                return false;
            }

            Logger.debug(String.Format("Unprovision database:{0}, bindings: {1}", name, credentials.ToJson()));


            ProvisionedService provisioned_service = ProvisionedService.GetService(name);

            if (provisioned_service == null)
            {
                throw new MsSqlError(MsSqlError.MSSQL_CONFIG_NOT_FOUND, name);
            }

            // TODO: validate that database files are not lingering
            // Delete all bindings, ignore not_found error since we are unprovision
            try
            {
                if (credentials != null)
                {
                    foreach (ServiceCredentials credential in credentials)
                    {
                        unbind(credential);
                    }
                }
            }
            catch (Exception)
            {
                // ignore
            }

            delete_database(provisioned_service);
            int storage = storage_for_service(provisioned_service);
            available_storage += storage;

            if (!provisioned_service.Destroy())
            {
                Logger.error(String.Format("Could not delete service: {0}", provisioned_service.Name));
                throw new MsSqlError(MsSqlError.MSSQL_LOCAL_DB_ERROR);
            }

            Logger.debug(String.Format("Successfully fulfilled unprovision request: {0}", name));
            return true;
        }

        protected override ServiceCredentials bind(string name, Dictionary<string, object> bind_opts, ServiceCredentials credential = null)
        {
            Logger.debug(String.Format("Bind service for db:{0}, bind_opts = {1}", name, bind_opts.ToJson()));
            Dictionary<string, object> binding = null;
            try
            {
                ProvisionedService service = ProvisionedService.GetService(name);
                if (service == null)
                {
                    throw new MsSqlError(MsSqlError.MSSQL_CONFIG_NOT_FOUND, name);
                }
                // create new credential for binding
                binding = new Dictionary<string, object>();

                if (credential != null)
                {
                    binding["user"] = credential.User;
                    binding["password"] = credential.Password;
                }
                else
                {
                    binding["user"] = "US3R" + Util.generate_credential();
                    binding["password"] = "P4SS" + Util.generate_credential();
                }
                binding["bind_opts"] = bind_opts;

                create_database_user(name, binding["user"] as string, binding["password"] as string);
                ServiceCredentials response = gen_credential(name, binding["user"] as string, binding["password"] as string);

                Logger.debug(String.Format("Bind response: {0}", response.ToJson()));
                binding_served += 1;
                return response;
            }
            catch (Exception ex)
            {
                if (binding != null)
                {
                    delete_database_user(binding["user"] as string);
                }
                throw ex;
            }
        }

        protected override bool unbind(ServiceCredentials credential)
        {
            if (credential == null)
            {
                return false;
            }

            Logger.debug(String.Format("Unbind service: {0}", credential.ToJson()));

            string name = credential.Name;
            string user = credential.User;
            Dictionary<string, object> bind_opts = credential.BindOptions;
            string password = credential.Password;

            ProvisionedService service = ProvisionedService.GetService(name);
            if (service == null)
            {
                throw new MsSqlError(MsSqlError.MSSQL_CONFIG_NOT_FOUND, name);
            }
            //todo: vladi: implement this on windows
            // validate the existence of credential, in case we delete a normal account because of a malformed credential
            //res = @connection.select_all("SELECT * from mssql.user WHERE user='#{user}' AND password=PASSWORD('#{passwd}')")
            //raise MsSqlError.new(MsSqlError::MSSQL_CRED_NOT_FOUND, credential.inspect) if res.num_rows()<=0
            delete_database_user(user);
            return true;
        }

        private void create_database(ProvisionedService provisioned_service)
        {
            string name = provisioned_service.Name;
            string password = provisioned_service.Password;
            string user = provisioned_service.User;

            try
            {
                DateTime start = DateTime.Now;
                Logger.debug(String.Format("Creating: {0}", provisioned_service.ToJson()));

                new SqlCommand(String.Format("CREATE DATABASE {0}", name), connection).ExecuteNonQuery();

                create_database_user(name, user, password);
                int storage = storage_for_service(provisioned_service);
                if (available_storage < storage)
                {
                    throw new MsSqlError(MsSqlError.MSSQL_DISK_FULL);
                }
                available_storage -= storage;
                Logger.debug(String.Format("Done creating {0}. Took {1} s.", provisioned_service.ToJson(), (start-DateTime.Now).TotalSeconds));
            }
            catch (Exception ex)
            {
                Logger.warn(String.Format("Could not create database: [{0}]", ex.Message));
            }
        }

        private void create_database_user(string name, string user, string password)
        {
            Logger.info(String.Format("Creating credentials: {0}/{1} for database {2}", user, password, name));
            

            using (TransactionScope ts = new TransactionScope())
            {
                SqlCommand cmdCreateLogin = new SqlCommand(String.Format(@"CREATE LOGIN {0} WITH PASSWORD = '{1}'", user, password), connection);
                cmdCreateLogin.ExecuteNonQuery();
                
                SqlConnection dbConnection = new SqlConnection(ConnectionString);
                dbConnection.Open();
                dbConnection.ChangeDatabase(name);

                SqlCommand cmdCreateUser = new SqlCommand(String.Format("CREATE USER {0} FOR LOGIN {0}", user), dbConnection);
                cmdCreateUser.ExecuteNonQuery();

                SqlCommand cmdAddRoleMember = new SqlCommand("sp_addrolemember", dbConnection);
                cmdAddRoleMember.CommandType = CommandType.StoredProcedure;
                cmdAddRoleMember.Parameters.Add("@rolename", SqlDbType.NVarChar).Value = "db_owner";
                cmdAddRoleMember.Parameters.Add("@membername", SqlDbType.NVarChar).Value = user;
                cmdAddRoleMember.ExecuteNonQuery();

                dbConnection.Close();
                ts.Complete();
            }
        }

        private void delete_database(ProvisionedService provisioned_service)
        {
            string name = provisioned_service.Name;
            string user = provisioned_service.User;

            try
            {
                delete_database_user(user);
                Logger.info(String.Format("Deleting database: {0}", name));

                using (TransactionScope ts = new TransactionScope())
                {
                    new SqlCommand(String.Format("ALTER DATABASE {0} SET OFFLINE WITH ROLLBACK IMMEDIATE", name), connection).ExecuteNonQuery();
                    new SqlCommand(String.Format("DROP DATABASE {0}", name), connection).ExecuteNonQuery();
                    ts.Complete();
                }
            }
            catch (Exception ex)
            {
                Logger.fatal(String.Format("Could not delete database: [{0}]", ex.Message));
            }
        }

        private void delete_database_user(string user)
        {
            Logger.info(String.Format("Delete user {0}", user));
            try
            {
                kill_user_session(user);

                using (TransactionScope ts = new TransactionScope())
                {
                    //new SqlCommand(String.Format(@"DROP USER {0}", user), connection).ExecuteNonQuery();
                    new SqlCommand(String.Format(@"DROP LOGIN {0}", user), connection).ExecuteNonQuery();

                    ts.Complete();
                }
            }
            catch (Exception ex)
            {
                Logger.fatal(String.Format("Could not delete user '{0}': [{1}]", user, ex.Message));
            }
        }

        // end active sesions for USER, to be able to drop the table
        private void kill_user_session(string user)
        {
            using (TransactionScope ts = new TransactionScope())
            {
                SqlDataReader sessionsToKill = new SqlCommand(String.Format("SELECT session_id FROM sys.dm_exec_sessions WHERE login_name = '{0}'", user), connection).ExecuteReader();

                while (sessionsToKill.Read())
                {
                    int sessionId = sessionsToKill.GetInt32(0);
                    new SqlCommand(String.Format("KILL {0}", sessionsToKill), connection).ExecuteNonQuery();
                }

                sessionsToKill.Close();
                ts.Complete();
            }
        }

        private void kill_database_session(string database)
        {
            //todo: vladi: Replace with code for odbc object for SQL Server
        }

        // restore a given instance using backup file.
        protected override bool restore(string name, string backup_path)
        {
            //todo: vladi: Replace with code for odbc object for SQL Server
            return false;
        }

        // Disable all credentials and kill user sessions
        protected override bool disable_instance(ServiceCredentials prov_cred, ServiceCredentials binding_creds)
        {
            //todo: vladi: Replace with code for odbc object for SQL Server
            return false;
        }

        // Dump db content into given path
        protected override bool dump_instance(ServiceCredentials prov_cred, ServiceCredentials binding_creds, string dump_file_path)
        {
            //todo: vladi: Replace with code for odbc object for SQL Server
            return false;
        }

        // Provision and import dump files
        // Refer to #dump_instance
        protected override bool import_instance(ServiceCredentials prov_cred, ServiceCredentials binding_creds, string dump_file_path, ProvisionedServicePlanType plan)
        {
            //todo: vladi: Replace with code for odbc object for SQL Server
            return false;
        }

        // Re-bind credentials
        // Refer to #disable_instance
        protected override bool enable_instance(ref ServiceCredentials prov_cred, ref Dictionary<string, object> binding_creds_hash)
        {
            //todo: vladi: Replace with code for odbc object for SQL Server
            return false;
        }

        protected override Dictionary<string, object> varz_details()
        {
            Logger.debug("Generate varz.");
            Dictionary<string, object> varz = new Dictionary<string, object>();
            try
            {
                // how many queries served since startup
                varz["queries_since_startup"] = get_queries_status();
                // queries per second
                varz["queries_per_second"] = get_qps();
                // disk usage per instance
                object[] status = get_instance_status();
                varz["database_status"] = status;
                // node capacity
                varz["node_storage_capacity"] = node_capacity;
                varz["node_storage_used"] = node_capacity - available_storage;
                // how many long queries and long txs are killed.
                varz["long_queries_killed"] = long_queries_killed;
                varz["long_transactions_killed"] = long_tx_killed;
                // how many provision/binding operations since startup.
                varz["provision_served"] = provision_served;
                varz["binding_served"] = binding_served;
                return varz;
            }
            catch (Exception ex)
            {
                Logger.error(String.Format("Error during generate varz: {0}", ex.Message));
                return new Dictionary<string, object>();
            }
        }

        protected override Dictionary<string, string> healthz_details()
        {
            //todo: vladi: Replace with code for odbc object for SQL Server
            Dictionary<string, string> healthz = new Dictionary<string, string>()
            {
                {"self", "ok"}
            };

            return healthz;
        }

        private string get_instance_healthz(ServiceCredentials instance)
        {
            //todo: vladi: Replace with code for odbc object for SQL Server
            string res = "ok";
            return res;
        }

        private int get_queries_status()
        {
            //todo: vladi: Replace with code for odbc object for SQL Server
            return 0;
        }

        private double get_qps()
        {
            Logger.debug("Calculate queries per seconds.");
            int queries = get_queries_status();
            DateTime ts = DateTime.Now;

            double delta_t = (ts - qps_last_updated).TotalSeconds;

            double qps = (queries - @queries_served) / delta_t;
            queries_served = queries;
            qps_last_updated = ts;

            return qps;
        }

        private object[] get_instance_status()
        {
            //todo: vladi: Replace with code for odbc object for SQL Server
            return new object[0];
        }

        private ServiceCredentials gen_credential(string name, string user, string passwd)
        {
            ServiceCredentials response = new ServiceCredentials();

            response.Name = name;
            response.HostName = local_ip;
            response.HostName = local_ip;
            response.Port = mssql_config.Port;
            response.User = user;
            response.Username = user;
            response.Password = passwd;

            return response;
        }

        public string gzip_bin { get; set; }
    }
}
