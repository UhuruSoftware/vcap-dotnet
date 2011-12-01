using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.IO;
using System.Web.Script.Serialization;
using System.Net;
using Newtonsoft.Json.Linq;
using SevenZip;
using System.Security.Cryptography;
using System.Collections.Specialized;
using System.Threading;
using System.Reflection;
using CloudFoundry.Net.HttpHelpers;
using CloudFoundry.Net;
using System.Globalization;

namespace CloudFoundry.Net
{

    public class PushStatusEventArgs : EventArgs
    {
        public PushStatus Status
        {
            get;
            set;
        }

        public string AppName
        {
            get;
            set;
        }

    }

    public enum PushStatus
    {
        CHECKING = 0,
        CREATING = 1,
        BINDING = 2,
        PACKAGING = 3,
        UPLOADING = 4,
        STAGING = 5,
        STARTING = 6
    }

    public class Client : IClient
    {
        public event EventHandler<PushStatusEventArgs> UpdatePushStatus;

        private const string DEFAULT_TARGET = "https://api.cloudfoundry.com";
        private const string DEFAULT_LOCAL_TARGET = "http://api.vcap.me";

        private const string INFO_PATH = "/info";
        private const string GLOBAL_SERVICES_PATH = "/info/services";
        private const string GLOBAL_RUNTIMES_PATH = "/info/runtimes";
        private const string RESOURCES_PATH = "/resources";

        private const string APPS_PATH = "/apps";
        private const string SERVICES_PATH = "/services";
        private const string USERS_PATH = "/users";
        
        
        private string proxyUser = String.Empty;
        private string userToken = String.Empty;
        private string targetUrl;

    
        private string userEmail;
        private HttpRequestHelper httpHelper = new HttpRequestHelper(String.Empty, String.Empty);
        private RestJsonHelper jsonHelper = new RestJsonHelper(new HttpRequestHelper(String.Empty, String.Empty));

        public Client Clone()
        {
            Client clientObj = new Client();
            clientObj.userEmail = userEmail;
            clientObj.userToken = userToken;
            clientObj.targetUrl = targetUrl;
            clientObj.proxyUser = proxyUser;
            clientObj.httpHelper = httpHelper;
            clientObj.jsonHelper = jsonHelper;
            return clientObj;
        }

        public string TargetUrl
        {
            get { return targetUrl; }
            set { targetUrl = value; }
        }
        
        public bool AddUser(string email, string password)
        {
            return addUser(email, password);
        }

        public List<App> Apps()
        {
            JArray appList = apps();

            List<App> result = new List<App>();

            foreach (JObject jObject in appList)
            {
                App app = new App();
                app.Name = jObject["name"].Value<string>();
                app.Instances = jObject["instances"].Value<string>();
                app.RunningInstances = jObject["runningInstances"].Value<string>();
                app.Services = String.Join(", ", jObject["services"].Values<string>().ToArray());
                app.Uris = String.Join(", ", jObject["uris"].Values<string>().ToArray());
                app.Version = jObject["version"].Value<string>();

                app.MetaCreated = jObject["meta"]["created"].Value<string>();
                app.MetaDebug = jObject["meta"]["debug"].Value<string>();
                app.MetaVersion = jObject["meta"]["version"].Value<string>();
                
                app.ResourcesDisk = jObject["resources"]["disk"].Value<string>();
                app.ResourcesFDS = jObject["resources"]["fds"].Value<string>();
                app.ResourcesMemory = jObject["resources"]["memory"].Value<string>();
                
                app.StagingModel = jObject["staging"]["model"].Value<string>();
                app.StagingStack = jObject["staging"]["stack"].Value<string>();

                result.Add(app);
            }

            return result;
        }

        public App GetAppByName(string appName)
        {
            //todo: vladi: this is a very inneficient way to do things
            JArray appList = apps();

            App result = null;

            foreach (JObject jObject in appList)
            {
                App app = new App();
                app.Name = jObject["name"].Value<string>();

                if (app.Name != appName)
                {
                    continue;
                }

                app.Instances = jObject["instances"].Value<string>();
                app.RunningInstances = jObject["runningInstances"].Value<string>();
                app.Services = String.Join(", ", jObject["services"].Values<string>().ToArray());
                app.Uris = String.Join(", ", jObject["uris"].Values<string>().ToArray());
                app.Version = jObject["version"].Value<string>();

                app.MetaCreated = jObject["meta"]["created"].Value<string>();
                app.MetaDebug = jObject["meta"]["debug"].Value<string>();
                app.MetaVersion = jObject["meta"]["version"].Value<string>();

                app.ResourcesDisk = jObject["resources"]["disk"].Value<string>();
                app.ResourcesFDS = jObject["resources"]["fds"].Value<string>();
                app.ResourcesMemory = jObject["resources"]["memory"].Value<string>();

                app.StagingModel = jObject["staging"]["model"].Value<string>();
                app.StagingStack = jObject["staging"]["stack"].Value<string>();

                result = app;
                break;
            }

            return result;
        }


        public bool DeleteUser(string email)
        {
            return deleteUser(email);
        }

        public List<Framework> Frameworks()
        {
            JObject serverInfo = this.serverInfo();

            List<Framework> result = new List<Framework>();

            foreach (JProperty property in ((JObject)serverInfo["frameworks"]).Properties())
            {
                Framework framework = new Framework();
                framework.Name = property.Name;

                JArray runtimes = (JArray)serverInfo["frameworks"][framework.Name]["runtimes"];

                foreach (JObject obj in runtimes)
                {
                    Runtime runtime = new Runtime();
                    runtime.Name = obj["name"].Value<string>();
                    runtime.Version = obj["version"].Value<string>();
                    runtime.Description = obj["description"].Value<string>();
                    framework.Runtimes.Add(runtime);
                }

                JArray appServers = (JArray)serverInfo["frameworks"][framework.Name]["appservers"];

                foreach (JObject obj in appServers)
                {
                    framework.AppServers.Add(obj["description"].Value<string>());
                }

                result.Add(framework);
            }

            return result;
        }

        public bool Login(string email, string password)
        {
            return login(email, password);
        }

        public void Logout()
        {
            this.userEmail = String.Empty;
            this.userToken = String.Empty;
            this.proxyUser = String.Empty;
            this.httpHelper = new HttpRequestHelper(String.Empty, string.Empty);
            this.jsonHelper = new RestJsonHelper(this.httpHelper);
        }

        public List<ProvisionedService> ProvisionedServices()
        {
            JArray serviceList = services();

            List<ProvisionedService> result = new List<ProvisionedService>();

            foreach (JObject jObject in serviceList)
            {
                ProvisionedService service = new ProvisionedService();
                service.Name = jObject["name"].Value<string>();
                service.Type = jObject["type"].Value<string>();
                service.Vendor = jObject["vendor"].Value<string>();
                service.Version = jObject["version"].Value<string>();
                service.Tier = jObject["tier"].Value<string>();
                
                service.MetaCreated = jObject["meta"]["created"].Value<string>();
                service.MetaTags = String.Join(", ", jObject["meta"]["tags"].Values<string>().ToArray());
                service.MetaUpdated = jObject["meta"]["updated"].Value<string>();
                service.MetaVersion = jObject["meta"]["version"].Value<string>();

                result.Add(service);
            }

            return result;
        }

        public List<Runtime> Runtimes()
        {
            JObject runtimes = runtimesInfo();

            List<Runtime> result = new List<Runtime>();

            foreach (JProperty runtimeProperty in runtimes.Properties())
            {
                Runtime runtime = new Runtime();
                runtime.Name = runtimeProperty.Name;
                runtime.Version = runtimes[runtime.Name]["version"].Value<string>();
                runtime.Description = "";

                result.Add(runtime);
            }

            return result;
        }

        public List<Service> Services()
        {
            JObject services = serviceInfo();

            List<Service> result = new List<Service>();

            foreach (JProperty typeProperty in services.Properties())
            {
                string type = typeProperty.Name;

                foreach (JProperty vendorProperty in ((JObject)services[type]).Properties())
                {
                    string vendor = vendorProperty.Name;

                    foreach (JProperty versionProperty in ((JObject)services[type][vendor]).Properties())
                    {
                        string version = versionProperty.Name;

                        Service service = new Service();
                        service.Vendor = vendor;
                        service.Version = version;
                        service.Description = services[type][vendor][version]["description"].Value<string>();
                        service.Type = type;
                        result.Add(service);
                    }
                }
            }

            return result;
        }

        public bool Target(string apiUrl)
        {
            //TODO: fix this so a proper URL can get out of it
            targetUrl = "http://" + apiUrl;
            return true;
        }

        public List<User> Users()
        {
            JArray userList = users();

            List<User> result = new List<User>();

            foreach (JObject jObject in userList)
            {
                User user = new User();
                user.Email = jObject["email"].Value<string>();
                user.IsAdmin = jObject["admin"].Value<string>();

                user.Apps = String.Join(", ", ((JArray)jObject["apps"]).Select(app => app["name"].Value<string>()).ToArray());

                result.Add(user);
            }

            return result;
        }

        public bool CreateService(string serviceName, string serviceType)
        {
            return createService(serviceType, serviceName);
        }

        public bool DeleteService(string serviceName)
        {
            return deleteService(serviceName);
        }

        public bool AppExists(string name)
        {
            return appInfo(name) != null;
        }

        public bool UpdateBits(string name, string path, bool debug = false)
        {
            if (path.EndsWith("\\"))
            {
                path.Remove(path.Length - 1);
            }

            checkLoginStatus();

            if (!uploadAppBits(name, path))
            {
                return false;
            }

            if (appInfo(name)["state"].Value<string>() == "STARTED")
            {
                if (!stopApp(name))
                {
                    return false;
                }

                if (!startApp(name, debug, false))
                {
                    return false;
                }
            }
            return true;
        }

        public bool Push(string name, string url, string path, int instances, string framework, string runtime, int memory, List<string> services, 
            bool debug, bool waitForStart, bool start = true)
        {
            if (path.EndsWith("\\"))
            {
                path.Remove(path.Length - 1);
            }

            if (!push(name, url, path, instances, framework, runtime, memory, services))
            {
                return false;
            }
            if (start)
            {
                return startApp(name, false, waitForStart);
            }
            return true;
        }

        public bool DeleteApp(string name)
        {
            return deleteApp(name);
        }

        public bool StartApp(string name, bool waitForStart = true, bool debug = false)
        {
            return startApp(name, debug, waitForStart);
        }

        public bool StopApp(string name)
        {
            return stopApp(name);
        }

        public bool BindService(string name, string service)
        {
            return bindService(service, name);
        }

        public bool UnbindService(string name, string service)
        {
            return unbindService(service, name);
        }

        public bool MapUri(string appName, string uri)
        {
            return mapUri(appName, uri);
        }

        public bool UnmapUri(string appName, string uri)
        {
            return unmapUri(appName, uri);
        }

        private void UpdateStatus(PushStatus status, string appName)
        {
            if (UpdatePushStatus != null)
            {
                PushStatusEventArgs args = new PushStatusEventArgs();
                args.Status = status;
                args.AppName = appName;
                UpdatePushStatus(this, args);
            }
        }

        private JObject serverInfo()
        {
            checkLoginStatus();
            return jsonHelper.GetObject(targetUrl + INFO_PATH);
        }

        private JObject serviceInfo()
        {
            checkLoginStatus();
            return jsonHelper.GetObject(targetUrl + GLOBAL_SERVICES_PATH);
        }

        private JObject runtimesInfo()
        {
            return jsonHelper.GetObject(targetUrl + GLOBAL_RUNTIMES_PATH);
        }

        private JArray apps()
        {
            checkLoginStatus();
            return jsonHelper.GetArray(targetUrl + APPS_PATH);
        }

        private void checkAppLimit()
        {
            JObject usage = (JObject)serverInfo()["usage"];
            JObject limits = (JObject)serverInfo()["limits"];

            if (usage == null || limits == null || limits["apps"] == null || limits["apps"].Value<int>() == 0)
            {
                return;
            }

            if (limits["apps"].Value<int>() <= usage["apps"].Value<int>())
            {
                throw new ApplicationException(
                    String.Format(CultureInfo.InvariantCulture, "Not enough capacity for operation. {0} out of {1} apps already in use.",
                    usage["apps"].Value<int>(), limits["apps"].Value<int>()));
            }
        }

   

        private bool uploadAppBits(string appname, string path)
        {
            UpdateStatus(PushStatus.PACKAGING, appname);

            string tempDir = Path.Combine(Path.GetTempPath(), "uhurucf\\" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(tempDir);
            string uploadFile = Path.Combine(tempDir, appname + ".zip");

            File.Delete(uploadFile);


            string explodeDir = Path.Combine(tempDir, String.Format(CultureInfo.InvariantCulture, "vmc_{0}_files", appname));
            if (Directory.Exists(explodeDir))
            {
                Directory.Delete(explodeDir, true);
            }
            Directory.CreateDirectory(explodeDir);

            string[] warFiles = Directory.GetFiles(path, "*.war", SearchOption.AllDirectories);

            // As VMC does it, only explode the first war encountered
            if (warFiles.Length > 0)
            {
                Utils.UnZipFile(explodeDir, warFiles[0]);
            }
            else
            {
                //Now Create all of the directories
                foreach (string dirPath in Directory.GetDirectories(path, "*", SearchOption.AllDirectories))
                {
                    Directory.CreateDirectory(dirPath.Replace(path, explodeDir));
                }

                //TODO: perhaps we should ignore git files, VMC does it
                //Copy all the files
                foreach (string newPath in Directory.GetFiles(path, "*", SearchOption.AllDirectories))
                {
                    File.Copy(newPath, newPath.Replace(path, explodeDir));
                }
            }

            //TODO: should have option to always push everything
            //Send the resource list to the cloudcontroller, the response will tell us what it already has..
            long totalSize = 0;
            List<Dictionary<string, object>> fingerprints = new List<Dictionary<string,object>>();

            string[] resource_files = Directory.GetFiles(explodeDir, "*", SearchOption.AllDirectories);

            foreach (string filename in resource_files)
            {
                FileStream fs = File.OpenRead(filename);
                string sha1 = BitConverter.ToString((new SHA1CryptoServiceProvider()).ComputeHash(fs)).Replace("-","");
                fs.Close();
                
                FileInfo fileInfo = new FileInfo(filename);

                fingerprints.Add(new Dictionary<string,object>(){
                    {"size", fileInfo.Length},
                    {"sha1", sha1},
                    {"fn", filename.Replace(explodeDir + "\\", "")}
                });
                totalSize += fileInfo.Length;
            }

            JArray appcloudResources = null;

            // Check to see if the resource check is worth the round trip
            if (totalSize > (64*1024))
            {
                // Send resource fingerprints to the cloud controller
                appcloudResources = checkResources(fingerprints);
            }

            if (appcloudResources!= null)
            {
                foreach (JObject resource in appcloudResources)
                {
                    File.Delete(Path.Combine(explodeDir, resource["fn"].Value<string>()));
                }
            }

            // If no resource needs to be sent, add an empty file to ensure we have
            // a multi-part request that is expected by nginx fronting the CC.
            if (Directory.GetFiles(explodeDir, "*", SearchOption.AllDirectories).Length == 0)
            {
                File.WriteAllText(Path.Combine(explodeDir, ".__empty__"), "");
            }

            // Perform Packing of the upload bits here.
            Utils.ZipFile(explodeDir, uploadFile);

            return uploadApp(appname, uploadFile, appcloudResources);
        }

        private bool push(string appName, string url, string path, int instances, string framework, string runtime, int memoryMB, List<string> servicesToBind)
        {
            UpdateStatus(PushStatus.CHECKING, appName);
            checkLoginStatus();
            // Check app existing upfront if we have appname
            if (appInfo(appName) != null)
            {
                throw new ApplicationException("Application " + appName + " already exists.");
            }

            // check if we have hit our app limit
            // TODO: this may not be the right place for this extra check
            checkAppLimit();

            // check memsize here for capacity
            // TODO: implement this for .Net
            //if memswitch && !no_start
            //  check_has_capacity_for(mem_choice_to_quota(memswitch) * instances)
            //end

            path = Path.GetFullPath(path);

            if (!Directory.Exists(path))
            {
                throw new ApplicationException("Deployment path does not exist");
            }

            Dictionary<string, object> manifest = new Dictionary<string, object>() {
                {"name", appName},
                {"staging", new Dictionary<string, object>() 
                {
                    {"framework", framework},
                    {"runtime", runtime}
                }},
                {"uris", new string[] {url}},
                {"instances", instances},
                {"resources", new Dictionary<string, object>() 
                {
                    {"memory", memoryMB}
                }}
            };

            UpdateStatus(PushStatus.CREATING, appName);

            if (!createApp(manifest))
            {
                throw new ApplicationException("Could not create app.");
            }

            UpdateStatus(PushStatus.BINDING, appName);

            foreach (string service in servicesToBind)
            {
                bindService(service, appName);
            }

            // Stage and upload the app bits.
            return uploadAppBits(appName, path);
        }

        private bool startApp(string appName, bool debug, bool waitForStarted)
        {
            JObject app = appInfo(appName);

            if (app == null)
            {
                throw new ApplicationException("Application " + appName + " could not be found");
            }

            if (app["state"].Value<string>() == "STARTED")
            {
                throw new ApplicationException("Application " + appName + " is already started");
            }

            UpdateStatus(PushStatus.STAGING, appName);

            //TODO: better error handling

            app["state"] = "STARTED";

            if (!updateApp(appName, app))
            {
                throw new ApplicationException("Could not start app " + appName);
            }

            UpdateStatus(PushStatus.STARTING, appName);

            if (!waitForStarted)
            {
                return true;
            }

            DateTime startTime = DateTime.Now;

            while ((startTime - DateTime.Now).TotalSeconds < 120)
            {
                //TODO: better error handling and debug state
                object runningInstances = appInfo(appName)["runningInstances"];
                if (runningInstances != null)
                {
                    int instances = 0;

                    if (Int32.TryParse(runningInstances.ToString(), out instances) && instances > 0)
                    {
                        return true;
                    }
                }
                Thread.Sleep(1000);
            }
            return false;
        }

        private bool stopApp(string name)
        {
            JObject app = appInfo(name);

            if (app == null)
            {
                throw new ApplicationException("Application " + name + " does not exist.");
            }

            if (app["state"].Value<string>() == "STOPPED")
            {
                throw new ApplicationException("Application " + name + " is already stopped.");
            }

            app["state"] = "STOPPED";

            return updateApp(name, app);
        }

        private bool createApp(Dictionary<string, object> manifest)
        {
            HttpResponse response = jsonHelper.Post(targetUrl + APPS_PATH, JObject.FromObject(manifest).ToString());
            if (response.Status != HttpStatusCode.Redirect)
            {
                return false;
            }
            return JObject.Parse(response.Body)["result"].Value<string>() == "success";
        }

        private bool updateApp(string name, object manifest)
        {
            checkLoginStatus();
            return jsonHelper.Put(targetUrl + String.Format(CultureInfo.InvariantCulture, "{0}/{1}", APPS_PATH, name), JObject.FromObject(manifest).ToString()).Status == HttpStatusCode.OK;
        }


        private bool uploadApp(string name, string zipfile, JArray resource_manifest)
        {
            UpdateStatus(PushStatus.UPLOADING, name);

            //FIXME, manifest should be allowed to be null, here for compatability with old cc's
            resource_manifest = resource_manifest == null ? new JArray() : resource_manifest;
            checkLoginStatus();

            return httpHelper.HttUploadZip(targetUrl + String.Format(CultureInfo.InvariantCulture, "{0}/{1}/application", APPS_PATH, name), zipfile,
                new NameValueCollection(){
                    {"_method", "put"},
                    {"resources", resource_manifest.ToString()}
                }).Status == HttpStatusCode.OK;
        }

        private bool deleteApp(string name)
        {
            checkLoginStatus();
            return httpHelper.Delete(targetUrl + String.Format(CultureInfo.InvariantCulture, "{0}/{1}", APPS_PATH, name)).Status == HttpStatusCode.OK;
        }

        private JObject appInfo(string name)
        {
            checkLoginStatus();
            return jsonHelper.GetObject(targetUrl + String.Format(CultureInfo.InvariantCulture, "{0}/{1}", APPS_PATH, name));
        }

        public JObject AppUpdateInfo(string name)
        {
            checkLoginStatus();
            return jsonHelper.GetObject(targetUrl + String.Format(CultureInfo.InvariantCulture, "{0}/{1}/update", APPS_PATH, name));
        }

        public JObject AppStats(string name)
        {
            checkLoginStatus();
            JObject stats_raw = jsonHelper.GetObject(targetUrl + String.Format(CultureInfo.InvariantCulture, "{0}/{1}/stats", APPS_PATH, name));
            return stats_raw;
        }

        public JObject AppInstances(string name)
        {
            checkLoginStatus();
            return jsonHelper.GetObject(targetUrl + String.Format(CultureInfo.InvariantCulture, "{0}/{1}/instances", APPS_PATH, name));
        }

        public JObject AppCrashes(string name)
        {
            checkLoginStatus();
            return jsonHelper.GetObject(targetUrl + String.Format(CultureInfo.InvariantCulture, "{0}/{1}/crashes", APPS_PATH, name));
        }

        public string AppFiles(string name, string path, int instance)
        {
            checkLoginStatus();
            string url = targetUrl + String.Format(CultureInfo.InvariantCulture, "{0}/{1}/instances/{2}/files/{3}", APPS_PATH, name, instance, path);
            url = url.Replace("//", "/");
            HttpResponse response = httpHelper.Get(url, "");

            return response.Body;
        }

        private JArray services()
        {
            checkLoginStatus();
            return jsonHelper.GetArray(targetUrl + SERVICES_PATH);
        }

        private bool createService(string service, string name)
        {
            checkLoginStatus();
            List<Service> services = Services();

            Dictionary<string, string> service_hash = null;

            foreach (Service serviceDescription in services)
            {
                if (serviceDescription.Vendor == service)
                {
                    service_hash = new Dictionary<string,string>() {
                        {"type", serviceDescription.Type},
                        {"tier", "free"},
                        {"vendor", service},
                        {"version", serviceDescription.Version},
                        {"name", name}
                    };
                    break;
                }
            }

            if (service_hash == null)
            {
                throw new ApplicationException(service + "is not a valid service choice.");
            }

            return jsonHelper.Post(targetUrl + SERVICES_PATH, JObject.FromObject(service_hash).ToString()).Status == HttpStatusCode.OK;
        }

        private bool deleteService(string name)
        {
            checkLoginStatus();

            List<ProvisionedService> provisionedServices = ProvisionedServices();

            if (!provisionedServices.Exists(service => service.Name == name))
            {
                throw new ApplicationException("Service " + name + " is not a valid service.");
            }

            return httpHelper.Delete(targetUrl + String.Format(CultureInfo.InvariantCulture, "{0}/{1}", SERVICES_PATH, name)).Status == HttpStatusCode.OK;
        }

        private bool bindService(string service, string appname)
        {
            checkLoginStatus();
            JObject app = appInfo(appname);
            JArray services = (JArray)app["services"];
            services.Add(service);
            app["services"] = services;

            return updateApp(appname, app);
        }

        public string[] GetAppServices(string appname)
        {
            checkLoginStatus();
            JObject app = appInfo(appname);
            JArray services = (JArray)app["services"];
            return services.ToObject<string[]>();
        }

        private bool unbindService(string service, string appname)
        {
            checkLoginStatus();
            JObject app = appInfo(appname);
            JArray services = (JArray)app["services"];
            foreach (JToken tok in services)
            {
                if (tok.ToObject<string>() == service)
                {
                    tok.Remove();
                    break;
                }
            }
            app["services"] = services;

            return updateApp(appname, app);
        }

        private JArray checkResources(List<Dictionary<string, object>> resources)
        {
            checkLoginStatus();
            HttpResponse response = jsonHelper.Post(targetUrl + RESOURCES_PATH,  JArray.FromObject(resources).ToString());
            if (response.Status == HttpStatusCode.OK)
            {
                return JArray.Parse(response.Body);
            }
            else
            {
                return new JArray();
            }
        }

        private bool targetValid
        {
            get
            {
                throw new NotImplementedException();
                //  return false unless descr = info
                //  return false unless descr[:name]
                //  return false unless descr[:build]
                //  return false unless descr[:version]
                //  return false unless descr[:support]
                //  true
                //rescue
                //  false
            }
        }
 
        private bool loggedIn
        {
            get
            {
                //descr = info
                //if descr
                //  return false unless descr[:user]
                //  return false unless descr[:usage]
                //  @user = descr[:user]
                //  true
                //end
                throw new NotImplementedException();
            }
        }

        private bool login(string user, string password)
        {
            HttpResponse response = jsonHelper.Post(targetUrl + USERS_PATH + "/" + user + "/tokens", 
                JObject.FromObject(new Dictionary<string, string>() 
                { 
                    {"password", password}
                }).ToString());

            if (response != null && response.Status == HttpStatusCode.OK)
            {
                userEmail = user;
                userToken = JObject.Parse(response.Body)["token"].Value<string>();

                //replace http and json helpers with authorized versions
                httpHelper = new HttpRequestHelper(userToken, "");
                jsonHelper = new RestJsonHelper(httpHelper);

                return true;
            }
            else
            {
                return false;
            }
        }

        public object ChangePassword(string new_password)
        {
            checkLoginStatus();
            //user_info = json_get("//{VMC::USERS_PATH}///{@user}")
            //if user_info
            //  user_info[:password] = new_password
            //  json_put("//{VMC::USERS_PATH}///{@user}", user_info)
            //end

            throw new NotImplementedException();
        }

        private JArray users()
        {
            checkLoginStatus();
            return jsonHelper.GetArray(targetUrl + USERS_PATH);
        }

        private bool addUser(string user_email, string password)
        {
            return jsonHelper.Post(targetUrl + USERS_PATH, 
                JObject.FromObject(new Dictionary<string, string>(){
                    {"email", user_email},
                    {"password", password}
                }).ToString()).Status == HttpStatusCode.NoContent;
        }

        private bool deleteUser(string user_email)
        {
            checkLoginStatus();
            return httpHelper.Delete(targetUrl + String.Format(CultureInfo.InvariantCulture, "{0}/{1}", USERS_PATH, user_email)).Status == HttpStatusCode.NoContent;
        }

        public void checkLoginStatus()
        {
            if (userEmail == String.Empty || userToken == String.Empty)
            {
                throw new ApplicationException("Not logged-in into CloudFoundry!");
            }
        }

        private bool mapUri(string appname, string uri)
        {
            checkLoginStatus();
            JObject app = appInfo(appname);
            JArray uris = (JArray)app["uris"];
            uris.Add(uri);
            app["uris"] = uris;
            return updateApp(appname, app);
        }

        private bool unmapUri(string appname, string uri)
        {
            checkLoginStatus();
            JObject app = appInfo(appname);
            JArray uris = (JArray)app["uris"];
            foreach (JToken tok in uris)
            {
                if (tok.ToObject<string>() == uri)
                {
                    tok.Remove();
                    break;
                }
            }
            app["uris"] = uris;
            return updateApp(appname, app);
        }
    }
}
