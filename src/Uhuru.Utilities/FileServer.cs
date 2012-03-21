// -----------------------------------------------------------------------
// <copyright file="FileServer.cs" company="Uhuru Software, Inc.">
// Copyright (c) 2011 Uhuru Software, Inc., All Rights Reserved
// </copyright>
// -----------------------------------------------------------------------

namespace Uhuru.Utilities
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.ServiceModel;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Security;
    using System.ServiceModel.Web;

    /// <summary>
    /// This class implements an http server that serves files from local storage.
    /// </summary>
    public sealed class FileServer : IDisposable
    {
        /// <summary>
        /// Table row size for directory listing.
        /// </summary>
        private const int TableRowSize = 46;

        /// <summary>
        /// Port on which the file server is listening.
        /// </summary>
        private int serverPort;

        /// <summary>
        /// Path to the directory served by the server.
        /// </summary>
        private string serverPhysicalPath;
        
        /// <summary>
        /// Virtual path of server (clients use the URI http://[host]:[port]/[virtual path]).
        /// </summary>
        private string serverVirtualPath;
        
        /// <summary>
        /// Username for basic http authentication.
        /// </summary>
        private string username;
        
        /// <summary>
        /// Password for basic http authentication.
        /// </summary>
        private string password;

        /// <summary>
        /// WCF service host for the FileServer.
        /// </summary>
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
            /// Gets a file from the server.
            /// </summary>
            /// <returns>A message containing a stream to the requested file.</returns>
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

            this.host.Credentials.UserNameAuthentication.CustomUserNamePasswordValidator = new UserCustomAuthentication(this.username, this.password);
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
            if (this.host != null)
            {
                this.host.Close();
            }
        }

        #endregion

        /// <summary>
        /// This is the WCF service class that is used to host the File Server.
        /// </summary>
        [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single)]
        private class FileServerService : IFileServerService
        {
            /// <summary>
            /// The char used to fill in the rows of the directory listing table so it's properly formatted.
            /// </summary>
            private const char DirectoryListingTableFillChar = ' ';

            /// <summary>
            /// Path to the directory served by the service.
            /// </summary>
            private string serverPhysicalPath;

            /// <summary>
            /// Virtual path of the http server.
            /// </summary>
            private string serverVirtualPath;

            /// <summary>
            /// Initializes the service with the physical and virtual path.
            /// </summary>
            /// <param name="physicalPath">The physical path.</param>
            /// <param name="virtualPath">The virtual path.</param>
            public void Initialize(string physicalPath, string virtualPath)
            {
                this.serverPhysicalPath = physicalPath;
                this.serverVirtualPath = virtualPath;
            }

            /// <summary>
            /// Gets a file from the server.
            /// </summary>
            /// <returns>
            /// A message containing a stream to the requested file.
            /// </returns>
            [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "Code has been refactored as per MSDN, but the warning is still generated.")]
            public Message GetFile()
            {
                Uri uri = WebOperationContext.Current.IncomingRequest.UriTemplateMatch.RequestUri;
                string uriPath = Uri.UnescapeDataString(uri.PathAndQuery);
                string fullPath = this.GetFullFilePath(uriPath);

                if (Path.GetDirectoryName(fullPath + "\\") == Path.GetDirectoryName(this.serverPhysicalPath + "\\"))
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
                    MemoryStream tempMemoryStream = null;
                    MemoryStream memoryStream = null;

                    try
                    {
                        tempMemoryStream = new MemoryStream();
                        StreamWriter outputStream = new StreamWriter(tempMemoryStream);

                        DirectoryInfo dirInfo = new DirectoryInfo(fullPath);
                        FileInfo[] fileInfos = dirInfo.GetFiles();
                        DirectoryInfo[] dirInfos = dirInfo.GetDirectories();

                        foreach (DirectoryInfo directory in dirInfos)
                        {
                            outputStream.WriteLine(CreateCLITableRow(directory.Name, "-"));
                        }

                        foreach (FileInfo file in fileInfos)
                        {
                            outputStream.WriteLine(CreateCLITableRow(file.Name, GetReadableForm(file.Length)));
                        }

                        outputStream.Flush();
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
                else
                {
                    return null;
                }
            }

            /// <summary>
            /// Creates a Message that contains an open stream to the contents of the specified file.
            /// </summary>
            /// <param name="filePath">The file path.</param>
            /// <returns>A Message that can stream the contents of the file.</returns>
            [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "Code has been refactored as per MSDN, but the warning is still generated.")]
            private static Message CreateStreamResponse(string filePath)
            {
                MemoryStream memoryStream = null;
                MemoryStream tempMemoryStream = null;

                using (FileStream fileStream = File.Open(filePath, FileMode.Open, FileAccess.Read, FileShare.Delete | FileShare.ReadWrite))
                {
                    try
                    {
                        tempMemoryStream = new MemoryStream((int)Math.Max(fileStream.Length, 1024 * 1024 * 10)); // max 1 MB file
                        fileStream.CopyTo(tempMemoryStream, tempMemoryStream.Capacity);
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
                }

                return WebOperationContext.Current.CreateStreamResponse(memoryStream, "text/plain");
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

            /// <summary>
            /// Creates the cli line.
            /// </summary>
            /// <param name="leftColumn">The left column of a table.</param>
            /// <param name="rightColumn">The right column of the table.</param>
            /// <returns>A string that is formatted as a table row with two columns.</returns>
            private static string CreateCLITableRow(string leftColumn, string rightColumn)
            {
                return leftColumn + new string(DirectoryListingTableFillChar, Math.Max(8, TableRowSize - leftColumn.Length - rightColumn.Length)) + rightColumn;
            }

            /// <summary>
            /// Gets the full path of a specified file or directory.
            /// </summary>
            /// <param name="path">The path.</param>
            /// <returns>The absolute path to the file or directory.</returns>
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
        }
    }
}
