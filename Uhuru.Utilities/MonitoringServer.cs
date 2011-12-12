// -----------------------------------------------------------------------
// <copyright file="MonitoringServer.cs" company="Uhuru Software, Inc.">
// Copyright (c) 2011 Uhuru Software, Inc., All Rights Reserved
// </copyright>
// -----------------------------------------------------------------------

namespace Uhuru.Utilities
{
    using System;
    using System.ServiceModel;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Security;
    using System.ServiceModel.Web;
    
    /// <summary>
    /// This class implements an http server that is used to get healthz and varz information about a Cloud Foundry component.
    /// </summary>
    public sealed class MonitoringServer : IDisposable
    {
        /// <summary>
        /// The singleton MonitoringServer instance.
        /// </summary>
        private static MonitoringServer instance = null;

        /// <summary>
        /// The port on which the server is listening.
        /// </summary>
        private int serverPort;
        
        /// <summary>
        /// The hostname of the machine hosting the service.
        /// </summary>
        private string hostName;
        
        /// <summary>
        /// The username used for basic http authentication.
        /// </summary>
        private string username;

        /// <summary>
        /// The password used for basic http authentication.
        /// </summary>
        private string password;

        /// <summary>
        /// The WCF service host used to publish the server.
        /// </summary>
        private WebServiceHost host;

        /// <summary>
        /// Initializes a new instance of the MonitoringServer class
        /// </summary>
        /// <param name="port">The port used by the server to listen.</param>
        /// <param name="host">The host used to publish the service.</param>
        /// <param name="serverUserName">A username for basic authentication.</param>
        /// <param name="serverPassword">A password for basic authentication.</param>
        public MonitoringServer(int port, string host, string serverUserName, string serverPassword)
        {
            this.hostName = host;
            this.serverPort = port;
            this.username = serverUserName;
            this.password = serverPassword;

            if (instance == null)
            {
                instance = this;
            }
        }

        /// <summary>
        /// Event that is raised when the server receives a healthz request (http://[ip]:[port]/healthz).
        /// </summary>
        public event EventHandler<HealthzRequestEventArgs> HealthzRequested;

        /// <summary>
        /// Event that is raised when the server receives a varz request (http://[ip]:[port]/varz).
        /// </summary>
        public event EventHandler<VarzRequestEventArgs> VarzRequested;

        /// <summary>
        /// WCF contract that the service implements.
        /// </summary>
        [ServiceContract]
        private interface IMonitoringService
        {
            /// <summary>
            /// Gets the healthz message.
            /// </summary>
            /// <returns>A JSON string containing the healthz message.</returns>
            [WebGet(UriTemplate = "/healthz")]
            Message GetHealthz();

            /// <summary>
            /// Gets the varz message.
            /// </summary>
            /// <returns>A JSON string containing various status information.</returns>
            [WebGet(UriTemplate = "/varz")]
            Message GetVarz();
        }

        /// <summary>
        /// Gets the singleton instance of the Monitoring Server.
        /// </summary>
        private static MonitoringServer Instance
        {
            get
            {
                return instance;
            }
        }

        /// <summary>
        /// Starts the server.
        /// </summary>
        public void Start()
        {
            Uri baseAddress = new Uri("http://" + this.hostName + ":" + this.serverPort);

            WebHttpBinding httpBinding = new WebHttpBinding();
            httpBinding.Security.Mode = WebHttpSecurityMode.TransportCredentialOnly;
            httpBinding.Security.Transport.ClientCredentialType = HttpClientCredentialType.Basic;

            this.host = new WebServiceHost(typeof(MonitoringService), baseAddress);
            this.host.AddServiceEndpoint(typeof(IMonitoringService), httpBinding, baseAddress);

            this.host.Credentials.UserNameAuthentication.CustomUserNamePasswordValidator = new UserCustomAuthentication(this.username, this.password);
            this.host.Credentials.UserNameAuthentication.UserNamePasswordValidationMode = UserNamePasswordValidationMode.Custom;
            this.host.Open();
        }

        /// <summary>
        /// Stops the server.
        /// </summary>
        public void Stop()
        {
            this.host.Close();
        }

        /// <summary>
        /// IDisposable implementation.
        /// </summary>
        public void Dispose()
        {
            if (this.host != null)
            {
                this.host.Close();
            }
        }

        /// <summary>
        /// Triggers the HealthzRequested event.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="Uhuru.Utilities.HealthzRequestEventArgs"/> instance containing the event data.</param>
        /// <returns>A string that contains the requested message.</returns>
        private string TriggerHealthz(object sender, HealthzRequestEventArgs e)
        {
            if (this.HealthzRequested != null)
            {
                this.HealthzRequested(sender, e);
            }

            if (e != null)
            {
                return e.HealthzMessage;
            }
            else
            {
                return string.Empty;
            }
        }

        /// <summary>
        /// Triggers the VarzRequested event.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="Uhuru.Utilities.VarzRequestEventArgs"/> instance containing the event data.</param>
        /// <returns>A string containing the requested message.</returns>
        private string TriggerVarz(object sender, VarzRequestEventArgs e)
        {
            if (this.VarzRequested != null)
            {
                this.VarzRequested(sender, e);
            }

            if (e != null)
            {
                return e.VarzMessage;
            }
            else
            {
                return string.Empty;
            }
        }

        /// <summary>
        /// This is the class that implements the IMonitoringService contract; WCF uses it as a singleton when it publishes the server.
        /// </summary>
        private class MonitoringService : IMonitoringService
        {
            /// <summary>
            /// Gets the healthz message.
            /// </summary>
            /// <returns>
            /// A JSON string containing the healthz message.
            /// </returns>
            public Message GetHealthz()
            {
                string message = MonitoringServer.Instance.TriggerHealthz(this, new HealthzRequestEventArgs());
                return CreateTextresponse(message, "text/plaintext");
            }

            /// <summary>
            /// Gets the varz message.
            /// </summary>
            /// <returns>
            /// A JSON string containing various status information.
            /// </returns>
            public Message GetVarz()
            {
                string message = MonitoringServer.Instance.TriggerVarz(this, new VarzRequestEventArgs());
                return CreateTextresponse(message, "application/json");
            }

            /// <summary>
            /// Creates a Message with a text response.
            /// </summary>
            /// <param name="message">The message.</param>
            /// <param name="contentType">Type of the content.</param>
            /// <returns>A message that has a text body.</returns>
            private static Message CreateTextresponse(string message, string contentType)
            {
                return WebOperationContext.Current.CreateTextResponse(message, contentType);
            }
        }
    }
}
