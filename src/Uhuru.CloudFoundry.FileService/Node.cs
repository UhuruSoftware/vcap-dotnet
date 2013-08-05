// -----------------------------------------------------------------------
// <copyright file="Node.cs" company="Uhuru Software, Inc.">
// Copyright (c) 2011 Uhuru Software, Inc., All Rights Reserved
// </copyright>
// -----------------------------------------------------------------------

namespace Uhuru.CloudFoundry.FileService
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.IO;
    using System.Linq;
    using System.Security.AccessControl;
    using Uhuru.CloudFoundry.ServiceBase;
    using Uhuru.Configuration;
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
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1303:Do not pass literals as localized parameters", MessageId = "Uhuru.Utilities.Logger.Error(System.String,System.Object[])", Justification = "Code more clear and readable."),
        System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "Start the node on instances failure.")]
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

            ProvisionedService.Initialize(options.LocalDB);
            ProvisionedService[] provisionedServices = ProvisionedService.GetInstances();

            var instances = new Dictionary<string, ProvisionedService>();
            var bindings = new Dictionary<string, ServiceBinding>();

            foreach (ProvisionedService instance in provisionedServices)
            {
                instances[instance.Name] = instance;
                bindings[instance.User] = new ServiceBinding();

                foreach (ServiceBinding binding in instance.Bindings)
                {
                    bindings[binding.User] = binding;
                }
            }


            HashSet<string> sharesCache = new HashSet<string>(WindowsShare.GetShares());
            HashSet<string> ftpCache = new HashSet<string>(FtpUtilities.GetFtpSties());
            HashSet<string> usersCache = new HashSet<string>(WindowsUsersAndGroups.GetUsers());
            HashSet<string> groupsCache = new HashSet<string>(WindowsUsersAndGroups.GetGroups());


            // Delete orphan shares
            foreach (string shareName in sharesCache)
            {
                if (shareName.StartsWith("D4Ta") && !instances.ContainsKey(shareName))
                {
                    // then this is orphan
                    Logger.Info("Deleting share {0}", shareName);

                    // Try delete windows share.
                    try
                    {
                        WindowsShare ws = new WindowsShare(shareName);
                        ws.DeleteShare();
                    }
                    catch (Exception ex)
                    {
                        Logger.Error("Unable to delete Windows Share for instance {0}. Exception: {1}", shareName, ex.ToString());
                    }

                    // Try delete directory quota.
                    try
                    {
                        string directory = this.GetInstanceDirectory(shareName);
                        if (this.useFsrm)
                        {
                            this.dirAccounting.RemoveDirectoryQuota(directory);
                        }
                    }
                    catch (Exception ex)
                    {
                        Logger.Error("Unable to delete directory quota rule for instance {0}. Exception: {1}", shareName, ex.ToString());
                    }
                }
            }

            // Delete orphan ftpsites
            foreach (string ftpName in ftpCache)
            {
                if (ftpName.StartsWith("D4Ta") && !instances.ContainsKey(ftpName))
                {
                    // then we have an orphan here
                    Logger.Info("Deleting ftp site {0}", ftpName);

                    // Try delete ftp site.
                    try
                    {
                        FtpUtilities.DeleteFtpSite(ftpName);
                    }
                    catch (Exception ex)
                    {
                        Logger.Error("Unable to delete FTP site for instance {0}. Exception: {1}", ftpName, ex.ToString());
                    }
                }
            }

            foreach (string user in usersCache)
            {
                if (
                    (user.StartsWith("D4Ta") || user.StartsWith("US3r")) &&
                    !instances.ContainsKey(user) &&
                    !bindings.ContainsKey(user)
                    )
                {
                    Logger.Info("Deleting instance user {0}", user);
                    try
                    {
                        Uhuru.Utilities.WindowsUsersAndGroups.DeleteUser(user);
                    }
                    catch (Exception ex)
                    {
                        Logger.Fatal("Unable to delete user for instance {0}. Exception: {1}", user, ex.ToString());
                    }
                }
            }

            foreach (string group in groupsCache)
            {
                if (group.StartsWith("D4Ta") && !instances.ContainsKey(group))
                {
                    Logger.Info("Deleting instance group {0}", group);
                    try
                    {
                        Uhuru.Utilities.WindowsUsersAndGroups.DeleteGroup(group);
                    }
                    catch (Exception ex)
                    {
                        Logger.Fatal("Unable to delete group for instance {0}. Exception: {1}", group, ex.ToString());
                    }
                }
            }

            //string directory = this.GetInstanceDirectory(name);

            //foreach (ProvisionedService instance in ProvisionedService.GetInstances())
            //{
            //    this.capacity -= this.CapacityUnit();

            //    // This check will make initialization faster.
            //    if (!sharesCache.Contains(instance.Name))
            //    {
            //        // This will setup the instance with new config changes or if the OS is fresh.
            //        // Don't want to fail if an instance is inconsistent or has errors.
            //        try
            //        {
            //            this.InstanceSystemSetup(instance);

            //            foreach (ServiceBinding binding in instance.Bindings)
            //            {
            //                try
            //                {
            //                    Bind(instance, binding);
            //                }
            //                catch (Exception ex)
            //                {
            //                    Logger.Error("Error binding instance {0} with {1}. Exception: {2}", instance.Name, binding.User, ex.ToString());
            //                }
            //            }
            //        }
            //        catch (Exception ex)
            //        {
            //            Logger.Error("Error setting up instance {0}. Exception: {1}", instance.Name, ex.ToString());
            //        }
            //    }
            //}

            //TimerHelper.RecurringCall(
            //    StorageQuotaInterval,
            //    delegate
            //    {
            //        this.EnforceStorageQuota();
            //    });

            //// initialize qps counter
            //this.provisionServed = 0;
            //this.bindingServed = 0;
            //base.Start(options);
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
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1303:Do not pass literals as localized parameters", Justification = "Less error prone."),
        System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "Error is properly logged")]
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

            Logger.Info("Disable instance {0} request", provisionedCredential.Name);

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
                Logger.Warning("Error disabling instance {0}: [{1}]", provisionedCredential.Name, ex.ToString());
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
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "Error is logged"),
        System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1303:Do not pass literals as localized parameters", Justification = "Less error prone.")]
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

            Logger.Info("Dump instance {0} content to {1}", provisionedCredential.Name, dumpFile);

            try
            {
                string instanceDir = this.GetInstanceDirectory(provisionedCredential.Name);
                if (!Directory.EnumerateFileSystemEntries(instanceDir).Any())
                {
                    File.Create(Path.Combine(instanceDir, Strings.MigrationEmptyFolderDummyFileName)).Dispose();
                }

                ZipUtilities.ZipFile(instanceDir, dumpFile);
            }
            catch (Exception ex)
            {
                Logger.Warning("Error dumping instance {0}: [{1}]", provisionedCredential.Name, ex.ToString());
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
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "Error is logged"),
        System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1303:Do not pass literals as localized parameters", Justification = "Less error prone.")]
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
            Logger.Debug("Import instance {0} from {1}", provisionedCredential.Name, dumpFile);

            try
            {
                this.Provision(planRequest, provisionedCredential, this.defaultVersion);
                string instanceDir = this.GetInstanceDirectory(provisionedCredential.Name);
                ZipUtilities.UnzipFile(this.GetInstanceDirectory(provisionedCredential.Name), dumpFile);
                if (File.Exists(Path.Combine(instanceDir, Strings.MigrationEmptyFolderDummyFileName)))
                {
                    File.Delete(Path.Combine(instanceDir, Strings.MigrationEmptyFolderDummyFileName));
                }
            }
            catch (Exception ex)
            {
                Logger.Warning("Error importing instance {0} from {1}: [{2}]", provisionedCredential.Name, dumpFile, ex.ToString());
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
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "Error is properly logged"),
        System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1303:Do not pass literals as localized parameters", Justification = "Less error prone.")]
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

            Logger.Debug("Enabling instance {0}", provisionedCredential.Name);

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
                Logger.Warning("Could not enable instance {0}: [{1}]", provisionedCredential.Name, ex.ToString());
                return false;
            }

            return true;
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
            return Provision(planRequest, null, this.defaultVersion);
        }

        /// <summary>
        /// Provisions an directory.
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
                throw new FileServiceErrorException(FileServiceErrorException.FileServiceInvalidPlan);
            }

            if (!this.supportedVersions.Contains(version))
            {
                throw new FileServiceErrorException(ServiceException.UnsupportedVersion);
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

                this.CreateInstanceStorage(provisioned_service);
                this.InstanceSystemSetup(provisioned_service);

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
                this.InstanceCleanup(provisioned_service);
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
        /// Subclasses have to implement this in order to update services.
        /// </summary>
        /// <param name="provisionedCredential">The provisioned credentials.</param>
        /// <param name="bindingCredentials">The binding credentials.</param>
        /// <returns>
        /// Updated service credentials
        /// </returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "Error is logged")]
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
        /// Unprovisions a directory.
        /// </summary>
        /// <param name="name">The name of the service to unprovision.</param>
        /// <param name="bindings">Array of bindings for the service that have to be unprovisioned.</param>
        /// <returns>
        /// A boolean specifying whether the unprovision request was successful.
        /// </returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1303:Do not pass literals as localized parameters", MessageId = "Uhuru.Utilities.Logger.Error(System.String,System.Object[])", Justification = "More clear."), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "Exception is properly logged, it should not bubble up here")]
        protected override bool Unprovision(string name, ServiceCredentials[] bindings)
        {
            if (string.IsNullOrEmpty(name))
            {
                return false;
            }

            bool success = true;

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

            if (!this.InstanceCleanup(provisioned_service))
            {
                success = false;
            }

            if (!provisioned_service.Destroy())
            {
                Logger.Error(Strings.SqlNodeDeleteServiceErrorMessage, provisioned_service.Name);
                throw new FileServiceErrorException(FileServiceErrorException.FileServiceLocalDBError);
            }

            Logger.Debug(Strings.SqlNodeUnprovisionSuccessDebugMessage, name);
            return success;
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

            ProvisionedService service = ProvisionedService.GetService(name);
            if (service == null)
            {
                throw new FileServiceErrorException(FileServiceErrorException.FileServiceConfigNotFound, name);
            }

            string user = null;
            string password = null;

            if (credentials != null)
            {
                user = credentials.User;
                password = credentials.Password;
            }
            else
            {
                user = "US3R" + Credentials.GenerateCredential();
                password = "P4SS" + Credentials.GenerateCredential();
            }

            var binding = new ServiceBinding
                {
                    User = user,
                    Password = password
                };

            Bind(service, binding);

            service.Bindings.Add(binding);

            if (!ProvisionedService.Save())
            {
                Logger.Error(Strings.SqlNodeCannotSaveProvisionedServicesErrorMessage, credentials == null ? string.Empty : credentials.SerializeToJson());
                throw new FileServiceErrorException(FileServiceErrorException.FileServiceLocalDBError);
            }

            ServiceCredentials response = this.GenerateCredential(name, user, password, service.Port.Value);

            Logger.Debug(Strings.SqlNodeBindResponseDebugMessage, response.SerializeToJson());
            this.bindingServed += 1;

            return response;
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

            bool success = true;

            Logger.Debug(Strings.SqlNodeUnbindServiceDebugMessage, credentials.SerializeToJson());

            string name = credentials.Name;
            string user = credentials.User;

            ProvisionedService serviceInstance = ProvisionedService.GetService(name);

            // Remove the binding form the local db
            var binding = serviceInstance.Bindings.FirstOrDefault(p => p.User == user);
            serviceInstance.Bindings.Remove(binding);

            if (!ProvisionedService.Save())
            {
                Logger.Error(Strings.SqlNodeCannotSaveProvisionedServicesErrorMessage, credentials.SerializeToJson());
                throw new FileServiceErrorException(FileServiceErrorException.FileServiceLocalDBError);
            }

            try
            {
                DeleteInstanceUser(user);
            }
            catch (Exception ex)
            {
                Logger.Error("Unable to delete bound user {1} for instance {0}. Exception: {2}", name, user, ex.ToString());
                success = false;
            }

            return success;
        }

        /// <summary>
        /// Bind the service instance.
        /// </summary>
        /// <param name="service">The service instance.</param>
        /// <param name="binding">Binding information.</param>
        private static void Bind(ProvisionedService service, ServiceBinding binding)
        {
            if (!WindowsUsersAndGroups.ExistsUser(binding.User))
            {
                CreateInstanceUser(service.Name, binding.User, binding.Password);
                AddInstanceUserToGroup(service.Name, binding.User);
            }
        }

        /// <summary>
        /// Creates a Windows group for permissions.
        /// </summary>
        /// <param name="name">Instance name.</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1303:Do not pass literals as localized parameters", MessageId = "Uhuru.Utilities.Logger.Info(System.String,System.Object[])", Justification = "Less error prone.")]
        private static void CreateInstanceGroup(string name)
        {
            Logger.Info("Creating Windows group {0}", name);
            WindowsUsersAndGroups.CreateGroup(name, "Uhuru File System Instance " + name);
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
            WindowsUsersAndGroups.CreateUser(user, password, "Uhuru File System Instance " + name);
        }

        /// <summary>
        /// Creates a Windows user for permissions.
        /// </summary>
        /// <param name="name">Instace name.</param>
        /// <param name="user">The user.</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1303:Do not pass literals as localized parameters", MessageId = "Uhuru.Utilities.Logger.Info(System.String,System.Object[])", Justification = "Less error prone.")]
        private static void AddInstanceUserToGroup(string name, string user)
        {
            Logger.Info("Adding Windows user {0} to group {1}", user, name);
            WindowsUsersAndGroups.AddUserToGroup(user, name);
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
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1303:Do not pass literals as localized parameters", MessageId = "Uhuru.Utilities.Logger.Error(System.String,System.Object[])", Justification = "Nonsense."), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2100:Review SQL queries for security vulnerabilities", Justification = "Query is retrieved from resource file."),
        System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "Exception is properly logged, it should not bubble up here")]
        private void CreateInstanceStorage(ProvisionedService provisionedService)
        {
            string name = provisionedService.Name;

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

                Logger.Debug(Strings.SqlNodeDoneCreatingDBDebugMessage, provisionedService.SerializeToJson(), (start - DateTime.Now).TotalSeconds);
            }
            catch (Exception ex)
            {
                Logger.Error("Cloud not create instance storage {0}. Exception {1}.", provisionedService.SerializeToJson(), ex.ToString());
                throw;
            }
        }

        /// <summary>
        /// Setup the windows system for the instance. This is a idempotent method.
        /// </summary>
        /// <param name="provisionedService">The provisioned service for which a directory has to be created.</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1303:Do not pass literals as localized parameters", Justification = "Nonsense."),
        System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2100:Review SQL queries for security vulnerabilities", Justification = "Query is retrieved from resource file."),
        System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "Exception is properly logged, it should not bubble up here")]
        private void InstanceSystemSetup(ProvisionedService provisionedService)
        {
            string name = provisionedService.Name;
            string user = provisionedService.User;
            string password = provisionedService.Password;
            string directory = this.GetInstanceDirectory(name);
            int port = provisionedService.Port.Value;

            try
            {
                DateTime start = DateTime.Now;
                Logger.Debug(Strings.SqlNodeCreateDatabaseDebugMessage, provisionedService.SerializeToJson());

                // The group and users have to be recreated if the box was recreated by bosh.
                // In that case only the file system resrouces remain. Every system 
                // resource of configuration has to be provisioned again.
                //
                // Create the group and user if necessary
                if (!WindowsUsersAndGroups.ExistsGroup(name))
                {
                    Logger.Info("Creating window group: {0}, for instance {1}", name, name);
                    CreateInstanceGroup(name);

                    // Add group permissions to directory
                    // TODO: stefi: consider cleaning up orphan users and groups
                    Logger.Info("Adding group permissions for directory: {0}", directory);
                    AddDirectoryPermissions(directory, name);

                    if (!WindowsUsersAndGroups.ExistsUser(user))
                    {
                        Logger.Info("Creating user: {0}, for instance {1}", user, name);
                        CreateInstanceUser(name, user, password);
                        AddInstanceUserToGroup(name, user);
                    }
                }

                // create the vhd if necessary
                if (this.useVhd)
                {
                    string vhdDirectory = Path.Combine(this.baseDir, name);
                    string vhd = Path.Combine(this.baseDir, name + ".vhd");

                    if (!VHDUtilities.IsMountPointPresent(vhdDirectory))
                    {
                        Logger.Info("Mounting VHD: {0}, at: {1}, for instance {2}", vhd, vhdDirectory, name);
                        VHDUtilities.MountVHD(vhd, vhdDirectory);
                    }
                }

                if (this.useFsrm)
                {
                    Logger.Info("Setting up windows FSRM for instance: {0}, with quota size: {1} MB", name, this.maxStorageSizeMB);
                    this.dirAccounting.SetDirectoryQuota(directory, this.maxStorageSizeMB * 1024 * 1024);
                }

                // create ftp service if necessary
                if (!FtpUtilities.Exists(name))
                {
                    Logger.Info("Creating ftp site for instance: {0}, at: {1}, with port: {2}", name, directory, port);
                    FtpUtilities.CreateFtpSite(name, directory, port);
                }

                if (!FtpUtilities.HasGroupAccess(name, name))
                {
                    // Add group permissions to ftp share
                    Logger.Info("Adding group permission for ftp site for instance: {0}", name);
                    FtpUtilities.AddGroupAccess(name, name);
                }

                // create the windows share with necessary permission 
                var ws = new WindowsShare(name);
                if (!ws.Exists())
                {
                    Logger.Info("Creating windows share for instance: {0}, at: {1}", name, directory);
                    ws = WindowsShare.CreateShare(name, directory);
                }

                if (!ws.HasPermission(name))
                {
                    // Add group permissions to windows share
                    Logger.Info("Adding group permission for windows share for instance: {0}", name);
                    ws.AddSharePermission(name);
                }

                Logger.Debug("Done setting up instance {0}. Took {1}s.", provisionedService.SerializeToJson(), (start - DateTime.Now).TotalSeconds);
            }
            catch (Exception ex)
            {
                Logger.Error("Cloud not setup instance {0}. Exception {1}.", provisionedService.SerializeToJson(), ex.ToString());
            }
        }

        /// <summary>
        /// Deletes a database.
        /// </summary>
        /// <param name="provisionedService">The provisioned service for which the directory needs to be deleted.</param>
        /// <returns>If the operation was successful true, else false.</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1303:Do not pass literals as localized parameters", MessageId = "Uhuru.Utilities.Logger.Error(System.String,System.Object[])", Justification = "Less error prone."),
        System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2100:Review SQL queries for security vulnerabilities", Justification = "Query is retrieved from resource file."),
        System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "Exception is properly logged, it should not bubble up here")]
        private bool InstanceCleanup(ProvisionedService provisionedService)
        {
            bool success = true;

            string name = provisionedService.Name;

            try
            {
                Logger.Info(Strings.SqlNodeDeletingDatabaseInfoMessage, name);

                // Try delete ftp site.
                try
                {
                    FtpUtilities.DeleteFtpSite(name);
                }
                catch (Exception ex)
                {
                    success = false;
                    Logger.Error("Unable to delete FTP site for instance {0}. Exception: {1}", name, ex.ToString());
                }

                // Try delete windows share.
                try
                {
                    WindowsShare ws = new WindowsShare(name);
                    ws.DeleteShare();
                }
                catch (Exception ex)
                {
                    success = false;
                    Logger.Error("Unable to delete Windows Share for instance {0}. Exception: {1}", name, ex.ToString());
                }

                // Try delete directory quota.
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
                    success = false;
                    Logger.Error("Unable to delete directory quota rule for instance {0}. Exception: {1}", name, ex.ToString());
                }

                if (!this.useVhd)
                {
                    // Try delete instance directory
                    try
                    {
                        string directory = this.GetInstanceDirectory(name);
                        Directory.Delete(directory, true);
                    }
                    catch (Exception ex)
                    {
                        success = false;
                        Logger.Error("Unable to delete instance directory for instance {0}. Exception: {1}", name, ex.ToString());
                    }
                }

                if (this.useVhd)
                {
                    string vhdDirectory = Path.Combine(this.baseDir, name);
                    string vhd = Path.Combine(this.baseDir, name + ".vhd");

                    // Try unmount VHD
                    try
                    {
                        VHDUtilities.UnmountVHD(vhd);
                    }
                    catch (Exception ex)
                    {
                        success = false;
                        Logger.Error("Unable to un-mount VHD {0} for instance {1}. Exception: {2}", vhd, name, ex.ToString());
                    }

                    // Try delete VHD
                    try
                    {
                        File.Delete(vhd);
                    }
                    catch (Exception ex)
                    {
                        success = false;
                        Logger.Error("Unable to delete VHD file {0} for instance {1}. Exception: {2}", vhd, name, ex.ToString());
                    }

                    // Try delete mount dir
                    try
                    {
                        // Do not use recursive. The directory should be empty.
                        // If the dir contains data, some users data may be lost
                        Directory.Delete(vhdDirectory, false);
                    }
                    catch (Exception ex)
                    {
                        success = false;
                        Logger.Error("Unable to delete VHD file {0} for instance {1}. Exception: {2}", vhd, name, ex.ToString());
                    }
                }

                // Try delete the user
                try
                {
                    DeleteInstanceUser(provisionedService.User);
                }
                catch (Exception ex)
                {
                    success = false;
                    Logger.Error("Unable to delete user {0} for instance {1}. Exception: {2}", provisionedService.User, name, ex.ToString());
                }

                // Try delete the group
                try
                {
                    DeleteInstanceGroup(name);
                }
                catch (Exception ex)
                {
                    success = false;
                    Logger.Error("Unable to delete group {0} for instance {1}. Exception: {2}", name, name, ex.ToString());
                }
            }
            catch (Exception ex)
            {
                success = false;
                Logger.Fatal(Strings.SqlNodeCannotDeleteDBFatalMessage, ex.ToString());
            }

            return success;
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
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1303:Do not pass literals as localized parameters", MessageId = "Uhuru.Utilities.Logger.Warning(System.String,System.Object[])", Justification = "Easier to find logs."),
        System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "Don't crash the entire method."),
        System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Method is not yet implemented")]
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
                try
                {
                    usage["used_storage_size"] = this.dirAccounting.GetDirectorySize(this.GetInstanceDirectory(instance.Name));
                }
                catch (Exception ex)
                {
                    Logger.Warning("Error getting FSRM info for {0}. Exception: {1}", instance.Name, ex.ToString());
                    usage["used_storage_size"] = -1;
                }
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
