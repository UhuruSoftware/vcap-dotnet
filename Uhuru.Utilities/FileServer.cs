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
    public class UserCustomAuthentication : UserNamePasswordValidator
    {

        string validUsername;
        string validPassword;

        public UserCustomAuthentication(string username, string password)
        {
            validPassword = password;
            validUsername = username;
        }

        public override void Validate(string userName, string password)
        {
            if (null == userName || null == password)
            {
                throw new ArgumentNullException();
            }

            if (!(userName == validUsername && password == validPassword))
            {
                throw new FaultException("Unknown Username or Incorrect Password");
            }
        }
    }
    

    public class FileServer
    {
        private int serverPort;
        private string serverPhysicalPath;
        private string serverVirtualPath;
        private string username;
        private string password;

        WebServiceHost host;

        public FileServer(int port, string physicalPath, string virtualPath, string serverUserName, string serverPassword)
        {
            serverPort = port;
            serverPhysicalPath = physicalPath;
            serverVirtualPath = virtualPath;
            username = serverUserName;
            password = serverPassword;
        }

        public void Start()
        {
            Uri baseAddress = new Uri("http://localhost:" + serverPort);
            Service service = new Service();

            WebHttpBinding httpBinding = new WebHttpBinding();
            httpBinding.Security.Mode = WebHttpSecurityMode.TransportCredentialOnly;
            httpBinding.Security.Transport.ClientCredentialType = HttpClientCredentialType.Basic;
            

            host = new WebServiceHost(service, baseAddress);
            host.AddServiceEndpoint(typeof(IService),
                httpBinding, baseAddress);

            host.Credentials.UserNameAuthentication.CustomUserNamePasswordValidator = new UserCustomAuthentication(username, password);
            host.Credentials.UserNameAuthentication.UserNamePasswordValidationMode = UserNamePasswordValidationMode.Custom; 

            ((Service)host.SingletonInstance).Initialize(serverPhysicalPath, serverVirtualPath);
            host.Open();
        }

        public void Stop()
        {
            host.Close();
        }
    }

    [ServiceContract]
    interface IService
    {
        [WebGet(UriTemplate = "/*")]
        Message GetFile();
    }

    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single)]
    class Service : IService
    {
        private string serverPhysicalPath;
        private string serverVirtualPath;

        public void Initialize(string physicalPath, string virtualPath)
        {
            serverPhysicalPath = physicalPath;
            serverVirtualPath = virtualPath;
        }

        public void IssueAuthenticationChallenge()
        {
            string Realm = "test";
            
            WebOperationContext context = WebOperationContext.Current;

            context.OutgoingResponse.StatusCode = HttpStatusCode.Unauthorized;
            context.OutgoingResponse.Headers[HttpResponseHeader.WwwAuthenticate] = String.Format(CultureInfo.InvariantCulture, "Basic realm =\"{0}\"", Realm);
        }


        public Message GetFile()
        {
            string path = WebOperationContext.Current.IncomingRequest.UriTemplateMatch.RequestUri.PathAndQuery;
            try
            {
                return CreateStreamResponse(GetFullFilePath(path));
            }
            catch (Exception ex)
            {
                WebOperationContext.Current.OutgoingResponse.StatusCode = System.Net.HttpStatusCode.InternalServerError;
                WebOperationContext.Current.OutgoingResponse.StatusDescription = ex.ToString();
                return null;
            }
        }

        private Message CreateStreamResponse(string filePath)
        {
            Stream stream = Stream.Null;
            using(FileStream fileStream = File.OpenRead(filePath))
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
