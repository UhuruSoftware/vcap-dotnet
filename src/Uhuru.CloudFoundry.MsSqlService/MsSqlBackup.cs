// -----------------------------------------------------------------------
// <copyright file="MSSqlBackup.cs" company="Uhuru Software, Inc.">
// Copyright (c) 2011 Uhuru Software, Inc., All Rights Reserved
// </copyright>
// -----------------------------------------------------------------------

namespace Uhuru.CloudFoundry.MSSqlService
{
    using System;
    using System.Collections.ObjectModel;
    using System.Data.SqlClient;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Threading;
    using System.Transactions;
    using Uhuru.CloudFoundry.ServiceBase;
    using Uhuru.Utilities;

    /// <summary>
    /// Handles MSSql node backups
    /// </summary>
    public class MSSqlBackup : BackupBase, IDisposable
    {
        /// <summary>
        /// MSSql system databases
        /// </summary>
        private string[] systemDatabases = { "master", "model", "msdb" };

        /// <summary>
        /// Databases to be ignored
        /// </summary>
        private string[] ignoreDatabases = { "tempdb" };

        /// <summary>
        /// Indicates wheather current object is disposed
        /// </summary>
        private bool disposed = false;

        /// <summary>
        /// This is the SQL server connection used to do things on the server.
        /// </summary>
        private SqlConnection connection;

        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="MSSqlBackup"/> is tolerant.
        /// </summary>
        /// <value>
        ///   <c>true</c> if tolerant; otherwise, <c>false</c>.
        /// </value>
        public bool IsTolerant
        {
            get
            {
                return this.tolerant;
            }

            set
            {
                this.tolerant = value;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="MSSqlBackup"/> is shutting down.
        /// </summary>
        /// <value>
        ///   <c>true</c> if shutdown; otherwise, <c>false</c>.
        /// </value>
        public bool ShutdownJob
        {
            get
            {
                return this.shutdown;
            }

            set
            {
                this.shutdown = value;
            }
        }

        /// <summary>
        /// Gets the connection string used to connect to the SQL Server.
        /// </summary>
        private string ConnectionString
        {
            get
            {
                SqlConnectionStringBuilder builder = new SqlConnectionStringBuilder();
                builder.UserID = uhuruSection.Service.MSSql.User;
                builder.Password = uhuruSection.Service.MSSql.Password;
                builder.DataSource = string.Format(CultureInfo.InvariantCulture, "{0},{1}", uhuruSection.Service.MSSql.Host, uhuruSection.Service.MSSql.Port);
                builder.Pooling = false;
                builder.MultipleActiveResultSets = true;

                return builder.ConnectionString;
            }
        }

        /// <summary>
        /// Starts this instance.
        /// </summary>
        public override void Start()
        {
            this.connection = this.ConnectMSSql();

            TimerHelper.RecurringCall(
                15000,
                delegate
                {
                    this.KeepAliveMSSql();
                });
            base.Start();
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources
        /// </summary>
        /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (!this.disposed)
            {
                if (disposing)
                {
                    if (this.connection != null)
                    {
                        this.connection.Close();
                        this.connection.Dispose();
                    }
                }

                this.disposed = true;
            }
        }

        /// <summary>
        /// Subclasses have to implement this to backup the service
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1303:Do not pass literals as localized parameters", Justification = "Code is more readable")]
        protected override void BackupServices()
        {
            Collection<string> dbs = this.ReadSqlServerDatabases();
            DateTime currentTime = DateTime.UtcNow;

            int success = 0;
            int failed = 0;
            int ignored = 0;

            Logger.Info("Begin backup at {0}", currentTime.ToLocalTime().ToString(CultureInfo.InvariantCulture));
            foreach (string db in dbs)
            {
                int result = this.ExecuteBackupDb(db, currentTime);
                switch (result)
                {
                    case 0:
                        {
                            success += 1;
                            break;
                        }

                    case 1:
                        {
                            failed += 1;
                            break;
                        }

                    case -1:
                        {
                            ignored += 1;
                            break;
                        }

                    default: break;
                }

                if (this.shutdown)
                {
                    Environment.Exit(0);
                }
            }

            Logger.Info("Backup started at {0} completed. Success: {1}. Failed: {2}. Ignored: {3}.", currentTime.ToString(CultureInfo.InvariantCulture), success, failed, ignored);
            Environment.Exit(0);
        }

        /// <summary>
        /// Keep connection alive, and check db liveliness
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2100:Review SQL queries for security vulnerabilities", Justification = "Query is retrieved from resource file."),
        System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "Exception is properly logged, it should not bubble up here")]
        private void KeepAliveMSSql()
        {
            // present in both mysql and postgresql
            try
            {
                using (SqlCommand cmd = new SqlCommand(Strings.SqlNodeKeepAliveSQL, this.connection))
                {
                    cmd.ExecuteScalar();
                }
            }
            catch (Exception ex)
            {
                Logger.Warning(Strings.SqlNodeConnectionLostWarningMessage, ex.ToString());
                this.connection = this.ConnectMSSql();
            }
        }

        /// <summary>
        /// Executes the backup.
        /// </summary>
        /// <param name="databaseName">Name of the database.</param>
        /// <param name="currentTime">The current time.</param>
        /// <returns>Value indicating success status: 0 (success), -1 (ignored), 1 (failure)</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2100:Review SQL queries for security vulnerabilities", Justification = "Code is more readable"), 
        System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1303:Do not pass literals as localized parameters", Justification = "Code is more readable"), 
        System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "Exception is logged")]
        private int ExecuteBackupDb(string databaseName, DateTime currentTime)
        {
            string fullPath;

            try
            {
                if (this.ignoreDatabases.Contains(databaseName))
                {
                    return -1;
                }
                else if (this.systemDatabases.Contains(databaseName))
                {
                    fullPath = this.GetDumpPath(databaseName, 1, currentTime);
                }
                else
                {
                    fullPath = this.GetDumpPath(databaseName, 0, currentTime);
                }

                if (!Directory.Exists(fullPath))
                {
                    Directory.CreateDirectory(fullPath);
                }

                string backupFile = Path.Combine(fullPath, databaseName);
                string sqlCommand = string.Format(CultureInfo.InvariantCulture, "BACKUP DATABASE {0} TO DISK = '{1}'", databaseName, backupFile);

                DateTime t1 = DateTime.Now;
                using (TransactionScope ts = new TransactionScope())
                {
                    using (SqlCommand cmdBackup = new SqlCommand(sqlCommand, this.connection))
                    {
                        cmdBackup.ExecuteNonQuery();
                    }

                    ts.Complete();
                }

                ZipUtilities.GZipFiles(new string[] { backupFile }, Path.ChangeExtension(backupFile, "gz"));
                File.Delete(backupFile);

                DateTime t2 = DateTime.Now;
                Logger.Info("Backup for db {0} completed in {1} seconds", databaseName, (t2 - t1).Seconds);
                return 0;
            }
            catch (Exception ex)
            {
                Logger.Error("Error when backup db {0}: [{1}]", databaseName, ex.ToString());
                return 1;
            }
        }

        /// <summary>
        /// Reads the SQL server databases.
        /// </summary>
        /// <returns>Collection of database names</returns>
        private Collection<string> ReadSqlServerDatabases()
        {
            Collection<string> databases = new Collection<string>();
            using (SqlCommand cmdReadDatabases = new SqlCommand("sp_databases", this.connection))
            {
                cmdReadDatabases.CommandType = System.Data.CommandType.StoredProcedure;
                SqlDataReader reader = cmdReadDatabases.ExecuteReader();
                while (reader.Read())
                {
                    databases.Add(reader.GetString(0));
                }
            }

            return databases;
        }

        /// <summary>
        /// Connects to the MS SQL database.
        /// </summary>
        /// <returns>An open sql connection.</returns>
        private SqlConnection ConnectMSSql()
        {
            for (int i = 0; i < 5; i++)
            {
                this.connection = new SqlConnection(this.ConnectionString);

                try
                {
                    this.connection.Open();
                    return this.connection;
                }
                catch (InvalidOperationException)
                {
                }
                catch (SqlException)
                {
                }

                Thread.Sleep(5000);
            }

            Logger.Fatal(Strings.SqlNodeConnectionUnrecoverableFatalMessage);
            Environment.Exit(1);
            return null;
        }
    }
}
