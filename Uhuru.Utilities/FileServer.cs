using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.ServiceModel.Web;
using System.IO;
using System.ServiceModel.Channels;
using System.ServiceModel;
using System.Net;
using System.ServiceModel.Security;
using System.IdentityModel.Selectors;
using System.Globalization;

namespace Uhuru.Utilities
{
    /// <summary>
    /// This class implements an http server that serves files from local storage.
    /// </summary>
    public sealed class FileServer : IDisposable
    {
        private int serverPort;
        private string serverPhysicalPath;
        private string serverVirtualPath;
        private string username;
        private string password;

        WebServiceHost host;

        /// <summary>
        /// Public constructor.
        /// </summary>
        /// <param name="port">Port used by the server to listen on.</param>
        /// <param name="physicalPath">Root of the path served by the server.</param>
        /// <param name="virtualPath">To get to the files, a caller needs to get http://[ip]:[port]/[virtualPath]/[file]</param>
        /// <param name="serverUserName">Username that is allowed access to the server.</param>
        /// <param name="serverPassword">Password that is allowed access to the server.</param>
        public FileServer(int port, string physicalPath, string virtualPath, string serverUserName, string serverPassword)
        {
            serverPort = port;
            serverPhysicalPath = physicalPath;
            serverVirtualPath = virtualPath;
            username = serverUserName;
            password = serverPassword;
        }

        /// <summary>
        /// Starts the server.
        /// </summary>
        public void Start()
        {
            Uri baseAddress = new Uri("http://localhost:" + serverPort);
            FileServerService service = new FileServerService();

            WebHttpBinding httpBinding = new WebHttpBinding();
            httpBinding.Security.Mode = WebHttpSecurityMode.TransportCredentialOnly;
            httpBinding.Security.Transport.ClientCredentialType = HttpClientCredentialType.Basic;
            

            host = new WebServiceHost(service, baseAddress);
            host.AddServiceEndpoint(typeof(IFileServerService),
                httpBinding, baseAddress);

            host.Credentials.UserNameAuthentication.CustomUserNamePasswordValidator = new UserCustomAuthentication(username, password);
            host.Credentials.UserNameAuthentication.UserNamePasswordValidationMode = UserNamePasswordValidationMode.Custom; 

            ((FileServerService)host.SingletonInstance).Initialize(serverPhysicalPath, serverVirtualPath);
            host.Open();
        }

        /// <summary>
        /// Stops the server.
        /// </summary>
        public void Stop()
        {
            host.Close();
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
        interface IFileServerService
        {
            [WebGet(UriTemplate = "/*")]
            Message GetFile();
        }

        [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single)]
        class FileServerService : IFileServerService
        {
            private string serverPhysicalPath;
            private string serverVirtualPath;

            public void Initialize(string physicalPath, string virtualPath)
            {
                this.serverPhysicalPath = physicalPath;
                this.serverVirtualPath = virtualPath;
            }

            public Message GetFile()
            {
                string path = WebOperationContext.Current.IncomingRequest.UriTemplateMatch.RequestUri.PathAndQuery;
                try
                {
                    return CreateStreamResponse(GetFullFilePath(path));
                }
                catch (IOException ioException)
                {
                    WebOperationContext.Current.OutgoingResponse.StatusCode = System.Net.HttpStatusCode.InternalServerError;
                    WebOperationContext.Current.OutgoingResponse.StatusDescription = ioException.ToString();
                    return null;
                }
            }

            private static Message CreateStreamResponse(string filePath)
            {
                Stream stream = Stream.Null;
                using (FileStream fileStream = File.OpenRead(filePath))
                {
                    fileStream.CopyTo(stream);
                }
                return WebOperationContext.Current.CreateStreamResponse(stream, "application/octet-stream");
            }

            private string GetFullFilePath(string path)
            {
                string filePath = serverPhysicalPath;
                List<string> splitPath = path.Split('/').ToList();
                splitPath.Remove(serverVirtualPath.Trim('/'));

                foreach (string str in splitPath)
                {
                    filePath = Path.Combine(filePath, str);
                }
                return filePath;
            }
        }
    }
}
