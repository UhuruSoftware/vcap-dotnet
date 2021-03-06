﻿// -----------------------------------------------------------------------
// <copyright file="Node.cs" company="Uhuru Software, Inc.">
// Copyright (c) 2011 Uhuru Software, Inc., All Rights Reserved
// </copyright>
// -----------------------------------------------------------------------

namespace Uhuru.CloudFoundry.MSSqlService
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Data;
    using System.Data.SqlClient;
    using System.Diagnostics;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Text.RegularExpressions;
    using System.Threading;
    using System.Transactions;
    using Uhuru.CloudFoundry.ServiceBase;
    using Uhuru.Configuration;
    using Uhuru.Utilities;
    using Uhuru.Utilities.Json;

    /// <summary>
    /// This class is the MS SQL Server Node that brings this RDBMS as a service to Cloud Foundry.
    /// </summary>
    public partial class Node : NodeBase
    {
        /// <summary>
        /// Interval at which the database connection is used so it doesn't die.
        /// </summary>
        private const int KeepAliveInterval = 15000;

        /// <summary>
        /// Interval at which to verify storage quotas.
        /// </summary>
        private const int StorageQuotaInterval = 1000;

        /// <summary>
        /// COnfiguration options for the SQL Server.
        /// </summary>
        private MSSqlOptions mssqlConfig = new MSSqlOptions();

        /// <summary>
        /// The maximum database size, in bytes.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields", Justification = "It will be used.")]
        private long maxDbSize;

        /// <summary>
        /// The maximum duration for a query.
        /// </summary>
        private int maxLongQuery;

        /// <summary>
        /// The maximum duration for a transaction.
        /// </summary>
        private int maxLongTx;

        /// <summary>
        /// The maximum connections per user.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields", Justification = "It will be used.")]
        private int maxUserConnections;

        /// <summary>
        /// This is the SQL server connection used to do things on the server.
        /// </summary>
        private SqlConnection connection;

        /// <summary>
        /// The base directory for this service.
        /// </summary>
        private string baseDir;

        /// <summary>
        /// Number of queries served by the node.
        /// </summary>
        private int queriesServed;

        /// <summary>
        /// Indicates when was the last queries/second metric snapshot taken.
        /// </summary>
        private DateTime qpsLastUpdated;

        /// <summary>
        /// Number of long queries that were killed by the node.
        /// </summary>
        private int longQueriesKilled;

        /// <summary>
        /// Number of long transactions that were killed by the node.
        /// </summary>
        private int longTxKilled;

        /// <summary>
        /// Number of database provision requests served by the node.
        /// </summary>
        private int provisionServed;

        /// <summary>
        /// Number of binding requests served by the node.
        /// </summary>
        private int bindingServed;

        /// <summary>
        /// The SQL script that creates the service database
        /// </summary>
        private string createDBScript;

        /// <summary>
        /// Gets any service-specific announcement details.
        /// </summary>
        protected override Announcement AnnouncementDetails
        {
            get
            {
                Announcement a = new Announcement();
                a.AvailableCapacity = this.capacity;
                a.CapacityUnit = this.CapacityUnit();
                a.SupportedVersions = this.supportedVersions.ToArray();
                return a;
            }
        }

        /// <summary>
        /// Gets the connection string used to connect to the SQL Server.
        /// </summary>
        private string ConnectionString
        {
            get
            {
                return string.Format(
                    CultureInfo.InvariantCulture,
                    Strings.SqlNodeConnectionString,
                    this.mssqlConfig.Host,
                    this.mssqlConfig.Port,
                    this.mssqlConfig.User,
                    this.mssqlConfig.Password);
            }
        }

        /// <summary>
        /// Starts the node.
        /// </summary>
        /// <param name="options">The configuration options for the node.</param>
        public override void Start(ServiceElement options)
        {
            if (options == null)
            {
                throw new ArgumentNullException("options");
            }

            this.mssqlConfig.Host = options.MSSql.Host;
            this.mssqlConfig.User = options.MSSql.User;
            this.mssqlConfig.Port = options.MSSql.Port;
            this.mssqlConfig.Password = options.MSSql.Password;
            this.mssqlConfig.LogicalStorageUnits = options.MSSql.LogicalStorageUnits;

            this.mssqlConfig.InitialDataSize = options.MSSql.InitialDataSize;
            this.mssqlConfig.InitialLogSize = options.MSSql.InitialLogSize;

            this.mssqlConfig.MaxDataSize = options.MSSql.MaxDataSize;
            this.mssqlConfig.MaxLogSize = options.MSSql.MaxLogSize;

            this.mssqlConfig.DataFileGrowth = options.MSSql.DataFileGrowth;
            this.mssqlConfig.LogFileGrowth = options.MSSql.LogFileGrowth;

            this.maxDbSize = options.MSSql.MaxDBSize * 1024 * 1024;
            this.maxLongQuery = options.MSSql.MaxLengthyQuery;
            this.maxLongTx = options.MSSql.MaxLengthTX;
            this.maxUserConnections = options.MSSql.MaxUserConnections;

            this.connection = this.ConnectMSSql();

            TimerHelper.RecurringCall(
                KeepAliveInterval,
                delegate
                {
                    this.KeepAliveMSSql();
                });

            if (this.maxLongQuery > 0)
            {
                TimerHelper.RecurringCall(
                    this.maxLongQuery / 2,
                    delegate
                    {
                        this.KillLongTransactions();
                    });
            }

            if (this.maxLongTx > 0)
            {
                TimerHelper.RecurringCall(
                    this.maxLongTx / 2,
                    delegate
                    {
                        this.KillLongQueries();
                    });
            }
            else
            {
                Logger.Info(Strings.LongTXKillerDisabledInfoMessage);
            }

            TimerHelper.RecurringCall(
                StorageQuotaInterval,
                delegate
                {
                    this.EnforceStorageQuota();
                });

            this.baseDir = options.BaseDir;
            if (!string.IsNullOrEmpty(this.baseDir))
            {
                Directory.CreateDirectory(this.baseDir);
            }

            ProvisionedService.Initialize(options.LocalDB);

            this.CheckDBConsistency();

            this.capacity = -this.CapacityUnit() * ProvisionedService.GetInstances().Count();

            this.queriesServed = 0;
            this.qpsLastUpdated = DateTime.Now;

            // initialize qps counter
            this.GetQPS();
            this.longQueriesKilled = 0;
            this.longTxKilled = 0;
            this.provisionServed = 0;
            this.bindingServed = 0;

            base.Start(options);
        }

        /// <summary>
        /// Restore a given instance using backup file.
        /// </summary>
        /// <param name="instanceId">The instance id.</param>
        /// <param name="backupPath">The backup path.</param>
        /// <returns>
        /// A bool indicating whether the request was successful.
        /// </returns>
        protected override bool Restore(string instanceId, string backupPath)
        {
            // todo: vladi: implement this
            return false;
        }

        /// <summary>
        /// This methos disables all credentials and kills user sessions.
        /// </summary>
        /// <param name="provisionedCredential">The provisioned credentials.</param>
        /// <param name="bindingCredentials">The binding credentials.</param>
        /// <returns>
        /// A bool indicating whether the request was successful.
        /// </returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "Error is being logged")]
        protected override bool DisableInstance(ServiceCredentials provisionedCredential, Collection<ServiceCredentials> bindingCredentials)
        {
            if (provisionedCredential == null)
            {
                throw new ArgumentNullException("provisionedCredential");
            }

            if (bindingCredentials == null)
            {
                throw new ArgumentNullException("bindingCredentials");
            }

            Logger.Debug(Strings.SqlNodeDisableInstanceStartMessage, provisionedCredential.Name);

            bindingCredentials.Add(provisionedCredential);

            try
            {
                foreach (ServiceCredentials credential in bindingCredentials)
                {
                    this.Unbind(credential);
                }
            }
            catch (Exception ex)
            {
                Logger.Warning(Strings.SqlNodeDisableInstanceError, provisionedCredential.Name, ex.ToString());
                return false;
            }

            return true;
        }

        /// <summary>
        /// Dumps database content into a given path.
        /// </summary>
        /// <param name="provisionedCredential">The provisioned credential.</param>
        /// <param name="bindingCredentials">The binding credentials.</param>
        /// <param name="filePath">The file path where to dump the service.</param>
        /// <returns>
        /// A bool indicating whether the request was successful.
        /// </returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2100:Review SQL queries for security vulnerabilities", Justification = "Query is retrieved from resource file."), 
        System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "Error is being logged")]
        protected override bool DumpInstance(ServiceCredentials provisionedCredential, Collection<ServiceCredentials> bindingCredentials, string filePath)
        {
            if (provisionedCredential == null)
            {
                throw new ArgumentNullException("provisionedCredential");
            }

            if (bindingCredentials == null)
            {
                throw new ArgumentNullException("bindingCredentials");
            }

            string dumpFile = Path.Combine(filePath, provisionedCredential.Name);

            Logger.Info(Strings.SqlNodeDumpDatabaseStartMessage, provisionedCredential.Name, dumpFile);

            try
            {
                using (TransactionScope ts = new TransactionScope())
                {
                    using (SqlCommand dumpDatabaseCommand = new SqlCommand(string.Format(CultureInfo.InvariantCulture, Strings.SqlNodeDumpDatabaseSQL, provisionedCredential.Name, dumpFile), this.connection))
                    {
                        dumpDatabaseCommand.ExecuteNonQuery();
                    }

                    ts.Complete();
                }

                Logger.Info(Strings.SqlNodeDumpDatabaseEndMessage, provisionedCredential.Name);
            }
            catch (Exception ex)
            {
                Logger.Warning(Strings.SqlNodeDumpDatabaseError, provisionedCredential.Name, ex.ToString());
                return false;
            }

            return true;
        }

        /// <summary>
        /// Imports an instance from a path.
        /// </summary>
        /// <param name="provisionedCredential">The provisioned credential.</param>
        /// <param name="bindingCredentialsHash">The binding credentials.</param>
        /// <param name="filePath">The file path from which to import the service.</param>
        /// <param name="planRequest">The payment plan.</param>
        /// <returns>
        /// A bool indicating whether the request was successful.
        /// </returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2100:Review SQL queries for security vulnerabilities", Justification = "Query is retrieved from resource file."),
        System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "Error is being logged")]
        protected override bool ImportInstance(ServiceCredentials provisionedCredential, Dictionary<string, object> bindingCredentialsHash, string filePath, string planRequest)
        {
            if (provisionedCredential == null)
            {
                throw new ArgumentNullException("provisionedCredential");
            }

            if (bindingCredentialsHash == null)
            {
                throw new ArgumentNullException("bindingCredentialsHash");
            }

            string dumpFile = Path.Combine(filePath, provisionedCredential.Name);
            Logger.Info(Strings.SqlNodeImportDatabaseStartMessage, provisionedCredential.Name, dumpFile);

            try
            {
                ServiceCredentials credentials = this.Provision(planRequest, provisionedCredential, this.defaultVersion);
                this.ImportDatabase(credentials, dumpFile);
                Logger.Info(Strings.SqlNodeImportDatabaseEndMessage, provisionedCredential.Name);
            }
            catch (Exception ex)
            {
                Logger.Error(Strings.SqlNodeImportDatabaseError, provisionedCredential.Name, ex.ToString());
                return false;
            }

            return true;
        }

        /// <summary>
        /// Re-enables the instance, re-binds credentials.
        /// </summary>
        /// <param name="provisionedCredential">The provisioned credential.</param>
        /// <param name="bindingCredentialsHash">The binding credentials hash.</param>
        /// <returns>
        /// A bool indicating whether the request was successful.
        /// </returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "Error is being logged")]
        protected override bool EnableInstance(ref ServiceCredentials provisionedCredential, ref Dictionary<string, object> bindingCredentialsHash)
        {
            if (provisionedCredential == null)
            {
                throw new ArgumentNullException("provisionedCredential");
            }

            if (bindingCredentialsHash == null)
            {
                throw new ArgumentNullException("bindingCredentialsHash");
            }

            Logger.Debug(Strings.SqlNodeEnableInstanceStartMessage, provisionedCredential.Name);

            try
            {
                provisionedCredential = Bind(provisionedCredential.Name, null, provisionedCredential);
                foreach (KeyValuePair<string, object> pair in bindingCredentialsHash)
                {
                    Handle handle = (Handle)pair.Value;
                    ServiceCredentials cred = new ServiceCredentials();
                    cred.FromJsonIntermediateObject(handle.Credentials);
                    Dictionary<string, object> bindingOptions = handle.Credentials.BindOptions;
                    Bind(cred.Name, bindingOptions, cred);
                }
            }
            catch (Exception ex)
            {
                Logger.Warning(Strings.SqlNodeEnableInstanceError, provisionedCredential.Name, ex.ToString());
                return false;
            }

            return true;
        }

        /// <summary>
        /// Gets varz details about the SQL Server Node.
        /// </summary>
        /// <returns>
        /// A dictionary containing varz details.
        /// </returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "Exception is properly logged, it should not bubble up here")]
        protected override Dictionary<string, object> VarzDetails()
        {
            Logger.Debug(Strings.SqlNodeGenerateVarzDebugMessage);
            Dictionary<string, object> varz = new Dictionary<string, object>();
            try
            {
                // how many queries served since startup
                varz["queries_since_startup"] = this.GetQueriesStatus();

                // queries per second
                varz["queries_per_second"] = this.GetQPS();

                // disk usage per instance
                object[] status = this.GetInstanceStatus();
                varz["database_status"] = status;

                // how many long queries and long txs are killed.
                varz["long_queries_killed"] = this.longQueriesKilled;
                varz["long_transactions_killed"] = this.longTxKilled;

                // how many provision/binding operations since startup.
                varz["provision_served"] = this.provisionServed;
                varz["binding_served"] = this.bindingServed;
                return varz;
            }
            catch (Exception ex)
            {
                Logger.Error(Strings.SqlNodeGenerateVarzErrorMessage, ex.ToString());
                return new Dictionary<string, object>();
            }
        }

        /// <summary>
        /// Gets healthz details about the SQL Server Node.
        /// </summary>
        /// <returns>
        /// A dictionary containing healthz details.
        /// </returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "Exception is properly logged, it should not bubble up here")]
        protected Dictionary<string, string> HealthzDetails()
        {
            // TODO: stefi: put this into varz
            Dictionary<string, string> healthz = new Dictionary<string, string>()
            {
                { "self", "ok" }
            };

            try
            {
                using (SqlCommand readDatabases = new SqlCommand("SELECT name FROM master..sysdatabases", this.connection))
                {
                    using (SqlDataReader reader = readDatabases.ExecuteReader())
                    {
                        reader.Read();
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Error(Strings.ErrorGettingDBListErrorMessage, ex.ToString());
                healthz["self"] = "fail";
                return healthz;
            }

            try
            {
                foreach (ProvisionedService instance in ProvisionedService.GetInstances())
                {
                    healthz[instance.Name] = this.GetInstanceHealthz(instance);
                }
            }
            catch (Exception ex)
            {
                Logger.Error(Strings.ErrorGettingInstanceListErrorMessage, ex.ToString());
                healthz["self"] = "fail";
            }

            return healthz;
        }

        /// <summary>
        /// Gets the service name.
        /// </summary>
        /// <returns>The service name for the MS SQL Node is 'MssqlaaS'</returns>
        protected override string ServiceName()
        {
            return "MssqlaaS";
        }

        /// <summary>
        /// Provisions an MS Sql Server database.
        /// </summary>
        /// <param name="planRequest">The payment plan for the service.</param>
        /// <returns>
        /// Credentials for the provisioned service.
        /// </returns>
        protected override ServiceCredentials Provision(string planRequest)
        {
            return Provision(planRequest, null, this.defaultVersion);
        }

        /// <summary>
        /// Provisions an MS Sql Server database.
        /// </summary>
        /// <param name="planRequest">The payment plan for the service.</param>
        /// <param name="credentials">Existing credentials for the service.</param>
        /// <param name="version">The service version.</param>
        /// <returns>
        /// Credentials for the provisioned service.
        /// </returns>
        protected override ServiceCredentials Provision(string planRequest, ServiceCredentials credentials, string version)
        {
            if (planRequest != this.plan)
            {
                throw new MSSqlErrorException(MSSqlErrorException.MSSqlInvalidPlan);
            }

            if (!this.supportedVersions.Contains(version))
            {
                throw new MSSqlErrorException(ServiceException.UnsupportedVersion);
            }

            //// todo: chek for plan
            ProvisionedService provisioned_service = new ProvisionedService();
            if (credentials == null)
            {
                throw new ArgumentNullException("credentials");
            }

            try
            {
                string name = credentials.Name;
                string user = credentials.User;
                string password = credentials.Password;
                provisioned_service.Name = name;
                provisioned_service.User = user;
                provisioned_service.Password = password;
                provisioned_service.Plan = planRequest;

                this.CreateDatabase(provisioned_service);

                if (!ProvisionedService.Save())
                {
                    Logger.Error(Strings.SqlNodeCannotSaveProvisionedServicesErrorMessage, provisioned_service.SerializeToJson());
                    throw new MSSqlErrorException(MSSqlErrorException.MSSqlLocalDBError);
                }

                ServiceCredentials response = this.GenerateCredential(provisioned_service.Name, provisioned_service.User, provisioned_service.Password);
                this.provisionServed += 1;
                return response;
            }
            catch (Exception)
            {
                this.DeleteDatabase(provisioned_service);
                throw;
            }
        }

        /// <summary>
        /// Generates credentials for a new service instance that has to be provisioned.
        /// </summary>
        /// <returns>
        /// Service credentials - name, user and password.
        /// </returns>
        protected override ServiceCredentials GenerateCredentials()
        {
            return this.GenerateCredential(
                "D4Ta" + Guid.NewGuid().ToString("N"),
                "US3r" + Credentials.GenerateCredential(),
                "P4Ss" + Credentials.GenerateCredential());
        }

        /// <summary>
        /// Unprovisions a SQL Server database.
        /// </summary>
        /// <param name="name">The name of the service to unprovision.</param>
        /// <param name="bindings">Array of bindings for the service that have to be unprovisioned.</param>
        /// <returns>
        /// A boolean specifying whether the unprovision request was successful.
        /// </returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "Exception is properly logged, it should not bubble up here")]
        protected override bool Unprovision(string name, ServiceCredentials[] bindings)
        {
            if (string.IsNullOrEmpty(name))
            {
                return false;
            }

            ProvisionedService provisioned_service = ProvisionedService.GetService(name);

            if (provisioned_service == null)
            {
                throw new MSSqlErrorException(MSSqlErrorException.MSSqlConfigNotFound, name);
            }

            // TODO: validate that database files are not lingering
            // Delete all bindings, ignore not_found error since we are unprovision
            try
            {
                if (bindings != null)
                {
                    foreach (ServiceCredentials credential in bindings)
                    {
                        this.Unbind(credential);
                    }
                }
            }
            catch (Exception)
            {
                // ignore
            }

            this.DeleteDatabase(provisioned_service);

            if (!provisioned_service.Destroy())
            {
                Logger.Error(Strings.SqlNodeDeleteServiceErrorMessage, provisioned_service.Name);
                throw new MSSqlErrorException(MSSqlErrorException.MSSqlLocalDBError);
            }

            Logger.Debug(Strings.SqlNodeUnprovisionSuccessDebugMessage, name);
            return true;
        }

        /// <summary>
        /// Binds a SQL Server database to an app.
        /// </summary>
        /// <param name="name">The name of the service.</param>
        /// <param name="bindOptions">Binding options.</param>
        /// <returns>
        /// A new set of credentials used for binding.
        /// </returns>
        protected override ServiceCredentials Bind(string name, Dictionary<string, object> bindOptions)
        {
            return Bind(name, bindOptions, null);
        }

        /// <summary>
        /// Binds a SQL Server database to an app.
        /// </summary>
        /// <param name="name">The name of the service.</param>
        /// <param name="bindOptions">Binding options.</param>
        /// <param name="credentials">Already existing credentials.</param>
        /// <returns>
        /// A new set of credentials used for binding.
        /// </returns>
        protected override ServiceCredentials Bind(string name, Dictionary<string, object> bindOptions, ServiceCredentials credentials)
        {
            Logger.Debug(Strings.SqlNodeBindServiceDebugMessage, name, JsonConvertibleObject.SerializeToJson(bindOptions));
            Dictionary<string, object> binding = null;
            try
            {
                ProvisionedService service = ProvisionedService.GetService(name);
                if (service == null)
                {
                    throw new MSSqlErrorException(MSSqlErrorException.MSSqlConfigNotFound, name);
                }

                // create new credential for binding
                binding = new Dictionary<string, object>();

                if (credentials != null)
                {
                    binding["user"] = credentials.User;
                    binding["password"] = credentials.Password;
                }
                else
                {
                    binding["user"] = "US3R" + Credentials.GenerateCredential();
                    binding["password"] = "P4SS" + Credentials.GenerateCredential();
                }

                binding["bind_opts"] = bindOptions;

                this.CreateDatabaseUser(name, binding["user"] as string, binding["password"] as string);
                ServiceCredentials response = this.GenerateCredential(name, binding["user"] as string, binding["password"] as string);

                Logger.Debug(Strings.SqlNodeBindResponseDebugMessage, response.SerializeToJson());
                this.bindingServed += 1;
                return response;
            }
            catch (Exception)
            {
                if (binding != null)
                {
                    this.DeleteDatabaseUser(binding["user"] as string, name);
                }

                throw;
            }
        }

        /// <summary>
        /// Unbinds a SQL Server database from an app.
        /// </summary>
        /// <param name="credentials">The credentials that have to be unprovisioned.</param>
        /// <returns>
        /// A bool indicating whether the unbind request was successful.
        /// </returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2100:Review SQL queries for security vulnerabilities", Justification = "Query is retrieved from resource file.")]
        protected override bool Unbind(ServiceCredentials credentials)
        {
            if (credentials == null)
            {
                return false;
            }

            Logger.Debug(Strings.SqlNodeUnbindServiceDebugMessage, credentials.SerializeToJson());

            string user = credentials.User;
            string databaseName = credentials.Name;

            using (SqlConnection databaseConnection = new SqlConnection(this.ConnectionString))
            {
                databaseConnection.Open();
                databaseConnection.ChangeDatabase(databaseName);

                using (SqlCommand cmdUserExists = new SqlCommand(string.Format(CultureInfo.InvariantCulture, "select count(*) from sys.sysusers where name=N'{0}'", user), databaseConnection))
                {
                    int userCount = (int)cmdUserExists.ExecuteScalar();
                    if (userCount != 1)
                    {
                        throw new MSSqlErrorException(MSSqlErrorException.MSSqlCredentialsNotFound, user);
                    }
                }
            }

            this.DeleteDatabaseUser(user, databaseName);
            return true;
        }

        /// <summary>
        /// Updates service bindings.
        /// </summary>
        /// <param name="provisionedCredential">The provisioned credentials.</param>
        /// <param name="bindingCredentials">The binding credentials.</param>
        /// <returns>
        /// Updated service credentials
        /// </returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "Error logged")]
        protected override object[] UpdateInstance(ServiceCredentials provisionedCredential, Dictionary<string, object> bindingCredentials)
        {
            if (provisionedCredential == null)
            {
                throw new ArgumentNullException("provisionedCredential");
            }

            if (bindingCredentials == null)
            {
                throw new ArgumentNullException("bindingCredentials");
            }

            object[] response = new object[2];

            string name = provisionedCredential.Name;
            try
            {
                provisionedCredential = Bind(name, null, provisionedCredential);
                Dictionary<string, object> bindingCredentialsResponse = new Dictionary<string, object>();

                foreach (KeyValuePair<string, object> pair in bindingCredentials)
                {
                    ServiceCredentials cred = (ServiceCredentials)pair.Value;
                    ServiceCredentials bindingCred = Bind(cred.Name, cred.BindOptions, cred);
                    bindingCredentialsResponse[pair.Key] = bindingCred;
                }

                response[0] = provisionedCredential;
                response[1] = bindingCredentialsResponse;

                return response;
            }
            catch (Exception ex)
            {
                Logger.Warning(ex.ToString());
                return new object[0];
            }
        }

        /// <summary>
        /// Checks local database consistency.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Method is not yet implemented")]
        private void CheckDBConsistency()
        {
            // method present in mysql and postgresql
            // todo: vladi: this should be replaced with ms sql server code
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
            Shutdown();
            Process.GetCurrentProcess().Kill();
            return null;
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
        /// Kills long transactions.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2100:ReviewSqlQueriesForSecurityVulnerabilities", Justification = "Not user input")]
        private void KillLongTransactions()
        {
            try
            {
                if (this.connection.State != ConnectionState.Open)
                {
                    this.connection = this.ConnectMSSql();
                }

                Stream resourceStream = Assembly.GetExecutingAssembly().GetManifestResourceStream("Uhuru.CloudFoundry.MSSqlService.LongRunningTransactions.sql");

                if (resourceStream == null)
                {
                    throw new FileNotFoundException(Strings.SqlNodeGetLongRunningQueriesScriptNotFound);
                }

                StreamReader sr = new StreamReader(resourceStream);

                string sqlLongRunningTransactions = sr.ReadToEnd();

                using (SqlCommand cmd = new SqlCommand(string.Format(CultureInfo.InvariantCulture, sqlLongRunningTransactions, this.maxLongTx), this.connection))
                {
                    SqlDataReader longTransactions = cmd.ExecuteReader(CommandBehavior.SingleResult);

                    while (longTransactions.Read())
                    {
                        using (SqlCommand killCmd = new SqlCommand(string.Format(CultureInfo.InvariantCulture, Strings.SqlNodeKillSessionSQL, longTransactions["session_id"]), this.connection))
                        {
                            try
                            {
                                killCmd.ExecuteNonQuery();
                                this.longTxKilled++;
                            }
                            catch (SqlException)
                            {
                                continue;
                            }
                        }
                    }

                    longTransactions.Close();
                }
            }
            catch (SqlException sex)
            {
                Logger.Warning(sex.Message);
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// Kills long queries.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Method is not yet implemented")]
        private void KillLongQueries()
        {
            // present in both mysql and postgresql
            // todo: vladi: implement this
        }

        /// <summary>
        /// Creates the database.
        /// </summary>
        /// <param name="provisionedService">The provisioned service for which a database has to be created.</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2100:Review SQL queries for security vulnerabilities", Justification = "Query is retrieved from resource file."),
        System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "Exception is properly logged, it should not bubble up here")]
        private void CreateDatabase(ProvisionedService provisionedService)
        {
            string databaseName = provisionedService.Name;
            string databasePassword = provisionedService.Password;
            string databaseUser = provisionedService.User;

            try
            {
                DateTime start = DateTime.Now;
                Logger.Debug(Strings.SqlNodeCreateDatabaseDebugMessage, provisionedService.SerializeToJson());

                this.createDBScript = this.ExtractSqlScriptFromTemplate("Uhuru.CloudFoundry.MSSqlService.CreateServiceDatabaseTemplate.sql", databaseName);

                // split script on GO command
                IEnumerable<string> commandStrings = Regex.Split(this.createDBScript, "^\\s*GO\\s*$", RegexOptions.Multiline);

                using (TransactionScope ts = new TransactionScope())
                {
                    foreach (string commandString in commandStrings)
                    {
                        if (!string.IsNullOrEmpty(commandString.Trim()) && !commandString.Contains("[master]"))
                        {
                            using (SqlCommand cmd = new SqlCommand(commandString, this.connection))
                            {
                                if (commandString.Contains("CREATE DATABASE"))
                                {
                                    cmd.CommandTimeout = 0;
                                }

                                cmd.ExecuteNonQuery();
                            }
                        }
                    }
                }

                this.CreateDatabaseUser(databaseName, databaseUser, databasePassword);

                Logger.Debug(Strings.SqlNodeDoneCreatingDBDebugMessage, provisionedService.SerializeToJson(), (start - DateTime.Now).TotalSeconds);
            }
            catch (Exception ex)
            {
                Logger.Warning(Strings.SqlNodeCouldNotCreateDBWarningMessage, ex.ToString());
                throw;
            }
        }

        /// <summary>
        /// Imports the database.
        /// </summary>
        /// <param name="credentials">The service credentials.</param>
        /// <param name="backupLocation">The backup location.</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2100:Review SQL queries for security vulnerabilities", Justification = "Query is retrieved from resource file.")]
        private void ImportDatabase(ServiceCredentials credentials, string backupLocation)
        {
            string importDBScript = this.ExtractSqlScriptFromTemplate("Uhuru.CloudFoundry.MSSqlService.ImportServiceDatabaseTemplate.sql", credentials.Name);

            importDBScript = importDBScript.Replace("<NfsLocation>", backupLocation);

            // split script on GO command
            IEnumerable<string> commandStrings = Regex.Split(importDBScript, "^\\s*GO\\s*$", RegexOptions.Multiline);

            using (TransactionScope ts = new TransactionScope())
            {
                foreach (string commandString in commandStrings)
                {
                    if (!string.IsNullOrEmpty(commandString.Trim()) && !commandString.Contains("[master]"))
                    {
                        using (SqlCommand cmd = new SqlCommand(commandString, this.connection))
                        {
                            cmd.ExecuteNonQuery();
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Loads the sql template and replaces the tags with user supplied values
        /// </summary>
        /// <param name="resourceFile">The resource file.</param>
        /// <param name="databaseName">The database name</param>
        /// <returns>
        /// An SQL script
        /// </returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204", Justification = "lol")]
        private string ExtractSqlScriptFromTemplate(string resourceFile, string databaseName)
        {
            Stream templateStream = Assembly.GetExecutingAssembly().GetManifestResourceStream(resourceFile);

            if (templateStream == null)
            {
                throw new FileNotFoundException("Resource error: Cannot find DB creation sql script template 'CreateServiceDatabaseTemplate.sql'");
            }

            StreamReader sr = new StreamReader(templateStream);

            string createDBSqlScript = sr.ReadToEnd();

            Regex regex = new Regex(@"<DatabaseName>");
            createDBSqlScript = regex.Replace(createDBSqlScript, databaseName);

            regex = new Regex(@"<RootDataPath>");
            createDBSqlScript = regex.Replace(createDBSqlScript, string.IsNullOrEmpty(this.mssqlConfig.LogicalStorageUnits) ? "C:" : this.mssqlConfig.LogicalStorageUnits);

            regex = new Regex(@"<InitialDataSize>");
            createDBSqlScript = regex.Replace(createDBSqlScript, this.mssqlConfig.InitialDataSize);

            regex = new Regex(@"<InitialLogSize>");
            createDBSqlScript = regex.Replace(createDBSqlScript, this.mssqlConfig.InitialLogSize);

            regex = new Regex(@"<MaxDataSize>");
            createDBSqlScript = regex.Replace(createDBSqlScript, this.mssqlConfig.MaxDataSize);

            regex = new Regex(@"<MaxLogSize>");
            createDBSqlScript = regex.Replace(createDBSqlScript, this.mssqlConfig.MaxLogSize);

            regex = new Regex(@"<DataFileGrowth>");
            createDBSqlScript = regex.Replace(createDBSqlScript, this.mssqlConfig.DataFileGrowth);

            regex = new Regex(@"<LogFileGrowth>");
            createDBSqlScript = regex.Replace(createDBSqlScript, this.mssqlConfig.LogFileGrowth);

            return createDBSqlScript;
        }

        /// <summary>
        /// Creates a database user.
        /// </summary>
        /// <param name="name">The name of the database.</param>
        /// <param name="user">The user.</param>
        /// <param name="password">The password.</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2100:Review SQL queries for security vulnerabilities", Justification = "Query is retrieved from resource file.")]
        private void CreateDatabaseUser(string name, string user, string password)
        {
            Logger.Info(Strings.SqlNodeCreatingCredentialsInfoMessage, user, password, name);

            using (TransactionScope ts = new TransactionScope())
            {
                using (SqlCommand cmdCreateLogin = new SqlCommand(string.Format(CultureInfo.InvariantCulture, Strings.SqlNodeCreateLoginSQL, user, password), this.connection))
                {
                    cmdCreateLogin.ExecuteNonQuery();
                }

                using (SqlConnection databaseConnection = new SqlConnection(this.ConnectionString))
                {
                    databaseConnection.Open();
                    databaseConnection.ChangeDatabase(name);

                    using (SqlCommand cmdCreateUser = new SqlCommand(string.Format(CultureInfo.InvariantCulture, Strings.SqlNodeCreateUserSQL, user), databaseConnection))
                    {
                        cmdCreateUser.ExecuteNonQuery();
                    }

                    using (SqlCommand cmdAddRoleMember = new SqlCommand("sp_addrolemember", databaseConnection))
                    {
                        cmdAddRoleMember.CommandType = CommandType.StoredProcedure;
                        cmdAddRoleMember.Parameters.Add("@rolename", SqlDbType.NVarChar).Value = "db_owner";
                        cmdAddRoleMember.Parameters.Add("@membername", SqlDbType.NVarChar).Value = user;
                        cmdAddRoleMember.ExecuteNonQuery();
                    }
                }

                ts.Complete();
            }
        }

        /// <summary>
        /// Deletes a database.
        /// </summary>
        /// <param name="provisionedService">The provisioned service for which the database needs to be deleted.</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2100:Review SQL queries for security vulnerabilities", Justification = "Query is retrieved from resource file."),
        System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "Exception is properly logged, it should not bubble up here")]
        private void DeleteDatabase(ProvisionedService provisionedService)
        {
            string name = provisionedService.Name;
            string user = provisionedService.User;

            try
            {
                this.DeleteDatabaseUser(user, name);
                Logger.Info(Strings.SqlNodeDeletingDatabaseInfoMessage, name);

                using (TransactionScope ts = new TransactionScope())
                {
                    using (SqlCommand takeOfflineCommand = new SqlCommand(string.Format(CultureInfo.InvariantCulture, Strings.SqlNodeTakeDBOfflineSQL, name), this.connection))
                    {
                        takeOfflineCommand.ExecuteNonQuery();
                    }

                    using (SqlCommand bringOnlineCommand = new SqlCommand(string.Format(CultureInfo.InvariantCulture, Strings.SqlNodeBringDBOnlineSQL, name), this.connection))
                    {
                        bringOnlineCommand.ExecuteNonQuery();
                    }

                    using (SqlCommand dropDatabaseCommand = new SqlCommand(string.Format(CultureInfo.InvariantCulture, Strings.SqlNodeDropDatabaseSQL, name), this.connection))
                    {
                        dropDatabaseCommand.ExecuteNonQuery();
                    }

                    ts.Complete();
                }
            }
            catch (Exception ex)
            {
                Logger.Fatal(Strings.SqlNodeCannotDeleteDBFatalMessage, ex.ToString());
            }
        }

        /// <summary>
        /// Deletes a database user.
        /// </summary>
        /// <param name="user">The user that has to be deleted.</param>
        /// <param name="database">The database owned by the user.</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2100:Review SQL queries for security vulnerabilities", Justification = "Query is retrieved from resource file."),
        System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "Exception is properly logged, it should not bubble up here")]
        private void DeleteDatabaseUser(string user, string database)
        {
            Logger.Info(Strings.SqlNodeDeleteUserInfoMessage, user);
            try
            {
                // Disable the login before kill the users session. So that other connections cannot be made between the killing
                // of user sessions and dropping the login.
                using (TransactionScope ts = new TransactionScope())
                {
                    using (SqlCommand disableLoginCommand = new SqlCommand(string.Format(CultureInfo.InvariantCulture, Strings.SqlNodeDisableLoginSQL, user), this.connection))
                    {
                        disableLoginCommand.ExecuteNonQuery();
                    }

                    this.KillUserSession(user);

                    using (SqlCommand dropLoginCommand = new SqlCommand(string.Format(CultureInfo.InvariantCulture, Strings.SqlNodeDropLoginSQL, user), this.connection))
                    {
                        dropLoginCommand.ExecuteNonQuery();
                    }

                    using (SqlCommand dropUserCommand = new SqlCommand(string.Format(CultureInfo.InvariantCulture, Strings.SqlNodeDropUserSQL, database, user), this.connection))
                    {
                        dropUserCommand.ExecuteNonQuery();
                    }

                    ts.Complete();
                }
            }
            catch (Exception ex)
            {
                Logger.Fatal(Strings.SqlNodeCannotDeleteUserFatalMessage, user, ex.ToString());
            }
        }

        /// <summary>
        /// Ends active sessions for USER, to be able to drop the database.
        /// </summary>
        /// <param name="user">The user.</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2100:Review SQL queries for security vulnerabilities", Justification = "Query is retrieved from resource file.")]
        private void KillUserSession(string user)
        {
            using (TransactionScope ts = new TransactionScope())
            {
                using (SqlCommand sessionsToKillCommand = new SqlCommand(string.Format(CultureInfo.InvariantCulture, Strings.SqlNodeGetUserSessionsSQL, user), this.connection))
                {
                    SqlDataReader sessionsToKill = sessionsToKillCommand.ExecuteReader();

                    while (sessionsToKill.Read())
                    {
                        int sessionId = sessionsToKill.GetInt16(0);
                        using (SqlCommand killSessionCommand = new SqlCommand(string.Format(CultureInfo.InvariantCulture, Strings.SqlNodeKillSessionSQL, sessionId), this.connection))
                        {
                            killSessionCommand.ExecuteNonQuery();
                        }
                    }

                    sessionsToKill.Close();
                }

                ts.Complete();
            }
        }

        /// <summary>
        /// Gets the instance health by checking if metadata can be retrieved.
        /// </summary>
        /// <param name="instance">The instance for which to get health status</param>
        /// <returns>A string that indicates whether the instance is healthy or not ("ok" / "fail")</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "Exception is properly logged, it should not bubble up here")]
        private string GetInstanceHealthz(ProvisionedService instance)
        {
            string res = "ok";

            try
            {
                string instanceConnectionString = string.Format(
                    CultureInfo.InvariantCulture,
                    Strings.SqlNodeConnectionString,
                    this.mssqlConfig.Host,
                    this.mssqlConfig.Port,
                    instance.User,
                    instance.Password);

                using (SqlConnection databaseConnection = new SqlConnection(instanceConnectionString))
                {
                    databaseConnection.Open();
                    databaseConnection.ChangeDatabase(instance.Name);

                    using (SqlCommand readDatabases = new SqlCommand("SELECT name from sys.tables", this.connection))
                    {
                        using (SqlDataReader reader = readDatabases.ExecuteReader())
                        {
                            reader.Read();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Warning(Strings.ErrorGettingDBTablesWarningMessage, instance.Name, ex.ToString());
                res = "fail";
            }

            return res;
        }

        /// <summary>
        /// Gets the number of queries executed by the SQL Server.
        /// </summary>
        /// <returns>The number of queries.</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Method is not yet implemented")]
        private int GetQueriesStatus()
        {
            // todo: vladi: implement this
            return 0;
        }

        /// <summary>
        /// Gets the Queries/Second metric.
        /// </summary>
        /// <returns>A double containing the number of queries per second executed by the SQL server.</returns>
        private double GetQPS()
        {
            Logger.Debug(Strings.SqlNodeCalculatingQPSDebugMessage);
            int queries = this.GetQueriesStatus();
            DateTime ts = DateTime.Now;

            double delta_t = (ts - this.qpsLastUpdated).TotalSeconds;

            double qps = (queries - @queriesServed) / delta_t;
            this.queriesServed = queries;
            this.qpsLastUpdated = ts;

            return qps;
        }

        /// <summary>
        /// Gets instance status, to be used in a varz message.
        /// </summary>
        /// <returns>An array containing the status of each instance.</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Method is not yet implemented")]
        private object[] GetInstanceStatus()
        {
            // todo: vladi: implement this
            return new object[0];
        }

        /// <summary>
        /// Creates a <see cref="ServiceCredentials"/> object using a database name, a username and a password.
        /// </summary>
        /// <param name="name">The name of the database.</param>
        /// <param name="user">The user.</param>
        /// <param name="password">The password.</param>
        /// <returns>A ServiceCredentials object.</returns>
        private ServiceCredentials GenerateCredential(string name, string user, string password)
        {
            ServiceCredentials response = new ServiceCredentials();

            response.Name = name;
            response.HostName = this.localIP;
            response.Port = this.mssqlConfig.Port;
            response.User = user;
            response.UserName = user;
            response.Password = password;

            return response;
        }
    }
}
