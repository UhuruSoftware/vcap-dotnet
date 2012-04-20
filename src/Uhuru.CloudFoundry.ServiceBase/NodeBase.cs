// -----------------------------------------------------------------------
// <copyright file="NodeBase.cs" company="Uhuru Software, Inc.">
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
    using System.Threading;
    using Uhuru.NatsClient;
    using Uhuru.Utilities;
    using Uhuru.Utilities.Json;

    /// <summary>
    /// This is the base class for all Cloud Foundry system services nodes.
    /// </summary>
    public abstract class NodeBase : SystemServiceBase
    {
        /// <summary>
        /// The node ID.
        /// </summary>
        private string nodeId;

        /// <summary>
        /// The service plan.
        /// </summary>
        private string plan;

        /// <summary>
        /// Migration Network File System.
        /// </summary>
        private string migrationNfs;

        /// <summary>
        /// Gets any service-specific announcement details.
        /// </summary>
        protected abstract Announcement AnnouncementDetails
        {
            get;
        }

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
            this.plan = options.Plan;
            this.migrationNfs = options.MigrationNFS;
            base.Start(options);
        }

        /// <summary>
        /// Gets the flavor of this service (only node for .Net)
        /// </summary>
        /// <returns>The value "Node"</returns>
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

            NodeNats.Subscribe(string.Format(CultureInfo.InvariantCulture, Strings.NatsSubjectProvision, this.ServiceName(), this.nodeId), new SubscribeCallback(this.OnProvision));
            NodeNats.Subscribe(string.Format(CultureInfo.InvariantCulture, Strings.NatsSubjectUnprovision, this.ServiceName(), this.nodeId), new SubscribeCallback(this.OnUnprovision));
            NodeNats.Subscribe(string.Format(CultureInfo.InvariantCulture, Strings.NatsSubjectBind, this.ServiceName(), this.nodeId), new SubscribeCallback(this.OnBind));
            NodeNats.Subscribe(string.Format(CultureInfo.InvariantCulture, Strings.NatsSubjectUnbind, this.ServiceName(), this.nodeId), new SubscribeCallback(this.OnUnbind));
            NodeNats.Subscribe(string.Format(CultureInfo.InvariantCulture, Strings.NatsSubjectRestore, this.ServiceName(), this.nodeId), new SubscribeCallback(this.OnRestore));

            NodeNats.Subscribe(string.Format(CultureInfo.InvariantCulture, Strings.NatsSubjectDiscover, this.ServiceName()), new SubscribeCallback(this.OnDiscover));

            NodeNats.Subscribe(string.Format(CultureInfo.InvariantCulture, Strings.NatsSubjectDisableInstance, this.ServiceName(), this.nodeId), new SubscribeCallback(this.OnDisableInstance));
            NodeNats.Subscribe(string.Format(CultureInfo.InvariantCulture, Strings.NatsSubjectEnableInstance, this.ServiceName(), this.nodeId), new SubscribeCallback(this.OnEnableInstance));
            NodeNats.Subscribe(string.Format(CultureInfo.InvariantCulture, Strings.NatsSubjectImportInstance, this.ServiceName(), this.nodeId), new SubscribeCallback(this.OnImportInstance));
            NodeNats.Subscribe(string.Format(CultureInfo.InvariantCulture, Strings.NatsSubjectCleanupNfs, this.ServiceName(), this.nodeId), new SubscribeCallback(this.OnCleanupNfs));

            NodeNats.Subscribe(string.Format(CultureInfo.InvariantCulture, Strings.NatsSubjectCheckOrphan, this.ServiceName()), new SubscribeCallback(this.OnCheckOrphan));
            NodeNats.Subscribe(string.Format(CultureInfo.InvariantCulture, Strings.NatsSubjectPurgeOrphan, this.ServiceName(), this.nodeId), new SubscribeCallback(this.OnPurgeOrphan));

            SendNodeAnnouncement();

            TimerHelper.RecurringCall(
                30000,
                delegate
                {
                    SendNodeAnnouncement();
                });
        }

        /// <summary>
        /// Get capacity unit.
        /// </summary>
        /// <returns>Capacity unit.</returns>
        protected virtual int CapacityUnit()
        {
            return 1;
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
        /// Service Node subclasses can override this method if they depend
        /// on some external service in order to operate; for example, MySQL
        /// and Postgresql require a connection to the underlying server.
        /// </summary>
        /// <returns>True if node is ready.</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Descendant classes may override this and use instance members.")]
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
            return this.AnnouncementDetails.ToJsonIntermediateObject();
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
                { "self", "ok" }
            };
        }

        /// <summary>
        /// Subclasses have to implement this in order to provision services.
        /// </summary>
        /// <param name="planRequest">The payment plan for the service.</param>
        /// <returns>Credentials for the provisioned service.</returns>
        protected abstract ServiceCredentials Provision(ProvisionedServicePlanType planRequest);

        /// <summary>
        /// Subclasses have to implement this in order to provision services.
        /// </summary>
        /// <param name="planRequest">The payment plan for the service.</param>
        /// <param name="credentials">Existing credentials for the service.</param>
        /// <returns>Credentials for the provisioned service.</returns>
        protected abstract ServiceCredentials Provision(ProvisionedServicePlanType planRequest, ServiceCredentials credentials);

        /// <summary>
        /// Subclasses have to implement this in order to unprovision services.
        /// </summary>
        /// <param name="name">The name of the service to unprovision.</param>
        /// <param name="bindings">Array of bindings for the service that have to be unprovisioned.</param>
        /// <returns>A boolean specifying whether the unprovision request was successful.</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Unprovision", Justification = "Word is in dictionary, but warning is still generated.")]
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
        /// <param name="planRequest">The payment plan.</param>
        /// <returns>A bool indicating whether the request was successful.</returns>
        protected abstract bool ImportInstance(ServiceCredentials provisionedCredential, ServiceCredentials bindingCredentials, string filePath, ProvisionedServicePlanType planRequest);

        /// <summary>
        /// Enables the instance.
        /// </summary>
        /// <param name="provisionedCredential">The provisioned credential.</param>
        /// <param name="bindingCredentialsHash">The binding credentials hash.</param>
        /// <returns>A bool indicating whether the request was successful.</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1045:DoNotPassTypesByReference", Justification = "This is the most elegant way to achieve what we need")]
        protected abstract bool EnableInstance(ref ServiceCredentials provisionedCredential, ref Dictionary<string, object> bindingCredentialsHash);

        /// <summary>
        /// Restores the specified instance id.
        /// </summary>
        /// <param name="instanceId">The instance id.</param>
        /// <param name="backupPath">The backup path.</param>
        /// <returns>A bool indicating whether the request was successful.</returns>
        protected abstract bool Restore(string instanceId, string backupPath);

        /// <summary>
        /// Encodes a successful status of an operation.
        /// </summary>
        /// <param name="response">The response message, with the success property set to true.</param>
        /// <returns>The response message serialized to a Json string.</returns>
        private static string EncodeSuccess(MessageWithSuccessStatus response)
        {
            response.Success = true;
            return response.SerializeToJson();
        }

        /// <summary>
        /// Encodes an unsuccessful status of an operation.
        /// </summary>
        /// <param name="response">The response message, with the success property set to false, and the exception set to the error property.</param>
        /// <param name="error">The inner exception that was raised.</param>
        /// <returns>The response message serialized to a Json string.</returns>   
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
        /// Called when a provision request is received.
        /// </summary>
        /// <param name="msg">The message payload.</param>
        /// <param name="reply">The reply to setting.</param>
        /// <param name="subject">The subject of the message.</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "We log everything that happens, any provision request error should not bubble up.")]
        private void OnProvision(string msg, string reply, string subject)
        {
            Logger.Debug(Strings.OnProvisionRequestDebugLogMessage, ServiceDescription(), msg, reply);
            ProvisionResponse response = new ProvisionResponse();
            ProvisionRequest provision_req = new ProvisionRequest();

            provision_req.FromJsonIntermediateObject(JsonConvertibleObject.DeserializeFromJson(msg));

            ProvisionedServicePlanType plan = provision_req.Plan;
            ServiceCredentials credentials = provision_req.Credentials;

            if (credentials == null)
            {
                credentials = GenerateCredentials();
            }

            credentials.ServiceWorkFactory.StartNew(
                () =>
                {
                    try
                    {
                        ServiceCredentials credential = Provision(plan, credentials);

                        credential.NodeId = this.nodeId;

                        response.Credentials = credential;

                        Logger.Debug(
                            Strings.OnProvisionSuccessDebugLogMessage,
                            ServiceDescription(),
                            msg,
                            response.SerializeToJson());

                        NodeNats.Publish(reply, null, EncodeSuccess(response));
                    }
                    catch (Exception ex)
                    {
                        Logger.Warning(ex.ToString());
                        NodeNats.Publish(reply, null, EncodeFailure(response));
                    }
                });
        }

        /// <summary>
        /// Called when an unprovision request is received.
        /// </summary>
        /// <param name="msg">The message payload.</param>
        /// <param name="reply">The reply to setting.</param>
        /// <param name="subject">The subject of the message.</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "The exception is logged; errors in this request must not bubble up.")]
        private void OnUnprovision(string msg, string reply, string subject)
        {
            Logger.Debug(Strings.UnprovisionRequestDebugLogMessage, ServiceDescription(), msg);

            SimpleResponse response = new SimpleResponse();

            UnprovisionRequest unprovision_req = new UnprovisionRequest();
            unprovision_req.FromJsonIntermediateObject(JsonConvertibleObject.DeserializeFromJson(msg));
            string name = unprovision_req.Name;

            // Create a service credentials object with a name set, so we can run this operation in parallel for different service instances.
            ServiceCredentials credentials = new ServiceCredentials();
            credentials.Name = name;

            credentials.ServiceWorkFactory.StartNew(
                () =>
                {
                    try
                    {
                        ServiceCredentials[] bindings = unprovision_req.Bindings;

                        bool result = this.Unprovision(name, bindings);

                        if (result)
                        {
                            this.NodeNats.Publish(reply, null, EncodeSuccess(response));
                        }
                        else
                        {
                            this.NodeNats.Publish(reply, null, EncodeFailure(response));
                        }
                    }
                    catch (Exception ex)
                    {
                        Logger.Warning(ex.ToString());
                        NodeNats.Publish(reply, null, EncodeFailure(response, ex));
                    }
                });
        }

        /// <summary>
        /// Called when a bind request is received.
        /// </summary>
        /// <param name="msg">The message payload.</param>
        /// <param name="reply">The reply to setting.</param>
        /// <param name="subject">The subject of the message.</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "The exception is logged; errors in this request must not bubble up.")]
        private void OnBind(string msg, string reply, string subject)
        {
            Logger.Debug(Strings.BindRequestLogMessage, ServiceDescription(), msg, reply);
            BindResponse response = new BindResponse();
            BindRequest bind_message = new BindRequest();
            bind_message.FromJsonIntermediateObject(JsonConvertibleObject.DeserializeFromJson(msg));
            string name = bind_message.Name;

            // Create a service credentials object with a name set, so we can run this operation in parallel for different service instances.
            ServiceCredentials nameCredentials = new ServiceCredentials();
            nameCredentials.Name = name;

            nameCredentials.ServiceWorkFactory.StartNew(
                () =>
                {
                    try
                    {
                        Dictionary<string, object> bind_opts = bind_message.BindOptions;
                        ServiceCredentials credentials = bind_message.Credentials;
                        response.Credentials = this.Bind(name, bind_opts, credentials);
                        NodeNats.Publish(reply, null, EncodeSuccess(response));
                    }
                    catch (Exception ex)
                    {
                        Logger.Warning(ex.ToString());
                        NodeNats.Publish(reply, null, EncodeFailure(response, ex));
                    }
                });
        }

        /// <summary>
        /// Called when an unbind request is received.
        /// </summary>
        /// <param name="msg">The message payload.</param>
        /// <param name="reply">The reply to setting.</param>
        /// <param name="subject">The subject of the message.</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "The exception is logged; errors in this request must not bubble up.")]
        private void OnUnbind(string msg, string reply, string subject)
        {
            Logger.Debug(Strings.UnbindRequestDebugLogMessage, ServiceDescription(), msg, reply);
            SimpleResponse response = new SimpleResponse();
            UnbindRequest unbind_req = new UnbindRequest();
            unbind_req.FromJsonIntermediateObject(JsonConvertibleObject.DeserializeFromJson(msg));

            unbind_req.Credentials.ServiceWorkFactory.StartNew(
                () =>
                {
                    try
                    {
                        bool result = this.Unbind(unbind_req.Credentials);

                        if (result)
                        {
                            this.NodeNats.Publish(reply, null, EncodeSuccess(response));
                        }
                        else
                        {
                            this.NodeNats.Publish(reply, null, EncodeFailure(response));
                        }
                    }
                    catch (Exception ex)
                    {
                        Logger.Warning(ex.ToString());
                        NodeNats.Publish(reply, null, EncodeFailure(response, ex));
                    }
                });
        }

        /// <summary>
        /// Called when a restore request is received.
        /// </summary>
        /// <param name="msg">The message payload.</param>
        /// <param name="reply">The reply to setting.</param>
        /// <param name="subject">The subject of the message.</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "The exception is logged; errors in this request must not bubble up.")]
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

                // TODO: vladi: need to make this parallel when we implement the actual restore for mssql
                bool result = this.Restore(instance_id, backup_path);
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

        /// <summary>
        /// Called when a disable instance request is received.
        /// </summary>
        /// <param name="msg">The message payload.</param>
        /// <param name="reply">The reply to setting.</param>
        /// <param name="subject">The subject of the message.</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "The exception is logged; errors in this request must not bubble up.")]
        private void OnDisableInstance(string msg, string reply, string subject)
        {
            Logger.Debug(Strings.OnDisableInstanceDebugLogMessage, ServiceDescription(), msg, reply);
            object[] credentials = new object[0];
            credentials = JsonConvertibleObject.DeserializeFromJsonArray(msg);

            ServiceCredentials prov_cred = new ServiceCredentials();
            ServiceCredentials binding_creds = new ServiceCredentials();

            prov_cred.FromJsonIntermediateObject(credentials[0]);
            binding_creds.FromJsonIntermediateObject(credentials[1]);

            prov_cred.ServiceWorkFactory.StartNew(
                () =>
                {
                    try
                    {
                        string instance = prov_cred.Name;
                        string file_path = this.GetMigrationFolder(instance);

                        Directory.CreateDirectory(file_path);

                        bool result = this.DisableInstance(prov_cred, binding_creds);

                        if (result)
                        {
                            result = this.DumpInstance(prov_cred, binding_creds, file_path);
                        }

                        NodeNats.Publish(reply, null, result.ToString());
                    }
                    catch (Exception ex)
                    {
                        Logger.Warning(ex.ToString());
                    }
                });
        }

        /// <summary>
        /// Called when an enable instance request is received.
        /// Enables an instance and sends updated credentials back.
        /// </summary>
        /// <param name="msg">The message payload.</param>
        /// <param name="reply">The reply to setting.</param>
        /// <param name="subject">The subject of the message.</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "The exception is logged; errors in this request must not bubble up.")]
        private void OnEnableInstance(string msg, string reply, string subject)
        {
            Logger.Debug(Strings.OnEnableInstanceDebugMessage, ServiceDescription(), msg, reply);
            object[] credentials = new object[0];
            credentials = JsonConvertibleObject.DeserializeFromJsonArray(msg);

            ServiceCredentials prov_cred = new ServiceCredentials();
            Dictionary<string, object> binding_creds_hash = new Dictionary<string, object>();

            prov_cred.FromJsonIntermediateObject(credentials[0]);
            binding_creds_hash = JsonConvertibleObject.ObjectToValue<Dictionary<string, object>>(credentials[1]);

            prov_cred.ServiceWorkFactory.StartNew(
                () =>
                {
                    try
                    {
                        this.EnableInstance(ref prov_cred, ref binding_creds_hash);

                        // Update node_id in provision credentials..
                        prov_cred.NodeId = this.nodeId;
                        credentials[0] = prov_cred;
                        credentials[1] = binding_creds_hash;
                        NodeNats.Publish(reply, null, JsonConvertibleObject.SerializeToJson(credentials));
                    }
                    catch (Exception ex)
                    {
                        Logger.Warning(ex.ToString());
                    }
                });
        }

        /// <summary>
        /// Called when a cleanup NFS request is received.
        /// Cleanup nfs folder which contains migration data.
        /// </summary>
        /// <param name="msg">The message payload.</param>
        /// <param name="reply">The reply to setting.</param>
        /// <param name="subject">The subject of the message.</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "The exception is logged; errors in this request must not bubble up.")]
        private void OnCleanupNfs(string msg, string reply, string subject)
        {
            Logger.Debug(Strings.CleanupNfsLogMessage, ServiceDescription(), msg, reply);
            object[] request = new object[0];
            request = JsonConvertibleObject.DeserializeFromJsonArray(msg);
            ServiceCredentials prov_cred = new ServiceCredentials();
            ServiceCredentials binding_creds = new ServiceCredentials();
            prov_cred.FromJsonIntermediateObject(request[0]);
            binding_creds.FromJsonIntermediateObject(request[1]);

            prov_cred.ServiceWorkFactory.StartNew(
                () =>
                {
                    try
                    {
                        string instance = prov_cred.Name;
                        Directory.Delete(this.GetMigrationFolder(instance), true);

                        NodeNats.Publish(reply, null, "true");
                    }
                    catch (Exception ex)
                    {
                        Logger.Warning(ex.ToString());
                    }
                });
        }

        /// <summary>
        /// Called when a check orphan request is received.
        /// </summary>
        /// <param name="msg">The message payload.</param>
        /// <param name="reply">The reply to setting.</param>
        /// <param name="subject">The subject of the message.</param>         
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "The exception is logged; errors in this request must not bubble up.")]
        private void OnCheckOrphan(string msg, string reply, string subject)
        {
            Logger.Debug(Strings.CheckOrphanLogMessage, ServiceDescription());

            CheckOrphanResponse response = new CheckOrphanResponse();
            try
            {
                CheckOrphanRequest request = new CheckOrphanRequest();
                request.FromJsonIntermediateObject(JsonConvertibleObject.DeserializeFromJson(msg));

                // TODO: vladi: investigate further if this needs to be parallelized; seems that this would take a very short time, and it's not tied to a specific instance, so it could run on a new thread
                this.CheckOrphan(request.Handles);

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
                    { "message", ex.Message },
                    { "stack", ex.StackTrace }
                };
            }
            finally
            {
                NodeNats.Publish(string.Format(CultureInfo.InvariantCulture, Strings.NatsSubjectOrphanResult, ServiceName()), null, response.SerializeToJson());
            }
        }

        /// <summary>
        /// Checks the specified service handles to see if they're orphaned.
        /// </summary>
        /// <param name="handles">The service handles (service ids and credentials).</param>
        private void CheckOrphan(Handle[] handles)
        {
            if (handles == null)
            {
                throw new ServiceException(ServiceException.NotFound, "No handles for checking orphan");
            }

            string[] live_ins_list = this.AllInstancesList();

            Dictionary<string, object> orphanInstancesHash = new Dictionary<string, object>();

            List<string> orphanInstancesList = new List<string>();

            foreach (string name in live_ins_list)
            {
                if (!handles.Any(h => h.Credentials.NodeId == this.nodeId && h.ServiceId == name))
                {
                    orphanInstancesList.Add(name);
                }
            }

            ServiceCredentials[] liveBindList = this.AllBindingsList();
            Dictionary<string, object> orphanBindingHash = new Dictionary<string, object>();

            List<ServiceCredentials> orphanBindingsList = new List<ServiceCredentials>();

            foreach (ServiceCredentials credential in liveBindList)
            {
                if (!handles.Any(h => h.Credentials.Name == credential.Name && h.Credentials.UserName == credential.UserName))
                {
                    orphanBindingsList.Add(credential);
                }
            }

            Logger.Debug(Strings.CheckOrphanDebugLogMessage, orphanInstancesList.Count, orphanBindingsList.Count);
            orphanInstancesHash[this.nodeId.ToString()] = orphanInstancesList;
            orphanBindingHash[this.nodeId.ToString()] = orphanBindingsList;
            this.OrphanInstancesHash = orphanInstancesHash;
            this.OrphanBindingHash = orphanBindingHash;
        }

        /// <summary>
        /// Called when a purge orphan request is received.
        /// </summary>
        /// <param name="msg">The message payload.</param>
        /// <param name="reply">The reply to setting.</param>
        /// <param name="subject">The subject of the message.</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "The exception is logged; errors in this request must not bubble up.")]
        private void OnPurgeOrphan(string msg, string reply, string subject)
        {
            // This may take a long time (it can unbind many services), so we run it on a different thread;
            ThreadPool.QueueUserWorkItem(
                (data) =>
                {
                    Logger.Debug(Strings.OnPurgeOrphanDebugLogMessage, ServiceDescription());
                    SimpleResponse response = new SimpleResponse();
                    try
                    {
                        PurgeOrphanRequest request = new PurgeOrphanRequest();
                        request.FromJsonIntermediateObject(JsonConvertibleObject.DeserializeFromJson(msg));

                        bool result = this.PurgeOrphan(request.OrphanInsList, request.OrphanBindingList);
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
                });
        }

        /// <summary>
        /// Purges orphan services.
        /// </summary>
        /// <param name="orphanInstancesList">The orphan service instances list.</param>
        /// <param name="orphanBindingsList">The orphan service bindings list.</param>
        /// <returns>A value indicating if the request is successful.</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "The exception is logged; errors in this request must not bubble up.")]
        private bool PurgeOrphan(string[] orphanInstancesList, ServiceCredentials[] orphanBindingsList)
        {
            bool ret = true;

            ServiceCredentials[] allBindingsList = this.AllBindingsList();

            foreach (string ins in orphanInstancesList)
            {
                try
                {
                    ServiceCredentials[] bindings = allBindingsList.Where(b => b.Name == ins).ToArray();
                    Logger.Debug(Strings.PurgeOrphanDebugLogMessage, ins, string.Join(", ", JsonConvertibleObject.SerializeToJson(bindings)));

                    ret = ret && this.Unprovision(ins, bindings);

                    // Remove the OBs that are unbinded by unprovision
                    orphanBindingsList = (from ob in orphanBindingsList
                                          where !bindings.Any(binding => binding.Name == ob.Name)
                                          select ob).ToArray();
                }
                catch (Exception ex)
                {
                    Logger.Debug(Strings.PurgeOrphanErrorLogMessage, ins, ex.ToString());
                }
            }

            foreach (ServiceCredentials credential in orphanBindingsList)
            {
                // We're running the unbind on the same thread that other stuff is running for this service instance, so we don't get race conditions; need to wait though, so we have a result status
                credential.ServiceWorkFactory.StartNew(
                    () =>
                    {
                        try
                        {
                            Logger.Debug(Strings.PurgeOrphanUnbindBindingDebugLogMessage, credential.SerializeToJson());
                            ret = ret && this.Unbind(credential);
                        }
                        catch (Exception ex)
                        {
                            Logger.Debug(Strings.PurgeOrphanUnbindBindingErrorLogMessage, credential.SerializeToJson(), ex.ToString());
                        }
                    }).Wait();
            }

            return ret;
        }

        /// <summary>
        /// Get the tmp folder for migration
        /// </summary>
        /// <param name="instance">Instance name.</param>
        /// <returns>A string containing the temp folder.</returns>
        private string GetMigrationFolder(string instance)
        {
            return Path.Combine(this.migrationNfs, "migration", ServiceName(), instance);
        }

        /// <summary>
        /// Called when an import instance request is received.
        /// </summary>
        /// <param name="msg">The message payload.</param>
        /// <param name="reply">The reply to setting.</param>
        /// <param name="subject">The subject of the message.</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "The exception is logged; errors in this request must not bubble up.")]
        private void OnImportInstance(string msg, string reply, string subject)
        {
            Logger.Debug(Strings.OnImportInstanceDebugLogMessage, ServiceDescription(), msg, reply);
            object[] credentials = new object[0];
            credentials = JsonConvertibleObject.DeserializeFromJsonArray(msg);

            ProvisionedServicePlanType plan = (ProvisionedServicePlanType)Enum.Parse(typeof(ProvisionedServicePlanType), JsonConvertibleObject.ObjectToValue<string>(credentials[0]));

            ServiceCredentials prov_cred = new ServiceCredentials();
            ServiceCredentials binding_creds = new ServiceCredentials();

            prov_cred.FromJsonIntermediateObject(credentials[1]);
            binding_creds.FromJsonIntermediateObject(credentials[2]);

            prov_cred.ServiceWorkFactory.StartNew(
                () =>
                {
                    try
                    {
                        string instance = prov_cred.Name;
                        string file_path = this.GetMigrationFolder(instance);

                        bool result = this.ImportInstance(prov_cred, binding_creds, file_path, plan);
                        NodeNats.Publish(reply, null, result.ToString());
                    }
                    catch (Exception ex)
                    {
                        Logger.Warning(ex.ToString());
                    }
                });
        }

        /// <summary>
        /// Called when a discover request is received.
        /// </summary>
        /// <param name="msg">The message payload.</param>
        /// <param name="reply">The reply to setting.</param>
        /// <param name="subject">The subject of the message.</param>
        private void OnDiscover(string msg, string reply, string subject)
        {
            this.SendNodeAnnouncement(msg, reply);
        }

        /// <summary>
        /// Sends the node announcement to the cloud controller.
        /// </summary>
        /// <param name="msg">The request json message.</param>
        /// <param name="reply">The reply subject.</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "The exception is logged; errors in this request must not bubble up.")]
        private void SendNodeAnnouncement(string msg = null, string reply = null)
        {
            try
            {
                if (!this.NodeReady())
                {
                    Logger.Debug(Strings.SendNodeAnnouncementNotReadyDebugLogMessage, ServiceDescription());
                    return;
                }

                Logger.Debug(Strings.SendNodeAnnouncementDebugLogMessage, ServiceDescription(), reply != null ? reply : "everyone");

                DiscoverMessage discoverMessage = new DiscoverMessage();
                if (msg != null)
                {
                    discoverMessage.FromJsonIntermediateObject(JsonConvertibleObject.DeserializeFromJson(msg));
                }

                if (string.IsNullOrEmpty(discoverMessage.Plan) || discoverMessage.Plan == this.plan)
                {
                    Announcement a = this.AnnouncementDetails;
                    a.Id = this.nodeId;
                    a.Plan = this.plan;
                    NodeNats.Publish(reply != null ? reply : string.Format(CultureInfo.InvariantCulture, Strings.NatsSubjectAnnounce, ServiceName()), null, a.SerializeToJson());
                }
            }
            catch (Exception ex)
            {
                Logger.Warning(ex.ToString());
            }
        }
    }
}