// -----------------------------------------------------------------------
// <copyright file="Node.cs" company="Uhuru Software">
// Copyright (c) 2011 Uhuru Software, Inc., All Rights Reserved
// </copyright>
// -----------------------------------------------------------------------

namespace Uhuru.CloudFoundry.ServiceBase
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using Uhuru.NatsClient;
    using Uhuru.Utilities;
    
    /// <summary>
    /// This is the base class for all Cloud Foundry system services nodes.
    /// </summary>
    public abstract class NodeBase : SystemServiceBase
    {
        private string nodeId;
        private string migrationNfs;
        
        /// <summary>
        /// Starts the node.
        /// </summary>
        /// <param name="options">The configuration options for the node.</param>
        public override void Start(Options options)
        {
            if (options == null)
            {
                throw new ArgumentNullException("options");
            }

            this.nodeId = options.NodeId;
            this.migrationNfs = options.MigrationNFS;
            base.Start(options);
        }

        /// <summary>
        /// Gets the flavor of this service (only node for .Net)
        /// </summary>
        /// <returns>"Node"</returns>
        protected override string Flavor()
        {
            return "Node";
        }

        /// <summary>
        /// This is called after the node is connected to NATS.
        /// </summary>
        protected override void OnConnectNode()
        {
           Logger.Debug(Strings.ConnectedLogMessage, ServiceDescription());

            NodeNats.Subscribe(String.Format(CultureInfo.InvariantCulture, Strings.NatsSubjectProvision, ServiceName(), nodeId), new SubscribeCallback(OnProvision));
            NodeNats.Subscribe(String.Format(CultureInfo.InvariantCulture, Strings.NatsSubjectUnprovision, ServiceName(), nodeId), new SubscribeCallback(OnUnprovision));
            NodeNats.Subscribe(String.Format(CultureInfo.InvariantCulture, Strings.NatsSubjectBind, ServiceName(), nodeId), new SubscribeCallback(OnBind));
            NodeNats.Subscribe(String.Format(CultureInfo.InvariantCulture, Strings.NatsSubjectUnbind, ServiceName(), nodeId), new SubscribeCallback(OnUnbind));
            NodeNats.Subscribe(String.Format(CultureInfo.InvariantCulture, Strings.NatsSubjectRestore, ServiceName(), nodeId), new SubscribeCallback(OnRestore));

            NodeNats.Subscribe(String.Format(CultureInfo.InvariantCulture, Strings.NatsSubjectDiscover, ServiceName()), new SubscribeCallback(OnDiscover));

            NodeNats.Subscribe(String.Format(CultureInfo.InvariantCulture, Strings.NatsSubjectDisableInstance, ServiceName(), nodeId), new SubscribeCallback(OnDisableInstance));
            NodeNats.Subscribe(String.Format(CultureInfo.InvariantCulture, Strings.NatsSubjectEnableInstance, ServiceName(), nodeId), new SubscribeCallback(OnEnableInstance));
            NodeNats.Subscribe(String.Format(CultureInfo.InvariantCulture, Strings.NatsSubjectImportInstance, ServiceName(), nodeId), new SubscribeCallback(OnImportInstance));
            NodeNats.Subscribe(String.Format(CultureInfo.InvariantCulture, Strings.NatsSubjectCleanupNfs, ServiceName(), nodeId), new SubscribeCallback(OnCleanupNfs));

            NodeNats.Subscribe(String.Format(CultureInfo.InvariantCulture, Strings.NatsSubjectCheckOrphan, ServiceName()), new SubscribeCallback(OnCheckOrphan));
            NodeNats.Subscribe(String.Format(CultureInfo.InvariantCulture, Strings.NatsSubjectPurgeOrphan, ServiceName(), nodeId), new SubscribeCallback(OnPurgeOrphan));

            SendNodeAnnouncement();

            TimerHelper.RecurringCall(30000, delegate()
            {
                SendNodeAnnouncement();
            });
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        private void OnProvision(string msg, string reply, string subject)
        {
            Logger.Debug(Strings.OnProvisionRequestDebugLogMessage, ServiceDescription(), msg, reply);
            ProvisionResponse response = new ProvisionResponse();
            try
            {
                ProvisionRequest provision_req = new ProvisionRequest();

                provision_req.FromJsonIntermediateObject(JsonConvertibleObject.DeserializeFromJson(msg));

                ProvisionedServicePlanType plan = provision_req.Plan;
                ServiceCredentials credentials = provision_req.Credentials;
                ServiceCredentials credential = Provision(plan, credentials);
                credential.NodeId = nodeId;

                response.Credentials = credential;

                Logger.Debug(Strings.OnProvisionSuccessDebugLogMessage,
                    ServiceDescription(), msg, response.SerializeToJson());

                NodeNats.Publish(reply, null, EncodeSuccess(response));
            }
            catch (Exception ex)
            {
                Logger.Warning(ex.ToString());
                NodeNats.Publish(reply, null, EncodeFailure(response));
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        private void OnUnprovision(string msg, string reply, string subject)
        {
            Logger.Debug(Strings.UnprovisionRequestDebugLogMessage, ServiceDescription(), msg);
           
            SimpleResponse response = new SimpleResponse();
            try
            {
                UnprovisionRequest unprovision_req = new UnprovisionRequest();
                unprovision_req.FromJsonIntermediateObject(JsonConvertibleObject.DeserializeFromJson(msg));

                string name = unprovision_req.Name;
                ServiceCredentials[] bindings = unprovision_req.Bindings;

                bool result = Unprovision(name, bindings);

                if (result)
                {
                    NodeNats.Publish(reply, null, EncodeSuccess(response));
                }
                else
                {
                    NodeNats.Publish(reply, null, EncodeFailure(response));
                }
            }
            catch (Exception ex)
            {
                Logger.Warning(ex.ToString());
                NodeNats.Publish(reply, null, EncodeFailure(response, ex));
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        private void OnBind(string msg, string reply, string subject)
        {
           Logger.Debug(Strings.BindRequestLogMessage, ServiceDescription(), msg, reply);
            BindResponse response = new BindResponse();
            try
            {
                BindRequest bind_message = new BindRequest();
                bind_message.FromJsonIntermediateObject(JsonConvertibleObject.DeserializeFromJson(msg));
                string name = bind_message.Name;
                Dictionary<string, object> bind_opts = bind_message.BindOptions;
                ServiceCredentials credentials = bind_message.Credentials;
                response.Credentials = Bind(name, bind_opts, credentials);
                NodeNats.Publish(reply, null, EncodeSuccess(response));
            }
            catch (Exception ex)
            {
                Logger.Warning(ex.ToString());
                NodeNats.Publish(reply, null, EncodeFailure(response, ex));
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        private void OnUnbind(string msg, string reply, string subject)
        {
            Logger.Debug(Strings.UnbindRequestDebugLogMessage, ServiceDescription(), msg, reply);
            SimpleResponse response = new SimpleResponse();
            try
            {
                UnbindRequest unbind_req = new UnbindRequest();
                unbind_req.FromJsonIntermediateObject(JsonConvertibleObject.DeserializeFromJson(msg));

                bool result = Unbind(unbind_req.Credentials);

                if (result)
                {
                    NodeNats.Publish(reply, null, EncodeSuccess(response));
                }
                else
                {
                    NodeNats.Publish(reply, null, EncodeFailure(response));
                }
            }
            catch (Exception ex)
            {
                Logger.Warning(ex.ToString());
                NodeNats.Publish(reply, null, EncodeFailure(response, ex));
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        private void OnRestore(string msg, string reply, string subject)
        {
            Logger.Debug(Strings.OnRestoreDebugLogMessage, ServiceDescription(), msg, reply);
            SimpleResponse response = new SimpleResponse();
            try
            {
                RestoreRequest restore_message = new RestoreRequest();
                restore_message.FromJsonIntermediateObject(JsonConvertibleObject.DeserializeFromJson(msg));
                string instance_id = restore_message.InstanceId;
                string backup_path = restore_message.BackupPath;

                bool result = Restore(instance_id, backup_path);
                if (result)
                {
                    NodeNats.Publish(reply, null, EncodeSuccess(response));
                }
                else
                {
                    NodeNats.Publish(reply, null, EncodeFailure(response));
                }
            }
            catch (Exception ex)
            {
                Logger.Warning(ex.ToString());
                NodeNats.Publish(reply, null, EncodeFailure(response, ex));
            }
        }

        // disable and dump instance
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        private void OnDisableInstance(string msg, string reply, string subject)
        {
           Logger.Debug(Strings.OnDisableInstanceDebugLogMessage, ServiceDescription(), msg, reply);
            try
            {
                object[] credentials = new object[0];
                credentials = JsonConvertibleObject.DeserializeFromJsonArray(msg);

                ServiceCredentials prov_cred = new ServiceCredentials();
                ServiceCredentials binding_creds = new ServiceCredentials();

                prov_cred.FromJsonIntermediateObject(credentials[0]);
                binding_creds.FromJsonIntermediateObject(credentials[1]);


                string instance = prov_cred.Name;
                string file_path = GetMigrationFolder(instance);

                Directory.CreateDirectory(file_path);

                bool result = DisableInstance(prov_cred, binding_creds);

                if (result)
                {
                    result = DumpInstance(prov_cred, binding_creds, file_path);
                }
                NodeNats.Publish(reply, null, result.ToString());
            }
            catch (Exception ex)
            {
                Logger.Warning(ex.ToString());
            }
        }

        // enable instance and send updated credentials back
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        private void OnEnableInstance(string msg, string reply, string subject)
        {
            Logger.Debug(Strings.OnEnableInstanceDebugMessage, ServiceDescription(), msg, reply);
            try
            {
                object[] credentials = new object[0];
                credentials = JsonConvertibleObject.DeserializeFromJsonArray(msg);

                ServiceCredentials prov_cred = new ServiceCredentials();
                Dictionary<string, object> binding_creds_hash = new Dictionary<string, object>();

                prov_cred.FromJsonIntermediateObject(credentials[0]);
                binding_creds_hash = JsonConvertibleObject.ObjectToValue<Dictionary<string, object>>(credentials[1]);

                EnableInstance(ref prov_cred, ref binding_creds_hash);

                // Update node_id in provision credentials..
                prov_cred.NodeId = nodeId;
                credentials[0] = prov_cred;
                credentials[1] = binding_creds_hash;
                NodeNats.Publish(reply, null, JsonConvertibleObject.SerializeToJson(credentials));
            }
            catch (Exception ex)
            {
                Logger.Warning(ex.ToString());
            }
        }

        // Cleanup nfs folder which contains migration data
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        private void OnCleanupNfs(string msg, string reply, string subject)
        {
           Logger.Debug(Strings.CleanupNfsLogMessage, ServiceDescription(), msg, reply);
            try
            {
                object[] request = new object[0];
                request = JsonConvertibleObject.DeserializeFromJsonArray(msg);
                ServiceCredentials prov_cred = new ServiceCredentials();
                ServiceCredentials binding_creds = new ServiceCredentials();
                prov_cred.FromJsonIntermediateObject(request[0]);
                binding_creds.FromJsonIntermediateObject(request[1]);

                string instance = prov_cred.Name;
                Directory.Delete(GetMigrationFolder(instance), true);

                NodeNats.Publish(reply, null, "true");
            }
            catch (Exception ex)
            {
                Logger.Warning(ex.ToString());
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        private void OnCheckOrphan(string msg, string reply, string subject)
        {
           Logger.Debug(Strings.CheckOrphanLogMessage, ServiceDescription());

            CheckOrphanResponse response = new CheckOrphanResponse();
            try
            {
                CheckOrphanRequest request = new CheckOrphanRequest();
                request.FromJsonIntermediateObject(JsonConvertibleObject.DeserializeFromJson(msg));
                CheckOrphan(request.Handles);

                response.OrphanInstances = OrphanInstancesHash;
                response.OrphanBindings = OrphanBindingHash;
                response.Success = true;
            }
            catch (Exception ex)
            {
                Logger.Warning(Strings.CheckOrphanExceptionLogMessage, ex.ToString());
                response.Success = false;
                response.Error = new Dictionary<string, object>() 
                {
                    {"message", ex.Message},
                    {"stack", ex.StackTrace}
                };
            }
            finally
            {
                NodeNats.Publish(String.Format(CultureInfo.InvariantCulture, Strings.NatsSubjectOrphanResult, ServiceName()), null, response.SerializeToJson());
            }
        }

        private void CheckOrphan(Handle[] handles)
        {
            if (handles == null)
            {
                throw new ServiceException(ServiceException.NotFound, "No handles for checking orphan");
            }

            string[] live_ins_list = AllInstancesList();

            Dictionary<string, object> orphan_ins_hash = new Dictionary<string, object>();

            List<string> oi_list = new List<string>();

            foreach (string name in live_ins_list)
            {
                if (!handles.Any(h => h.Credentials.NodeId == nodeId && h.ServiceId == name))
                {
                    oi_list.Add(name);
                }
            }

            ServiceCredentials[] live_bind_list = AllBindingsList();
            Dictionary<string, object> orphan_binding_hash = new Dictionary<string, object>();

            List<ServiceCredentials> ob_list = new List<ServiceCredentials>();

            foreach (ServiceCredentials credential in live_bind_list)
            {
                if (!handles.Any(h => h.Credentials.Name == credential.Name && h.Credentials.UserName == credential.UserName))
                {
                    ob_list.Add(credential);
                }
            }

            Logger.Debug(Strings.CheckOrphanDebugLogMessage, oi_list.Count, ob_list.Count);
            orphan_ins_hash[nodeId.ToString()] = oi_list;
            orphan_binding_hash[nodeId.ToString()] = ob_list;
            this.OrphanInstancesHash = orphan_ins_hash;
            this.OrphanBindingHash = orphan_binding_hash;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        private void OnPurgeOrphan(string msg, string reply, string subject)
        {
            Logger.Debug(Strings.OnPurgeOrphanDebugLogMessage, ServiceDescription());
            SimpleResponse response = new SimpleResponse();
            try
            {
                PurgeOrphanRequest request = new PurgeOrphanRequest();
                request.FromJsonIntermediateObject(JsonConvertibleObject.DeserializeFromJson(msg));

                bool result = PurgeOrphan(request.OrphanInsList, request.OrphanBindingList);
                if (result)
                {
                    NodeNats.Publish(reply, null, EncodeSuccess(response));
                }
                else
                {
                    NodeNats.Publish(reply, null, EncodeFailure(response));
                }
            }
            catch (Exception ex)
            {
                Logger.Warning(ex.Message);
                NodeNats.Publish(reply, null, EncodeFailure(response, ex));
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        private bool PurgeOrphan(string[] oi_list, ServiceCredentials[] ob_list)
        {
            bool ret = true;
            ServiceCredentials[] ab_list = AllBindingsList();

            foreach (string ins in oi_list)
            {
                try
                {
                    ServiceCredentials[] bindings = ab_list.Where(b => b.Name == ins).ToArray();
                    Logger.Debug(Strings.PurgeOrphanDebugLogMessage, ins, String.Join(", ", JsonConvertibleObject.SerializeToJson(bindings)));

                    ret = ret && Unprovision(ins, bindings);

                    // Remove the OBs that are unbinded by unprovision
                    ob_list = (from ob in ob_list
                               where !bindings.Any(binding => binding.Name == ob.Name)
                               select ob).ToArray();
                }
                catch (Exception ex)
                {
                   Logger.Debug(Strings.PurgeOrphanErrorLogMessage, ins, ex.Message);
                }
            }

            foreach (ServiceCredentials credential in ob_list)
            {
                try
                {
                    Logger.Debug(Strings.PurgeOrphanUnbindBindingDebugLogMessage, credential.SerializeToJson());
                    ret = ret && Unbind(credential);
                }
                catch (Exception ex)
                {
                    Logger.Debug(Strings.PurgeOrphanUnbindBindingErrorLogMessage, credential.SerializeToJson(), ex.ToString());
                }
            }
            return ret;
        }

        /// <summary>
        /// Subclass must overwrite this method to enable check orphan instance feature.
        /// Otherwise it will not check orphan instance
        /// </summary>
        /// <returns>The return value should be a list of instance names.</returns>
        protected virtual string[] AllInstancesList()
        {
            return new string[0];
        }

        /// <summary>
        /// Subclass must overwrite this method to enable check orphan binding feature.
        /// Otherwise it will not check orphan bindings
        /// </summary>
        /// <returns>The return value should be a list of binding credentials</returns>
        protected virtual ServiceCredentials[] AllBindingsList()
        {
            return new ServiceCredentials[0];
        }

        /// <summary>
        /// Get the tmp folder for migration
        /// </summary>
        /// <param name="instance">Instance name.</param>
        /// <returns>A string containing the temp folder.</returns>
        private string GetMigrationFolder(string instance)
        {
            return Path.Combine(migrationNfs, "migration", ServiceName(), instance);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        private void OnImportInstance(string msg, string reply, string subject)
        {
            Logger.Debug(Strings.OnImportInstanceDebugLogMessage, ServiceDescription(), msg, reply);
            try
            {
                object[] credentials = new object[0];
                credentials = JsonConvertibleObject.DeserializeFromJsonArray(msg);

                ProvisionedServicePlanType plan = (ProvisionedServicePlanType)Enum.Parse(typeof(ProvisionedServicePlanType), JsonConvertibleObject.ObjectToValue<string>(credentials[0]));

                ServiceCredentials prov_cred = new ServiceCredentials();
                ServiceCredentials binding_creds = new ServiceCredentials();

                prov_cred.FromJsonIntermediateObject(credentials[1]);
                binding_creds.FromJsonIntermediateObject(credentials[2]);

                string instance = prov_cred.Name;
                string file_path = GetMigrationFolder(instance);

                bool result = ImportInstance(prov_cred, binding_creds, file_path, plan);
                NodeNats.Publish(reply, null, result.ToString());
            }
            catch (Exception ex)
            {
                Logger.Warning(ex.ToString());
            }
        }

        private void OnDiscover(string msg, string reply, string subject)
        {
            SendNodeAnnouncement(reply);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        private void SendNodeAnnouncement(string reply = null)
        {
            try
            {
                if (!NodeReady())
                {
                    Logger.Debug(Strings.SendNodeAnnouncementNotReadyDebugLogMessage, ServiceDescription());
                    return;
                }

                Logger.Debug(Strings.SendNodeAnnouncementDebugLogMessage, ServiceDescription(), reply != null ? reply : "everyone");

                Announcement a = AnnouncementDetails;
                a.Id = nodeId;
                NodeNats.Publish(reply != null ? reply : String.Format(CultureInfo.InvariantCulture, Strings.NatsSubjectAnnounce, ServiceName()), null, a.SerializeToJson());
            }
            catch (Exception ex)
            {
                Logger.Warning(ex.ToString());
            }
        }

        /// <summary>
        /// Service Node subclasses can override this method if they depend
        /// on some external service in order to operate; for example, MySQL
        /// and Postgresql require a connection to the underlying server.
        /// </summary>
        /// <returns>True if node is ready.</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic")]
        protected bool NodeReady()
        {
            return true;
        }

        /// <summary>
        /// Service Node subclasses may want to override this method to
        /// provide service specific data beyond what is returned by their
        /// "announcement" method.
        /// </summary>
        /// <returns>A dictionary containing varz details.</returns>
        protected override Dictionary<string, object> VarzDetails()
        {
            return AnnouncementDetails.ToJsonIntermediateObject();
        }

        /// <summary>
        /// Service Node subclasses may want to override this method to
        /// provide service specific data
        /// </summary>
        /// <returns>A dictionary containing healthz details.</returns>
        protected override Dictionary<string, string> HealthzDetails()
        {
            return new Dictionary<string, string>() 
            {
                {"self", "ok"}
            };
        }

        private static string EncodeSuccess(MessageWithSuccessStatus response)
        {
            response.Success = true;
            return response.SerializeToJson();
        }

        private static string EncodeFailure(MessageWithSuccessStatus response, Exception error = null)
        {
            response.Success = false;
            if (error == null || !(error is ServiceException))
            {
              error = new ServiceException(ServiceException.InternalError);
            }
            response.Error = ((ServiceException)error).ToDictionary();
            return response.SerializeToJson();
        }

        /// <summary>
        /// Subclasses have to implement this in order to provision services.
        /// </summary>
        /// <param name="plan">The payment plan for the service.</param>
        /// <returns>Credentials for the provisioned service.</returns>
        protected abstract ServiceCredentials Provision(ProvisionedServicePlanType plan);
        /// <summary>
        /// Subclasses have to implement this in order to provision services.
        /// </summary>
        /// <param name="plan">The payment plan for the service.</param>
        /// <param name="credentials">Existing credentials for the service.</param>
        /// <returns>Credentials for the provisioned service.</returns>
        protected abstract ServiceCredentials Provision(ProvisionedServicePlanType plan, ServiceCredentials credentials);

        /// <summary>
        /// Subclasses have to implement this in order to unprovision services.
        /// </summary>
        /// <param name="name">The name of the service to unprovision.</param>
        /// <param name="bindings">Array of bindings for the service that have to be unprovisioned.</param>
        /// <returns>A boolean specifying whether the unprovision request was successful.</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Unprovision")]
        protected abstract bool Unprovision(string name, ServiceCredentials[] bindings);

        /// <summary>
        /// Subclasses have to implement this in order to bind a provisioned service to an app.
        /// </summary>
        /// <param name="name">The name of the service.</param>
        /// <param name="bindOptions">Binding options.</param>
        /// <param name="credentials">Already existing credentials.</param>
        /// <returns>A new set of credentials used for binding.</returns>
        protected abstract ServiceCredentials Bind(string name, Dictionary<string, object> bindOptions, ServiceCredentials credentials);

        /// <summary>
        /// Subclasses have to implement this in order to bind a provisioned service to an app.
        /// </summary>
        /// <param name="name">The name of the service.</param>
        /// <param name="bindOptions">Binding options.</param>
        /// <returns>A new set of credentials used for binding.</returns>
        protected abstract ServiceCredentials Bind(string name, Dictionary<string, object> bindOptions);

        /// <summary>
        /// Subclasses have to implement this in order to unbind a service from an app.
        /// </summary>
        /// <param name="credentials">The credentials that have to be unprovisioned.</param>
        /// <returns>A bool indicating whether the unbind request was successful.</returns>
        protected abstract bool Unbind(ServiceCredentials credentials);

        /// <summary>
        /// Gets any service-specific announcement details.
        /// </summary>
        protected abstract Announcement AnnouncementDetails
        {
            get;
        }

        /// <summary>
        /// Subclasses have to implement this in order to disable an instance.
        /// </summary>
        /// <param name="provisionedCredential">The provisioned credentials.</param>
        /// <param name="bindingCredentials">The binding credentials.</param>
        /// <returns>A bool indicating whether the request was successful.</returns>
        protected abstract bool DisableInstance(ServiceCredentials provisionedCredential, ServiceCredentials bindingCredentials);
        /// <summary>
        /// Subclasses have to implement this in order to dump an instance.
        /// </summary>
        /// <param name="provisionedCredential">The provisioned credential.</param>
        /// <param name="bindingCredentials">The binding credentials.</param>
        /// <param name="filePath">The file path where to dump the service.</param>
        /// <returns>A bool indicating whether the request was successful.</returns>
        protected abstract bool DumpInstance(ServiceCredentials provisionedCredential, ServiceCredentials bindingCredentials, string filePath);
        /// <summary>
        /// Subclasses have to implement this in order to import an instance.
        /// </summary>
        /// <param name="provisionedCredential">The provisioned credential.</param>
        /// <param name="bindingCredentials">The binding credentials.</param>
        /// <param name="filePath">The file path from which to import the service.</param>
        /// <param name="plan">The payment plan.</param>
        /// <returns>A bool indicating whether the request was successful.</returns>
        protected abstract bool ImportInstance(ServiceCredentials provisionedCredential, ServiceCredentials bindingCredentials, string filePath, ProvisionedServicePlanType plan);
        /// <summary>
        /// Enables the instance.
        /// </summary>
        /// <param name="provisionedCredential">The provisioned credential.</param>
        /// <param name="bindingCredentialsHash">The binding credentials hash.</param>
        /// <returns>A bool indicating whether the request was successful.</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1045:DoNotPassTypesByReference")]
        protected abstract bool EnableInstance(ref ServiceCredentials provisionedCredential, ref Dictionary<string, object> bindingCredentialsHash);
        /// <summary>
        /// Restores the specified instance id.
        /// </summary>
        /// <param name="instanceId">The instance id.</param>
        /// <param name="backupPath">The backup path.</param>
        /// <returns>A bool indicating whether the request was successful.</returns>
        protected abstract bool Restore(string instanceId, string backupPath);

    }
}