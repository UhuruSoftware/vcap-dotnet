using System;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Security;
using System.ServiceModel.Web;

namespace Uhuru.Utilities
{
    /// <summary>
    /// This is an EventArgs class used by the Healthz and Varz server.
    /// When the server needs healthz information, it raises an event.
    /// The subscriber to that event sets the message of these args.
    /// </summary>
    public class HealthzRequestEventArgs : EventArgs
    {
        private string healthzMessage;

        /// <summary>
        /// Gets or sets the healthz message that will be served by the server.
        /// </summary>
        public string HealthzMessage
        {
            get { return healthzMessage; }
            set { healthzMessage = value; }
        }
    }

    /// <summary>
    /// This is an EventArgs class used by the Healthz and Varz server.
    /// When the server needs varz information, it raises an event.
    /// The subscriber to that event sets the message of these args.
    /// </summary>
    public class VarzRequestEventArgs : EventArgs
    {
        private string varzMessage;

        /// <summary>
        /// Gets or sets the varz message that will be served by the server.
        /// </summary>
        public string VarzMessage
        {
            get { return varzMessage; }
            set { varzMessage = value; }
        }
    }

    /// <summary>
    /// This class implements an http server that is used to get healthz and varz information about a Cloud Foundry component.
    /// </summary>
    public sealed class MonitoringServer : IDisposable
    {
        private int serverPort;
        private string hostName;
        private string username;
        private string password;
        private WebServiceHost host;
        private static MonitoringServer instance = null;

        /// <summary>
        /// Event that is raised when the server receives a healthz request (http://[ip]:[port]/healthz).
        /// </summary>
        public event EventHandler<HealthzRequestEventArgs> HealthzRequested;
        /// <summary>
        /// Event that is raised when the server receives a varz request (http://[ip]:[port]/varz).
        /// </summary>
        public event EventHandler<VarzRequestEventArgs> VarzRequested;

        /// <summary>
        /// Public constructor.
        /// </summary>
        /// <param name="port">The port used by the server to listen.</param>
        /// <param name="host">The host used to publish the service.</param>
        /// <param name="serverUserName">A username for basic authentication.</param>
        /// <param name="serverPassword">A password for basic authentication.</param>
        public MonitoringServer(int port, string host, string serverUserName, string serverPassword)
        {
            hostName = host;
            serverPort = port;
            username = serverUserName;
            password = serverPassword;

            if (instance == null)
            {
                instance = this;
            }
        }

        /// <summary>
        /// Starts the server.
        /// </summary>
        public void Start()
        {
            Uri baseAddress = new Uri("http://" + hostName + ":" + serverPort);

            WebHttpBinding httpBinding = new WebHttpBinding();
            httpBinding.Security.Mode = WebHttpSecurityMode.TransportCredentialOnly;
            httpBinding.Security.Transport.ClientCredentialType = HttpClientCredentialType.Basic;
            
            host = new WebServiceHost(typeof(MonitoringService), baseAddress);
            host.AddServiceEndpoint(typeof(IMonitoringService),
                httpBinding, baseAddress);
            
            host.Credentials.UserNameAuthentication.CustomUserNamePasswordValidator = new UserCustomAuthentication(username, password);
            host.Credentials.UserNameAuthentication.UserNamePasswordValidationMode = UserNamePasswordValidationMode.Custom; 
            host.Open();
        }

        /// <summary>
        /// Stops the server.
        /// </summary>
        public void Stop()
        {
            host.Close();
        }

        private string TriggerHealthz(object sender, HealthzRequestEventArgs e)
        {
            if (HealthzRequested != null)
            {
                 HealthzRequested(sender, e);
            }
            if (e != null)
            {
                return e.HealthzMessage;
            }
            else
            {
                return String.Empty;
            }
        }

        private string TriggerVarz(object sender, VarzRequestEventArgs e)
        {
            if (VarzRequested != null)
            {
                VarzRequested(sender, e);
            }
            if (e != null)
            {
                return e.VarzMessage;
            }
            else
            {
                return String.Empty;
            }
        }

        private static MonitoringServer Instance
        {
            get
            {
                return instance;
            }
        }

        #region IDisposable Members

        /// <summary>
        /// IDisposable implementation.
        /// </summary>
        public void Dispose()
        {
            if (host != null)
            {
                host.Close();
            }
        }

        #endregion



        [ServiceContract]
        interface IMonitoringService
        {
            [WebGet(UriTemplate = "/heathz")]
            Message GetHealthz();

            [WebGet(UriTemplate = "/varz")]
            Message GetVarz();
        }

        private class MonitoringService : IMonitoringService
        {
            public Message GetHealthz()
            {
                string message = MonitoringServer.Instance.TriggerHealthz(this, new HealthzRequestEventArgs());
                return CreateTextresponse(message, "text/plaintext");
            }

            public Message GetVarz()
            {
                string message = MonitoringServer.Instance.TriggerVarz(this, new VarzRequestEventArgs());
                return CreateTextresponse(message, "application/json");
            }

            private static Message CreateTextresponse(string message, string contentType)
            {
                return WebOperationContext.Current.CreateTextResponse(message, contentType);
            }
        }
    }
}
