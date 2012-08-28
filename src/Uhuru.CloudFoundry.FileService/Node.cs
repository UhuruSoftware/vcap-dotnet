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
    using Uhuru.Configuration.Service;
    using Uhuru.Utilities;
    using Uhuru.Utilities.Json;

    /// <summary>
    /// This class is the File Service Node that brings persistent file storage to Cloud Foundry.
    /// </summary>
    public partial class FileServiceNode : NodeBase
    {
        /// <summary>
        /// Interval at which to verify storage quotas.
        /// </summary>
        private const int StorageQuotaInterval = 1000;

        /// <summary>
        /// Directory where the data is stored.
        /// </summary>
        private string baseDir;

        /// <summary>
        /// Maximum storage size. Value in MB.
        /// </summary>
        private long maxStorageSizeMB;

        /// <summary>
        /// Use VHD or not.
        /// </summary>
        private bool useVhd;

        /// <summary>
        /// Is the VHD is fixed or expandable.
        /// </summary>
        private bool vhdFixedSize;

        /// <summary>
        /// Use File Server Resource Manager or not to enforce qutoa and account for disk usage.
        /// </summary>
        private bool useFsrm;

        /// <summary>
        /// Use File Server Resource Manager or not to enforce qutoa and account for disk usage.
        /// </summary>
        private DirectoryAccounting dirAccounting;

        /// <summary>
        /// Number of directory provision requests served by the node.
        /// </summary>
        private int provisionServed;

        /// <summary>
        /// Number of binding requests served by the node.
        /// </summary>
        private int bindingServed;

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

                return a;
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

            this.baseDir = options.BaseDir;
            this.maxStorageSizeMB = options.Uhurufs.MaxStorageSize;
            this.useVhd = options.Uhurufs.UseVHD;
            this.vhdFixedSize = options.Uhurufs.VHDFixedSize;
            this.useFsrm = options.Uhurufs.UseFsrm;

            if (this.useFsrm)
            {
                this.dirAccounting = new DirectoryAccounting();
            }
            else
            {
                this.dirAccounting = null;
            }

            TimerHelper.RecurringCall(
                StorageQuotaInterval,
                delegate
                {
                    this.EnforceStorageQuota();
                });

            ProvisionedService.Initialize(options.LocalDB);

            foreach (ProvisionedService instance in ProvisionedService.GetInstances())
            {
                this.capacity = -this.CapacityUnit();
                if (this.useFsrm)
                {
                    this.dirAccounting.SetDirectoryQuota(this.GetInstanceDirectory(instance.Name), this.maxStorageSizeMB * 1024 * 1024);
                }
            }

            // initialize qps counter
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
        protected override bool ImportInstance(ServiceCredentials provisionedCredential, ServiceCredentials bindingCredentials, string filePath, string planRequest)
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
        /// Gets varz details about the Node.
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
                varz["max_capacity"] = this.maxCapacity;
                varz["available_capacity"] = this.capacity;

                varz["provisioned_instances_num"] = ProvisionedService.GetInstances().Count();

                var provisionedInstances = new List<object>();
                varz["provisioned_instances"] = provisionedInstances;

                foreach (ProvisionedService instance in ProvisionedService.GetInstances())
                {
                    provisionedInstances.Add(this.GetVarz(instance));
                }

                // how many provision/binding operations since startup.
                varz["provision_served"] = this.provisionServed;
                varz["binding_served"] = this.bindingServed;

                varz["instances"] = new Dictionary<string, string>();

                foreach (ProvisionedService instance in ProvisionedService.GetInstances())
                {
                    (varz["instances"] as Dictionary<string, string>)[instance.Name] = this.GetStatus(instance);
                }

                return varz;
            }
            catch (Exception ex)
            {
                Logger.Error(Strings.SqlNodeGenerateVarzErrorMessage, ex.ToString());
                return new Dictionary<string, object>();
            }
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
        protected override ServiceCredentials Provision(string planRequest)
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
        protected override ServiceCredentials Provision(string planRequest, ServiceCredentials credentials)
        {
            if (planRequest != this.plan)
            {
                throw new FileServiceErrorException(FileServiceErrorException.FileServiceInvalidPlan);
            }

            ProvisionedService provisioned_service = new ProvisionedService();

            if (credentials == null)
            {
                credentials = this.GenerateCredentials();
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

                CreateInstanceGroup(name);
                CreateInstanceUser(name, user, password);
                this.CreateInstanceStorage(provisioned_service);

                // add directory permissions
                string directory = this.GetInstanceDirectory(name);

                // Add permissions
                AddDirectoryPermissions(directory, name);

                // add permissions to ftp share
                FtpUtilities.AddUserAccess(name, user);

                // add permissions to windows share
                Uhuru.Utilities.WindowsShare ws = new Uhuru.Utilities.WindowsShare(name);
                ws.AddSharePermissions(user);

                if (!ProvisionedService.Save())
                {
                    Logger.Error(Strings.SqlNodeCannotSaveProvisionedServicesErrorMessage, provisioned_service.SerializeToJson());
                    throw new FileServiceErrorException(FileServiceErrorException.FileServiceLocalDBError);
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
                throw new FileServiceErrorException(FileServiceErrorException.FileServiceConfigNotFound, name);
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

            if (!provisioned_service.Destroy())
            {
                Logger.Error(Strings.SqlNodeDeleteServiceErrorMessage, provisioned_service.Name);
                throw new FileServiceErrorException(FileServiceErrorException.FileServiceLocalDBError);
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
                    throw new FileServiceErrorException(FileServiceErrorException.FileServiceConfigNotFound, name);
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

                CreateInstanceUser(name, binding["user"] as string, binding["password"] as string);

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
                    DeleteInstanceUser(binding["user"] as string);
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
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1303:Do not pass literals as localized parameters", MessageId = "Uhuru.Utilities.Logger.Error(System.String,System.Object[])", Justification = "Less error prone."), 
        System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "Necessary for cleanup."), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2100:Review SQL queries for security vulnerabilities", Justification = "Query is retrieved from resource file.")]
        protected override bool Unbind(ServiceCredentials credentials)
        {
            if (credentials == null)
            {
                return false;
            }

            Logger.Debug(Strings.SqlNodeUnbindServiceDebugMessage, credentials.SerializeToJson());

            string user = credentials.User;

            try
            {
                FtpUtilities.DeleteUserAccess(credentials.Name, user);
            }
            catch (Exception ex)
            {
                Logger.Error("Unable to revoke FTP access for instance {0} and user {1}. Exception: {2}", credentials.Name, user, ex.ToString());
            }

            try
            {
                WindowsShare ws = new WindowsShare(credentials.Name);
                ws.DeleteSharePermission(credentials.Name);
            }
            catch (Exception ex)
            {
                Logger.Error("Unable to revoke Windows Share access for instance {0} and user {1}. Exception: {2}", credentials.Name, user, ex.ToString());
            }

            try
            {
                DeleteInstanceUser(credentials.Name);
            }
            catch (Exception ex)
            {
                Logger.Error("Unable to delete bound user {1} for instance {0}. Exception: {2}", credentials.Name, user, ex.ToString());
            }

            return true;
        }

        /// <summary>
        /// Creates a Windows group for permissions.
        /// </summary>
        /// <param name="name">Instance name.</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1303:Do not pass literals as localized parameters", MessageId = "Uhuru.Utilities.Logger.Info(System.String,System.Object[])", Justification = "Less error prone.")]
        private static void CreateInstanceGroup(string name)
        {
            Logger.Info("Creating Windows group {0}", name);
            Uhuru.Utilities.WindowsUsersAndGroups.CreateGroup(name, "Uhuru File System Instance " + name);
        }

        /// <summary>
        /// Creates a Windows user for permissions.
        /// </summary>
        /// <param name="name">Instace name.</param>
        /// <param name="user">The user.</param>
        /// <param name="password">The password.</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1303:Do not pass literals as localized parameters", MessageId = "Uhuru.Utilities.Logger.Info(System.String,System.Object[])", Justification = "Less error prone.")]
        private static void CreateInstanceUser(string name, string user, string password)
        {
            Logger.Info("Creating Windows user {0} for instance {1}", user, name);
            Uhuru.Utilities.WindowsUsersAndGroups.CreateUser(user, password, "Uhuru File System Instance " + name);

            Logger.Info("Adding Windows user {0} to group {1}", user, name);
            Uhuru.Utilities.WindowsUsersAndGroups.AddUserToGroup(user, name);
        }

        /// <summary>
        /// Adds the directory permissions.
        /// </summary>
        /// <param name="directoryPath">The directory path.</param>
        /// <param name="usename">The usename.</param>
        private static void AddDirectoryPermissions(string directoryPath, string usename)
        {
            DirectoryInfo dir = new DirectoryInfo(directoryPath);
            DirectorySecurity deploymentDirSecurity = dir.GetAccessControl();

            deploymentDirSecurity.SetAccessRule(
                new FileSystemAccessRule(
                    usename,
                    FileSystemRights.FullControl,
                    InheritanceFlags.ContainerInherit | InheritanceFlags.ObjectInherit,
                    PropagationFlags.None,
                    AccessControlType.Allow));

            dir.SetAccessControl(deploymentDirSecurity);
        }

        /// <summary>
        /// Deletes a user.
        /// </summary>
        /// <param name="user">The user that has to be deleted.</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1303:Do not pass literals as localized parameters", MessageId = "Uhuru.Utilities.Logger.Info(System.String,System.Object[])", Justification = "Less error prone."),
        System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "Exception is properly logged, it should not bubble up here")]
        private static void DeleteInstanceUser(string user)
        {
            Logger.Info("Deleting instance user {0}", user);
            try
            {
                Uhuru.Utilities.WindowsUsersAndGroups.DeleteUser(user);
            }
            catch (Exception ex)
            {
                Logger.Fatal(Strings.SqlNodeCannotDeleteUserFatalMessage, user, ex.ToString());
            }
        }

        /// <summary>
        /// Deletes a user.
        /// </summary>
        /// <param name="name">The user that has to be deleted.</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1303:Do not pass literals as localized parameters", MessageId = "Uhuru.Utilities.Logger.Info(System.String,System.Object[])", Justification = "Less error prone."),
        System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "Exception is properly logged, it should not bubble up here")]
        private static void DeleteInstanceGroup(string name)
        {
             Logger.Info("Deleting instance group {0}", name);
            try
            {
                Uhuru.Utilities.WindowsUsersAndGroups.DeleteGroup(name);
            }
            catch (Exception ex)
            {
                Logger.Fatal(Strings.SqlNodeCannotDeleteUserFatalMessage, name, ex.ToString());
            }
        }

        /// <summary>
        /// Creates the database.
        /// </summary>
        /// <param name="provisionedService">The provisioned service for which a directory has to be created.</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2100:Review SQL queries for security vulnerabilities", Justification = "Query is retrieved from resource file."),
        System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "Exception is properly logged, it should not bubble up here")]
        private void CreateInstanceStorage(ProvisionedService provisionedService)
        {
            string name = provisionedService.Name;
            int port = provisionedService.Port.Value;

            try
            {
                DateTime start = DateTime.Now;
                Logger.Debug(Strings.SqlNodeCreateDatabaseDebugMessage, provisionedService.SerializeToJson());

                if (this.useVhd)
                {
                    string vhdDirectory = Path.Combine(this.baseDir, name);
                    string vhd = Path.Combine(this.baseDir, name + ".vhd");

                    VHDUtilities.CreateVHD(vhd, this.maxStorageSizeMB, this.vhdFixedSize);
                    VHDUtilities.MountVHD(vhd, vhdDirectory);

                    // TODO: stefi: revoke all permissions on parent direcotry if using VHD, 
                    // to deny read/write permissions when VHD is not mounted or got dismounted
                }

                string directory = this.GetInstanceDirectory(name);
                Directory.CreateDirectory(directory);

                if (this.useFsrm)
                {
                    this.dirAccounting.SetDirectoryQuota(directory, this.maxStorageSizeMB * 1024 * 1024);
                }

                FtpUtilities.CreateFtpSite(name, directory, port);

                WindowsShare.CreateShare(name, directory);

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
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1303:Do not pass literals as localized parameters", MessageId = "Uhuru.Utilities.Logger.Error(System.String,System.Object[])", Justification = "Less error prone."),
        System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2100:Review SQL queries for security vulnerabilities", Justification = "Query is retrieved from resource file."),
        System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "Exception is properly logged, it should not bubble up here")]
        private void DeleteDirectory(ProvisionedService provisionedService)
        {
            string name = provisionedService.Name;
            string user = provisionedService.User;

            try
            {
                Logger.Info(Strings.SqlNodeDeletingDatabaseInfoMessage, name);

                try
                {
                    FtpUtilities.DeleteFtpSite(name);
                }
                catch (Exception ex)
                {
                    Logger.Error("Unable to delete FTP site for instance {0}. Exception: {1}", name, ex.ToString());
                }

                try
                {
                    WindowsShare ws = new WindowsShare(name);
                    ws.DeleteShare();
                }
                catch (Exception ex)
                {
                    Logger.Error("Unable to delete Windows Share for instance {0}. Exception: {1}", name, ex.ToString());
                }

                try
                {
                    string directory = this.GetInstanceDirectory(name);
                    if (this.useFsrm)
                    {
                        this.dirAccounting.RemoveDirectoryQuota(directory);
                    }
                }
                catch (Exception ex)
                {
                    Logger.Error("Unable to delete directory quota rule for instance {0}. Exception: {1}", name, ex.ToString());
                }

                try
                {
                    string directory = this.GetInstanceDirectory(name);
                    Directory.Delete(directory, true);
                }
                catch (Exception ex)
                {
                    Logger.Error("Unable to delete instance directory for instance {0}. Exception: {1}", name, ex.ToString());
                }

                try
                {
                    DeleteInstanceUser(user);
                }
                catch (Exception ex)
                {
                    Logger.Error("Unable to delete user {0} for instance {1}. Exception: {2}", user, name, ex.ToString());
                }

                try
                {
                    DeleteInstanceGroup(name);
                }
                catch (Exception ex)
                {
                    Logger.Error("Unable to delete group {0} for instance {1}. Exception: {2}", name, name, ex.ToString());
                }

                if (this.useVhd)
                {
                    string vhdDirectory = Path.Combine(this.baseDir, name);
                    string vhd = Path.Combine(this.baseDir, name + ".vhd");

                    try
                    {
                        VHDUtilities.UnmountVHD(vhd);
                    }
                    catch (Exception ex)
                    {
                        Logger.Error("Unable to un-mount VHD {0} for instance {1}. Exception: {2}", vhd, name, ex.ToString());
                    }

                    try
                    {
                        File.Delete(vhd);
                    }
                    catch (Exception ex)
                    {
                        Logger.Error("Unable to delete VHD file {0} for instance {1}. Exception: {2}", vhd, name, ex.ToString());
                    }

                    try
                    {
                        Directory.Delete(vhdDirectory, true);
                    }
                    catch (Exception ex)
                    {
                        Logger.Error("Unable to delete VHD mount directory {0} for instance {1}. Exception: {2}", vhd, name, ex.ToString());
                    }
                }
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
        private string GetStatus(ProvisionedService instance)
        {
            // TODO: migrate this into varz: ship service per-instance healthz data over varz: https://github.com/cloudfoundry/vcap-services/commit/8b12af491edfea58953cd07e1c80954a9006b22d
            string res = "ok";

            try
            {
                using (new UserImpersonator(instance.User, ".", instance.Password, false))
                {
                    string testFile = Path.Combine(this.GetInstanceDirectory(instance.Name), Guid.NewGuid().ToString("N"));
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
        /// Gets instances status, to be used in a varz message.
        /// </summary>
        /// <param name="instance">The instance.</param>
        /// <returns>
        /// An object containing the status of the instance.
        /// </returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Method is not yet implemented")]
        private object GetVarz(ProvisionedService instance)
        {
            var varz = new Dictionary<string, object>();
            varz["name"] = instance.Name;
            varz["port"] = instance.Port;
            varz["plan"] = instance.Plan;
            
            var usage = new Dictionary<string, object>();
            varz["usage"] = usage;
            usage["max_storage_size"] = this.maxStorageSizeMB * 1024 * 1024;

            if (this.useFsrm)
            {
                usage["used_storage_size"] = this.dirAccounting.GetDirectorySize(this.GetInstanceDirectory(instance.Name));
            }
            else
            {
                usage["used_storage_size"] = -1;
            }

            // TODO: stefi: complete this instance varz
            // add: samba active connectoins, ftp active connections, disk IO (if possible), network IO (if possible)
            return varz;
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
            response.HostName = this.localIP;
            response.Port = port;
            response.User = user;
            response.UserName = user;
            response.Password = password;

            return response;
        }

        /// <summary>
        /// Gets the instance dir.
        /// </summary>
        /// <param name="instanceName">Name of the instance.</param>
        /// <returns>Instance directory</returns>
        private string GetInstanceDirectory(string instanceName)
        {
            if (this.useVhd)
            {
                return Path.Combine(this.baseDir, instanceName, instanceName);
            }
            else
            {
                return Path.Combine(this.baseDir, instanceName);
            }
        }
    }
}