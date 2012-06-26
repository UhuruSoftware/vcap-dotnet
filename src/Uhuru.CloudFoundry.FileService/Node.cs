// -----------------------------------------------------------------------
// <copyright file="Node.cs" company="Uhuru Software, Inc.">
// Copyright (c) 2011 Uhuru Software, Inc., All Rights Reserved
// </copyright>
// -----------------------------------------------------------------------

namespace Uhuru.CloudFoundry.FileService
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Security.AccessControl;
    using System.Threading;
    using System.Transactions;
    using Uhuru.CloudFoundry.ServiceBase;
    using Uhuru.Utilities;
    using Uhuru.Utilities.Json;

    /// <summary>
    /// This class is the File Service Node that brings persistent file storage to Cloud Foundry.
    /// </summary>
    public partial class Node : NodeBase
    {
        /// <summary>
        /// Interval at which to verify storage quotas.
        /// </summary>
        private const int StorageQuotaInterval = 1000;

        /// <summary>
        /// COnfiguration options for the File Service.
        /// </summary>
        private FileServiceOptions fileServiceConfig;

        /// <summary>
        /// The maximum file size, in bytes.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields", Justification = "Used for quata enforcement.")]
        private long maxFileSizeBytes;

        /// <summary>
        /// Current available storage on the node.
        /// </summary>
        private int availableCapacity;

        /// <summary>
        /// Number of directory provision requests served by the node.
        /// </summary>
        private int provisionServed;

        /// <summary>
        /// Number of binding requests served by the node.
        /// </summary>
        private int bindingServed;

        /// <summary>
        /// Local machine IP used by the service.
        /// </summary>
        private string localIp;

        /// <summary>
        /// Gets any service-specific announcement details.
        /// </summary>
        protected override Announcement AnnouncementDetails
        {
            get
            {
                Announcement a = new Announcement();
                a.AvailableCapacity = this.availableCapacity;
                a.CapacityUnit = this.CapacityUnit();

                return a;
            }
        }

        /// <summary>
        /// Starts the node.
        /// </summary>
        /// <param name="options">The configuration options for the node.</param>
        public override void Start(Options options)
        {
            base.Start(options);
        }

        /// <summary>
        /// Starts the node.
        /// </summary>
        /// <param name="options">The configuration options for the node.</param>
        /// <param name="fileServiceOptions">The file service options.</param>
        public void Start(Options options, FileServiceOptions fileServiceOptions)
        {
            if (options == null)
            {
                throw new ArgumentNullException("options");
            }

            if (fileServiceOptions == null)
            {
                throw new ArgumentNullException("fileServiceOptions");
            }

            this.fileServiceConfig = fileServiceOptions;
            this.maxFileSizeBytes = options.MaxDBSize * 1024 * 1024;
            this.localIp = NetworkInterface.GetLocalIPAddress(options.LocalRoute);

            TimerHelper.RecurringCall(
                StorageQuotaInterval,
                delegate
                {
                    this.EnforceStorageQuota();
                });

            ProvisionedService.Initialize(options.LocalDB);

            this.CheckDBConsistency();

            this.availableCapacity = options.Capacity;
            this.availableCapacity -= ProvisionedService.GetInstances().Count();

            // initialize qps counter
            this.provisionServed = 0;
            this.bindingServed = 0;
            this.Start(options);
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
        protected override bool DisableInstance(ServiceCredentials provisionedCredential, ServiceCredentials bindingCredentials)
        {
            // todo: vladi: Replace with code for odbc object for SQL Server
            return false;
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
        protected override bool DumpInstance(ServiceCredentials provisionedCredential, ServiceCredentials bindingCredentials, string filePath)
        {
            // todo: vladi: Replace with code for odbc object for SQL Server
            return false;
        }

        /// <summary>
        /// Imports an instance from a path.
        /// </summary>
        /// <param name="provisionedCredential">The provisioned credential.</param>
        /// <param name="bindingCredentials">The binding credentials.</param>
        /// <param name="filePath">The file path from which to import the service.</param>
        /// <param name="planRequest">The payment plan.</param>
        /// <returns>
        /// A bool indicating whether the request was successful.
        /// </returns>
        protected override bool ImportInstance(ServiceCredentials provisionedCredential, ServiceCredentials bindingCredentials, string filePath, ProvisionedServicePlanType planRequest)
        {
            // todo: vladi: Replace with code for odbc object for SQL Server
            return false;
        }

        /// <summary>
        /// Re-enables the instance, re-binds credentials.
        /// </summary>
        /// <param name="provisionedCredential">The provisioned credential.</param>
        /// <param name="bindingCredentialsHash">The binding credentials hash.</param>
        /// <returns>
        /// A bool indicating whether the request was successful.
        /// </returns>
        protected override bool EnableInstance(ref ServiceCredentials provisionedCredential, ref Dictionary<string, object> bindingCredentialsHash)
        {
            // todo: vladi: Replace with code for odbc object for SQL Server
            return false;
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
                // disk usage per instance
                object[] status = this.GetInstanceStatus();
                varz["service_status"] = status;

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
        protected override Dictionary<string, string> HealthzDetails()
        {
            Dictionary<string, string> healthz = new Dictionary<string, string>()
            {
                { "self", "ok" }
            };

            try
            {
                string testFile = Path.Combine(this.fileServiceConfig.SharedDrive, Guid.NewGuid().ToString("N"));
                File.WriteAllText(testFile, "test");
                File.Delete(testFile);
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
            return "UhurufsaaS";
        }

        /// <summary>
        /// Provisions a directory.
        /// </summary>
        /// <param name="planRequest">The payment plan for the service.</param>
        /// <returns>
        /// Credentials for the provisioned service.
        /// </returns>
        protected override ServiceCredentials Provision(ProvisionedServicePlanType planRequest)
        {
            return Provision(planRequest, null);
        }

        /// <summary>
        /// Provisions an directory.
        /// </summary>
        /// <param name="planRequest">The payment plan for the service.</param>
        /// <param name="credentials">Existing credentials for the service.</param>
        /// <returns>
        /// Credentials for the provisioned service.
        /// </returns>
        protected override ServiceCredentials Provision(ProvisionedServicePlanType planRequest, ServiceCredentials credentials)
        {
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
                int port = credentials.Port;
                provisioned_service.Name = name;
                provisioned_service.User = user;
                provisioned_service.Password = password;
                provisioned_service.Plan = planRequest;
                provisioned_service.Port = port;

                this.CreateDirectory(provisioned_service);

                // add directory permissions
                string directory = Path.Combine(this.fileServiceConfig.SharedDrive, name);
                AddDirectoryPermissions(directory, user);

                // add permissions to ftp share
                FtpUtilities.AddUserAccess(name, user);

                // add permissions to windows share
                Uhuru.Utilities.WindowsShare ws = new Uhuru.Utilities.WindowsShare(name);
                ws.AddSharePermissions(user);

                if (!ProvisionedService.Save())
                {
                    Logger.Error(Strings.SqlNodeCannotSaveProvisionedServicesErrorMessage, provisioned_service.SerializeToJson());
                    throw new FileServiceErrorException(FileServiceErrorException.MSSqlLocalDBError);
                }

                ServiceCredentials response = this.GenerateCredential(provisioned_service.Name, provisioned_service.User, provisioned_service.Password, provisioned_service.Port.Value);
                this.provisionServed += 1;
                return response;
            }
            catch (Exception)
            {
                this.DeleteDirectory(provisioned_service);
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
                "P4Ss" + Credentials.GenerateCredential(),
                Uhuru.Utilities.NetworkInterface.GrabEphemeralPort());
        }

        /// <summary>
        /// Unprovisions a directory.
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

            Logger.Debug(Strings.SqlNodeUnprovisionDatabaseDebugMessage, name, JsonConvertibleObject.SerializeToJson(bindings.Select(binding => binding.ToJsonIntermediateObject()).ToArray()));

            ProvisionedService provisioned_service = ProvisionedService.GetService(name);

            if (provisioned_service == null)
            {
                throw new FileServiceErrorException(FileServiceErrorException.MSSqlConfigNotFound, name);
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

            this.DeleteDirectory(provisioned_service);
            this.availableCapacity += this.CapacityUnit();

            if (!provisioned_service.Destroy())
            {
                Logger.Error(Strings.SqlNodeDeleteServiceErrorMessage, provisioned_service.Name);
                throw new FileServiceErrorException(FileServiceErrorException.MSSqlLocalDBError);
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
        protected override ServiceCredentials
            Bind(string name, Dictionary<string, object> bindOptions)
        {
            return Bind(name, bindOptions, null);
        }

        /// <summary>
        /// Binds a shared directory to an app.
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
                    throw new FileServiceErrorException(FileServiceErrorException.MSSqlConfigNotFound, name);
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

                CreateUser(name, binding["user"] as string, binding["password"] as string);

                // add directory permissions
                string directory = Path.Combine(this.fileServiceConfig.SharedDrive, name);
                AddDirectoryPermissions(directory, binding["user"] as string);

                // add permissions to ftp site
                FtpUtilities.AddUserAccess(name, binding["user"] as string);

                // add permissions to windows share
                Uhuru.Utilities.WindowsShare ws = new Uhuru.Utilities.WindowsShare(name);
                ws.AddSharePermissions(binding["user"] as string);

                ServiceCredentials response = this.GenerateCredential(name, binding["user"] as string, binding["password"] as string, service.Port.Value);

                Logger.Debug(Strings.SqlNodeBindResponseDebugMessage, response.SerializeToJson());
                this.bindingServed += 1;
                return response;
            }
            catch (Exception)
            {
                if (binding != null)
                {
                    DeleteUser(binding["user"] as string);
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

            FtpUtilities.DeleteUserAccess(credentials.Name, user);

            WindowsShare ws = new WindowsShare(credentials.Name);
            ws.DeleteSharePermission(user);

            DeleteUser(user);
            return true;
        }

        /// <summary>
        /// Creates a windows user for use with .
        /// </summary>
        /// <param name="name">The name of the database.</param>
        /// <param name="user">The user.</param>
        /// <param name="password">The password.</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2100:Review SQL queries for security vulnerabilities", Justification = "Query is retrieved from resource file.")]
        private static void CreateUser(string name, string user, string password)
        {
            Logger.Info(Strings.SqlNodeCreatingCredentialsInfoMessage, user, password, name);

            Uhuru.Utilities.WindowsVCAPUsers.CreateUser(user, password);
        }

        /// <summary>
        /// Adds the directory permissions.
        /// </summary>
        /// <param name="directoryPath">The directory path.</param>
        /// <param name="usename">The usename.</param>
        private static void AddDirectoryPermissions(string directoryPath, string usename)
        {
            DirectoryInfo dir = Directory.CreateDirectory(directoryPath);

            DirectorySecurity deploymentDirSecurity = dir.GetAccessControl();
            deploymentDirSecurity.SetAccessRule(
                new FileSystemAccessRule(
                    usename,
                    FileSystemRights.Write | FileSystemRights.Read | FileSystemRights.Delete | FileSystemRights.Modify,
                    InheritanceFlags.ContainerInherit | InheritanceFlags.ObjectInherit,
                    PropagationFlags.None,
                    AccessControlType.Allow));
            dir.SetAccessControl(deploymentDirSecurity);
        }

        /// <summary>
        /// Deletes a user.
        /// </summary>
        /// <param name="user">The user that has to be deleted.</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2100:Review SQL queries for security vulnerabilities", Justification = "Query is retrieved from resource file."),
        System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "Exception is properly logged, it should not bubble up here")]
        private static void DeleteUser(string user)
        {
            Logger.Info(Strings.SqlNodeDeleteUserInfoMessage, user);
            try
            {
                Uhuru.Utilities.WindowsVCAPUsers.DeleteUser(user);
            }
            catch (Exception ex)
            {
                Logger.Fatal(Strings.SqlNodeCannotDeleteUserFatalMessage, user, ex.ToString());
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
        /// Creates the database.
        /// </summary>
        /// <param name="provisionedService">The provisioned service for which a directory has to be created.</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2100:Review SQL queries for security vulnerabilities", Justification = "Query is retrieved from resource file."),
        System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "Exception is properly logged, it should not bubble up here")]
        private void CreateDirectory(ProvisionedService provisionedService)
        {
            string name = provisionedService.Name;
            string password = provisionedService.Password;
            string user = provisionedService.User;
            int port = provisionedService.Port.Value;

            try
            {
                DateTime start = DateTime.Now;
                Logger.Debug(Strings.SqlNodeCreateDatabaseDebugMessage, provisionedService.SerializeToJson());

                string directory = Path.Combine(this.fileServiceConfig.SharedDrive, name);
                Directory.CreateDirectory(directory);

                CreateUser(name, user, password);

                FtpUtilities.CreateFtpSite(name, directory, port);

                WindowsShare.CreateShare(name, directory);

                if (this.availableCapacity < this.CapacityUnit())
                {
                    throw new FileServiceErrorException(FileServiceErrorException.MSSqlDiskFull);
                }

                this.availableCapacity -= CapacityUnit();
                Logger.Debug(Strings.SqlNodeDoneCreatingDBDebugMessage, provisionedService.SerializeToJson(), (start - DateTime.Now).TotalSeconds);
            }
            catch (Exception ex)
            {
                Logger.Warning(Strings.SqlNodeCouldNotCreateDBWarningMessage, ex.ToString());
            }
        }

        /// <summary>
        /// Deletes a database.
        /// </summary>
        /// <param name="provisionedService">The provisioned service for which the directory needs to be deleted.</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2100:Review SQL queries for security vulnerabilities", Justification = "Query is retrieved from resource file."),
        System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "Exception is properly logged, it should not bubble up here")]
        private void DeleteDirectory(ProvisionedService provisionedService)
        {
            string name = provisionedService.Name;
            string user = provisionedService.User;

            try
            {
                DeleteUser(user);
                Logger.Info(Strings.SqlNodeDeletingDatabaseInfoMessage, name);

                string directory = Path.Combine(this.fileServiceConfig.SharedDrive, name);

                FtpUtilities.DeleteFtpSite(name);

                WindowsShare ws = new WindowsShare(name);
                ws.DeleteShare();

                Directory.Delete(directory, true);
            }
            catch (Exception ex)
            {
                Logger.Fatal(Strings.SqlNodeCannotDeleteDBFatalMessage, ex.ToString());
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
                using (new UserImpersonator(instance.User, ".", instance.Password, false))
                {
                    string instancePath = Path.Combine(this.fileServiceConfig.SharedDrive, instance.Name);

                    string testFile = Path.Combine(instancePath, Guid.NewGuid().ToString("N"));

                    File.WriteAllText(testFile, "test");

                    File.Delete(testFile);
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
        /// Creates a <see cref="ServiceCredentials"/> object using a directory name, a username and a password.
        /// </summary>
        /// <param name="name">The name of the directory.</param>
        /// <param name="user">The user.</param>
        /// <param name="password">The password.</param>
        /// <param name="port">The port.</param>
        /// <returns>
        /// A ServiceCredentials object.
        /// </returns>
        private ServiceCredentials GenerateCredential(string name, string user, string password, int port)
        {
            ServiceCredentials response = new ServiceCredentials();

            response.Name = name;
            response.HostName = this.localIp;
            response.Port = port;
            response.User = user;
            response.UserName = user;
            response.Password = password;

            return response;
        }
    }
}