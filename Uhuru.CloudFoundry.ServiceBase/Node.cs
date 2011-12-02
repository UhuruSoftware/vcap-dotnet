using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CloudFoundry.Net.Nats;
using Uhuru.CloudFoundry.Server;
using Uhuru.Utilities;
using System.IO;

namespace Uhuru.CloudFoundry.Server.MsSqlNode.Base
{
    public abstract class Node : Base
    {
        string node_id;
        string migration_nfs;
        private Dictionary<string, object> orphan_ins_hash;
        private Dictionary<string, object> orphan_binding_hash;

        public override void Start(Options options)
        {
            node_id = options.NodeId;
            migration_nfs = options.MigrationNfs;
            base.Start(options);
        }

        protected override string flavor()
        {
            return "Node";
        }

        protected override void on_connect_node()
        {
            Logger.Debug(String.Format("{0}: Connected to node mbus", service_description()));

            node_nats.Subscribe(String.Format("{0}.provision.{1}", service_name(), node_id), new SubscribeCallback(on_provision));
            node_nats.Subscribe(String.Format("{0}.unprovision.{1}", service_name(), node_id), new SubscribeCallback(on_unprovision));
            node_nats.Subscribe(String.Format("{0}.bind.{1}", service_name(), node_id), new SubscribeCallback(on_bind));
            node_nats.Subscribe(String.Format("{0}.restore.{1}", service_name(), node_id), new SubscribeCallback(on_restore));
            
            node_nats.Subscribe(String.Format("{0}.discover", service_name()), new SubscribeCallback(on_discover));
            
            node_nats.Subscribe(String.Format("{0}.disable_instance.{1}", service_name(), node_id), new SubscribeCallback(on_disable_instance));
            node_nats.Subscribe(String.Format("{0}.enable_instance.{1}", service_name(), node_id), new SubscribeCallback(on_enable_instance));
            node_nats.Subscribe(String.Format("{0}.import_instance.{1}", service_name(), node_id), new SubscribeCallback(on_import_instance));
            node_nats.Subscribe(String.Format("{0}.cleanup_nfs.{1}", service_name(), node_id), new SubscribeCallback(on_cleanup_nfs));
            
            node_nats.Subscribe(String.Format("{0}.check_orphan", service_name()), new SubscribeCallback(on_check_orphan));
            node_nats.Subscribe(String.Format("{0}.purge_orphan.{1}", service_name(), node_id), new SubscribeCallback(on_purge_orphan));

            pre_send_announcement();
            send_node_announcement();

            TimerHelper.RecurringCall(30000, delegate()
            {
                send_node_announcement();
            });
        }

        private void on_provision(string msg, string reply, string subject)
        {
            Logger.Debug(String.Format("{0}: Provision request: {1} from {2}", service_description(), msg, reply));
            ProvisionResponse response = new ProvisionResponse();
            try
            {
                ProvisionRequest provision_req = new ProvisionRequest();
                provision_req.FromJson(msg);

                ProvisionedServicePlanType plan = provision_req.Plan;
                ServiceCredentials credentials = provision_req.Credentials;
                ServiceCredentials credential = provision(plan, credentials);
                credential.NodeId = node_id;

                response.Credentials = credential;

                Logger.Debug(String.Format("{0}: Successfully provisioned service for request {1}: {2}",
                    service_description(), msg, response.ToJson()));

                node_nats.Publish(reply, msg: encode_success(response));
            }
            catch (Exception ex)
            {
                Logger.Warning(ex.ToString());
                node_nats.Publish(reply, msg: encode_failure(response));
            }
        }

        private void on_unprovision(string msg, string reply, string subject)
        {
            Logger.Debug(String.Format("{0}: Unprovision request: {1}.", service_description(), msg));
           
            SimpleResponse response = new SimpleResponse();
            try
            {
                UnprovisionRequest unprovision_req = new UnprovisionRequest();
                unprovision_req.FromJson(msg);

                string name = unprovision_req.Name;
                ServiceCredentials[] bindings = unprovision_req.Bindings;


                bool result = unprovision(name, bindings);

                if (result)
                {
                    node_nats.Publish(reply, msg: encode_success(response));
                }
                else
                {
                    node_nats.Publish(reply, msg: encode_failure(response));
                }
            }
            catch (Exception ex)
            {
                Logger.Warning(ex.ToString());
                node_nats.Publish(reply, msg: encode_failure(response, ex));
            }
        }

        private void on_bind(string msg, string reply, string subject)
        {
            Logger.Debug(String.Format("{0}: Bind request: {1} from {2}", service_description(), msg, reply));
            BindResponse response = new BindResponse();
            try
            {
                BindRequest bind_message = new BindRequest();
                bind_message.FromJson(msg);
                string name = bind_message.Name;
                Dictionary<string, object> bind_opts = bind_message.BindOptions;
                ServiceCredentials credentials = bind_message.Credentials;
                response.Credentials = bind(name, bind_opts, credentials);
                node_nats.Publish(reply, msg: encode_success(response));
            }
            catch (Exception ex)
            {
                Logger.Warning(ex.ToString());
                node_nats.Publish(reply, msg: encode_failure(response, ex));
            }
        }

        private void on_unbind(string msg, string reply, string subject)
        {
            Logger.Debug(String.Format("{0}: Unbind request: {1} from {2}", service_description(), msg, reply));
            SimpleResponse response = new SimpleResponse();
            try
            {
                UnbindRequest unbind_req = new UnbindRequest();
                unbind_req.FromJson(msg);

                bool result = unbind(unbind_req.Credentials);

                if (result)
                {
                    node_nats.Publish(reply, msg: encode_success(response));
                }
                else
                {
                    node_nats.Publish(reply, msg: encode_failure(response));
                }
            }
            catch (Exception ex)
            {
                Logger.Warning(ex.ToString());
                node_nats.Publish(reply, msg: encode_failure(response, ex));
            }
        }

        private void on_restore(string msg, string reply, string subject)
        {
            Logger.Debug(String.Format("{0}: Restore request: {1} from {2}", service_description(), msg, reply));
            SimpleResponse response = new SimpleResponse();
            try
            {
                RestoreRequest restore_message = new RestoreRequest();
                restore_message.FromJson(msg);
                string instance_id = restore_message.InstanceId;
                string backup_path = restore_message.BackupPath;

                bool result = restore(instance_id, backup_path);
                if (result)
                {
                    node_nats.Publish(reply, msg: encode_success(response));
                }
                else
                {
                    node_nats.Publish(reply, msg: encode_failure(response));
                }
            }
            catch (Exception ex)
            {
                Logger.Warning(ex.ToString());
                node_nats.Publish(reply, msg: encode_failure(response, ex));
            }
        }

        // disable and dump instance
        private void on_disable_instance(string msg, string reply, string subject)
        {
            Logger.Debug(String.Format("{0}: Disable instance {1} request from {2}", service_description(), msg, reply));
            try
            {
                object[] credentials = new object[0];
                credentials = credentials.FromJson(msg);

                ServiceCredentials prov_cred = new ServiceCredentials();
                ServiceCredentials binding_creds = new ServiceCredentials();

                prov_cred.FromJson(credentials[0].ToJson());
                binding_creds.FromJson(credentials[1].ToJson());


                string instance = prov_cred.Name;
                string file_path = get_migration_folder(instance);

                Directory.CreateDirectory(file_path);

                bool result = disable_instance(prov_cred, binding_creds);

                if (result)
                {
                    result = dump_instance(prov_cred, binding_creds, file_path);
                }
                node_nats.Publish(reply, msg: result.ToString());
            }
            catch (Exception ex)
            {
                Logger.Warning(ex.ToString());
            }
        }

        // enable instance and send updated credentials back
        private void on_enable_instance(string msg, string reply, string subject)
        {
            Logger.Debug(String.Format("{0}: enable instance {1} request from {2}", service_description(), msg, reply));
            try
            {
                object[] credentials = new object[0];
                credentials = credentials.FromJson(msg);

                ServiceCredentials prov_cred = new ServiceCredentials();
                Dictionary<string, object> binding_creds_hash = new Dictionary<string, object>();

                prov_cred.FromJson(credentials[0].ToJson());
                binding_creds_hash = binding_creds_hash.FromJson(credentials[1].ToJson());

                bool result = enable_instance(ref prov_cred, ref binding_creds_hash);

                // Update node_id in provision credentials..
                prov_cred.NodeId = node_id;
                credentials[0] = prov_cred.ToDictionary();
                credentials[1] = binding_creds_hash;
                node_nats.Publish(reply, msg: credentials.ToJson());
            }
            catch (Exception ex)
            {
                Logger.Warning(ex.ToString());
            }
        }

        // Cleanup nfs folder which contains migration data
        private void on_cleanup_nfs(string msg, string reply, string subject)
        {
            Logger.Debug(String.Format("{0}: cleanup nfs request {1} from {2}", service_description(), msg, reply));
            try
            {
                object[] request = new object[0];
                request = request.FromJson(msg);
                ServiceCredentials prov_cred = new ServiceCredentials();
                ServiceCredentials binding_creds = new ServiceCredentials();
                prov_cred.FromJson(request[0].ToJson());
                binding_creds.FromJson(request[1].ToJson());

                string instance = prov_cred.Name;
                Directory.Delete(get_migration_folder(instance), true);

                node_nats.Publish(reply, msg: "true");
            }
            catch (Exception ex)
            {
                Logger.Warning(ex.ToString());
            }
        }

        private void on_check_orphan(string msg, string reply, string subject)
        {
            Logger.Debug(String.Format("{0}: handles for checking orphan ", service_description()));

            CheckOrphanResponse response = new CheckOrphanResponse();
            try
            {
                CheckOrphanRequest request = new CheckOrphanRequest();
                request.FromJson(msg);
                check_orphan(request.Handles);

                response.OrphanInstances = orphan_ins_hash;
                response.OrphanBindings = orphan_binding_hash;
                response.Success = true;
            }
            catch (Exception ex)
            {
                Logger.Warning(String.Format("Exception at on_check_orphan {0}", ex.ToString()));
                response.Success = false;
                response.Error = new Dictionary<string, object>() 
                {
                    {"message", ex.Message},
                    {"stack", ex.StackTrace}
                };
            }
            finally
            {
                node_nats.Publish(String.Format("{0}.orphan_result", service_name()), msg: response.ToJson());
            }
        }

        private void check_orphan(Handle[] handles)
        {
            if (handles == null)
            {
                throw new ServiceError(ServiceError.NOT_FOUND, "No handles for checking orphan");
            }

            string[] live_ins_list = all_instances_list();

            Dictionary<string, object> orphan_ins_hash = new Dictionary<string, object>();

            List<string> oi_list = new List<string>();

            foreach (string name in live_ins_list)
            {
                if (!handles.Any(h => h.Credentials.NodeId == node_id && h.ServiceId == name))
                {
                    oi_list.Add(name);
                }
            }

            ServiceCredentials[] live_bind_list = all_bindings_list();
            Dictionary<string, object> orphan_binding_hash = new Dictionary<string, object>();

            List<ServiceCredentials> ob_list = new List<ServiceCredentials>();

            foreach (ServiceCredentials credential in live_bind_list)
            {
                if (!handles.Any(h => h.Credentials.Name == credential.Name && h.Credentials.Username == credential.Username))
                {
                    ob_list.Add(credential);
                }
            }

            Logger.Debug(String.Format("Orphan Instances: {0};  Orphan Bindings: {1}", oi_list.Count, ob_list.Count));
            orphan_ins_hash[node_id.ToString()] = oi_list;
            orphan_binding_hash[node_id.ToString()] = ob_list;
            this.orphan_ins_hash = orphan_ins_hash;
            this.orphan_binding_hash = orphan_binding_hash;
        }

        private void on_purge_orphan(string msg, string reply, string subject)
        {
            Logger.Debug(String.Format("{0}: Message for purging orphan ", service_description()));
            SimpleResponse response = new SimpleResponse();
            try
            {
                PurgeOrphanRequest request = new PurgeOrphanRequest();
                request.FromJson(msg);

                bool result = purge_orphan(request.OrphanInsList, request.OrphanBindingList);
                if (result)
                {
                    node_nats.Publish(reply, msg: encode_success(response));
                }
                else
                {
                    node_nats.Publish(reply, msg: encode_failure(response));
                }
            }
            catch (Exception ex)
            {
                Logger.Warning(ex.Message);
                node_nats.Publish(reply, msg: encode_failure(response, ex));
            }
        }

        private bool purge_orphan(string[] oi_list, ServiceCredentials[] ob_list)
        {
            bool ret = true;
            ServiceCredentials[] ab_list = all_bindings_list();

            foreach (string ins in oi_list)
            {
                try
                {
                    ServiceCredentials[] bindings = ab_list.Where(b => b.Name == ins).ToArray();
                    Logger.Debug(String.Format("Unprovision orphan instance {0} and its bindings {1}", ins, String.Join(", ", bindings.Select(binding => binding.ToJson()).ToArray())));

                    ret = ret && unprovision(ins, bindings);

                    // Remove the OBs that are unbinded by unprovision
                    ob_list = (from ob in ob_list
                               where !bindings.Any(binding => binding.Name == ob.Name)
                               select ob).ToArray();
                }
                catch (Exception ex)
                {
                    Logger.Debug(String.Format("Error on purge orphan instance {0}: {1}", ins, ex.Message));
                }
            }

            foreach (ServiceCredentials credential in ob_list)
            {
                try
                {
                    Logger.Debug(String.Format("Unbind orphan binding {0}", credential.ToJson()));
                    ret = ret && unbind(credential);
                }
                catch (Exception ex)
                {
                    Logger.Debug(String.Format("Error on purge orphan binding {0}: {1}", credential.ToJson(), ex.ToString()));
                }
            }
            return ret;
        }

        // Subclass must overwrite this method to enable check orphan instance feature.
        // Otherwise it will not check orphan instance
        // The return value should be a list of instance name(handle["service_id"]).
        private string[] all_instances_list()
        {
            return new string[0];
        }

        // Subclass must overwrite this method to enable check orphan binding feature.
        // Otherwise it will not check orphan bindings
        // The return value should be a list of binding credentials
        // Binding credential will be the argument for unbind method
        // And it should have at least username & name property for base code
        // to find the orphans
        private ServiceCredentials[] all_bindings_list()
        {
            return new ServiceCredentials[0];
        }

        // Get the tmp folder for migration
        private string get_migration_folder(string instance)
        {
            return Path.Combine(migration_nfs, "migration", service_name(), instance);
        }

        private void on_import_instance(string msg, string reply, string subject)
        {
            Logger.Debug(String.Format("{0}: import instance {1} request from {2}", service_description(), msg, reply));
            try
            {
                object[] credentials = new object[0];
                credentials = credentials.FromJson(msg);

                ProvisionedServicePlanType plan = (ProvisionedServicePlanType)Enum.Parse(typeof(ProvisionedServicePlanType), credentials[0].ToValue<string>());

                ServiceCredentials prov_cred = new ServiceCredentials();
                ServiceCredentials binding_creds = new ServiceCredentials();

                prov_cred.FromJson(credentials[1].ToJson());
                binding_creds.FromJson(credentials[2].ToJson());

                string instance = prov_cred.Name;
                string file_path = get_migration_folder(instance);

                bool result = import_instance(prov_cred, binding_creds, file_path, plan);
                node_nats.Publish(reply, msg: result.ToString());
            }
            catch (Exception ex)
            {
                Logger.Warning(ex.ToString());
            }
        }

        private void on_discover(string msg, string reply, string subject)
        {
            send_node_announcement(reply);
        }

        private void pre_send_announcement()
        {
        }

        private void send_node_announcement(string reply = null)
        {
            try
            {
                if (!node_ready())
                {
                    Logger.Debug(String.Format("{0}: Not ready to send announcement", service_description()));
                    return;
                }

                Logger.Debug(String.Format("{0}: Sending announcement for {1}", service_description(), reply != null ? reply : "everyone"));

                Announcement a = announcement();
                a.Id = node_id;
                node_nats.Publish(reply != null ? reply : String.Format("{0}.announce", service_name()), msg: a.ToJson());
            }
            catch (Exception ex)
            {
                Logger.Warning(ex.ToString());
            }
        }

        private bool node_ready()
        {
            // Service Node subclasses can override this method if they depend
            // on some external service in order to operate; for example, MySQL
            // and Postgresql require a connection to the underlying server.
            return true;
        }

        protected override Dictionary<string, object> varz_details()
        {
            // Service Node subclasses may want to override this method to
            // provide service specific data beyond what is returned by their
            // "announcement" method.
            return announcement().ToDictionary();
        }

        protected override Dictionary<string, string> healthz_details()
        {
            // Service Node subclasses may want to override this method to
            // provide service specific data
            return new Dictionary<string, string>() 
            {
                {"self", "ok"}
            };
        }

        // Helper
        private string encode_success(IWithSuccessStatus response)
        {
            response.Success = true;
            return response.ToJson();
        }

        private string encode_failure(IWithSuccessStatus response, Exception error = null)
        {
            response.Success = false;
            if (error == null || !(error is ServiceError))
            {
              error = new ServiceError(ServiceError.INTERNAL_ERROR);
            }
            response.Error = ((ServiceError)error).ToDictionary();
            return response.ToJson();
        }

        // Service Node subclasses must implement the following methods

        // provision(plan) --> {name, host, port, user, password}
        protected abstract ServiceCredentials provision(ProvisionedServicePlanType plan, ServiceCredentials credentials);

        // unprovision(name) --> void
        protected abstract bool unprovision(string name, ServiceCredentials[] bindings);

        // bind(name, bind_opts) --> {host, port, login, secret}
        protected abstract ServiceCredentials bind(string name, Dictionary<string, object> bind_opts, ServiceCredentials credentials);

        // unbind(credentials)  --> void
        protected abstract bool unbind(ServiceCredentials credentials);

        // announcement() --> { any service-specific announcement details }
        protected abstract Announcement announcement();

        // service_name() --> string
        // (inhereted from VCAP::Services::Base::Base)

        // <action>_instance(prov_credential, binding_credentials)  -->  true for success and nil for fail
        protected abstract bool disable_instance(ServiceCredentials prov_credential, ServiceCredentials binding_credentials);
        protected abstract bool dump_instance(ServiceCredentials prov_credential, ServiceCredentials binding_credentials, string file_path);
        protected abstract bool import_instance(ServiceCredentials prov_credential, ServiceCredentials binding_credentials, string file_path, ProvisionedServicePlanType plan);
        protected abstract bool enable_instance(ref ServiceCredentials prov_credential, ref Dictionary<string, object> binding_credentials_hash);
        protected abstract bool restore(string instanceId, string backup_path);

    }
}