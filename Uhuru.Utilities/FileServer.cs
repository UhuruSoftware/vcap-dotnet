// -----------------------------------------------------------------------
// <copyright file="FileServer.cs" company="Uhuru Software">
// Copyright (c) 2011 Uhuru Software, Inc., All Rights Reserved
// </copyright>
// -----------------------------------------------------------------------

namespace Uhuru.Utilities
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.ServiceModel;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Security;
    using System.ServiceModel.Web;
    using System.Text;
    using System.Globalization;
    using System.ServiceModel.Description;
    using System.ServiceModel.Dispatcher;
    using System.Reflection;
    
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

        private const int tableRowSize = 46;

        private WebServiceHost host;

        /// <summary>
        /// Initializes a new instance of the FileServer class
        /// </summary>
        /// <param name="port">Port used by the server to listen on.</param>
        /// <param name="physicalPath">Root of the path served by the server.</param>
        /// <param name="virtualPath">To get to the files, a caller needs to get http://[ip]:[port]/[virtualPath]/[file]</param>
        /// <param name="serverUserName">Username that is allowed access to the server.</param>
        /// <param name="serverPassword">Password that is allowed access to the server.</param>
        public FileServer(int port, string physicalPath, string virtualPath, string serverUserName, string serverPassword)
        {
            this.serverPort = port;
            this.serverPhysicalPath = physicalPath;
            this.serverVirtualPath = virtualPath;
            this.username = serverUserName;
            this.password = serverPassword;
        }

        /// <summary>
        /// interface / contract for an endpoint
        /// </summary>
        [ServiceContract]
        private interface IFileServerService
        {
            /// <summary>
            /// gets a file from another endpoint
            /// </summary>
            /// <returns></returns>
            [WebGet(UriTemplate = "/*")]
            Message GetFile();
        }


        /// <summary>
        /// Starts the server.
        /// </summary>
        public void Start()
        {
            Uri baseAddress = new Uri("http://localhost:" + this.serverPort);
            FileServerService service = new FileServerService();

            WebHttpBinding httpBinding = new WebHttpBinding();
            httpBinding.Security.Mode = WebHttpSecurityMode.TransportCredentialOnly;
            httpBinding.Security.Transport.ClientCredentialType = HttpClientCredentialType.Basic;
            
            this.host = new WebServiceHost(service, baseAddress);
            this.host.AddServiceEndpoint(typeof(IFileServerService), httpBinding, baseAddress);

            this.host.Credentials.UserNameAuthentication.CustomUserNamePasswordValidator = new UserCustomAuthentication(username, password);
            this.host.Credentials.UserNameAuthentication.UserNamePasswordValidationMode = UserNamePasswordValidationMode.Custom; 

            ((FileServerService)this.host.SingletonInstance).Initialize(this.serverPhysicalPath, this.serverVirtualPath);
            this.host.Open();
        }

        /// <summary>
        /// Stops the server.
        /// </summary>
        public void Stop()
        {
            this.host.Close();
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

        [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single)]
        private class FileServerService : IFileServerService
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
                Uri uri = WebOperationContext.Current.IncomingRequest.UriTemplateMatch.RequestUri;

                string fullPath = this.GetFullFilePath(uri.PathAndQuery);

                if (Path.GetDirectoryName(fullPath + "\\") == Path.GetDirectoryName(serverPhysicalPath + "\\"))
                {
                    throw new SecurityAccessDeniedException();
                }

                if (File.Exists(fullPath))
                {
                    try
                    {
                        return CreateStreamResponse(fullPath);
                    }
                    catch (IOException exception)
                    {
                        WebOperationContext.Current.OutgoingResponse.StatusCode = System.Net.HttpStatusCode.InternalServerError;
                        WebOperationContext.Current.OutgoingResponse.StatusDescription = exception.ToString();
                        return null;
                    }
                }
                else if (Directory.Exists(fullPath))
                {
                    MemoryStream memoryStream = new MemoryStream();
                    StreamWriter outputStream = new StreamWriter(memoryStream);

                    DirectoryInfo dirInfo = new DirectoryInfo(fullPath);

                    FileInfo [] fileInfos = dirInfo.GetFiles();
                    DirectoryInfo [] dirInfos = dirInfo.GetDirectories();

                    foreach (DirectoryInfo directory in dirInfos)
                    {
                        outputStream.WriteLine(CreateCliLine(directory.Name, "-", tableRowSize, ' '));
                    }

                    foreach (FileInfo file in fileInfos)
                    {
                        
                        outputStream.WriteLine(CreateCliLine(file.Name, GetReadableForm(file.Length), tableRowSize, ' '));
                    }

                    outputStream.Flush();
                    memoryStream.Seek(0, SeekOrigin.Begin);
                    return WebOperationContext.Current.CreateStreamResponse(memoryStream, "text/plain");                    
                }
                else
                {
                    return null;
                }
            }

            private static Message CreateStreamResponse(string filePath)
            {
                MemoryStream memoryStream = null;
                MemoryStream tempMemoryStream = null;

                try
                {
                    using (FileStream fileStream = File.OpenRead(filePath))
                    {
                        tempMemoryStream = new MemoryStream((int)Math.Max(fileStream.Length, 1024 * 1024)); //max 1 MB file
                        fileStream.CopyTo(tempMemoryStream, tempMemoryStream.Capacity);
                    }
                    tempMemoryStream.Seek(0, SeekOrigin.Begin);
                    memoryStream = tempMemoryStream;
                    tempMemoryStream = null;
                }
                finally
                {
                    if (tempMemoryStream != null)
                    {
                        tempMemoryStream.Close();
                    }
                }

                return WebOperationContext.Current.CreateStreamResponse(memoryStream, "text/plain");
            }

            private string GetFullFilePath(string path)
            {
                string filePath = this.serverPhysicalPath;
                List<string> splitPath = path.Split('/').ToList();
                splitPath.Remove(this.serverVirtualPath.Trim('/'));

                foreach (string str in splitPath)
                {
                    filePath = Path.Combine(filePath, str);
                }

                return filePath;
            }

            /// <summary>
            /// converts a numeric file size into a human readable one
            /// </summary>
            /// <param name="size">the size to convert (e.g. 800)</param>
            /// <returns>a nicely formatted string (e.g. 800B)</returns>
            private static string GetReadableForm(long size)
            {
                string[] sizes = { "B", "K", "M", "G" };

                int order = 0;
                while (size >= 1024 && order + 1 < sizes.Length)
                {
                    order++;
                    size = size / 1024;
                }

                string result = string.Format(CultureInfo.InvariantCulture, "{0:0.##}{1}", size, sizes[order]);

                return result;
            }

            private static string CreateCliLine(string left, string right, int lineSize, char fillChar)
            {
                return left + new string(fillChar, Math.Max(8, lineSize - left.Length - right.Length)) + right;
            }

        }
    }
}
