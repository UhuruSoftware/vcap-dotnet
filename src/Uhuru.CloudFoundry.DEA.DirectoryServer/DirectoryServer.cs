// -----------------------------------------------------------------------
// <copyright file="DirectoryServer.cs" company="Uhuru Software, Inc.">
// Copyright (c) 2011 Uhuru Software, Inc., All Rights Reserved
// </copyright>
// -----------------------------------------------------------------------

namespace Uhuru.CloudFoundry.DEA.DirectoryServer
{
    using System;
    using System.Collections.Specialized;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Net.Mime;
    using System.Text;
    using System.Threading;
    using Uhuru.Configuration;
    using Uhuru.Utilities;

    /// <summary>
    /// Package directoryserver implements a HTTP-based directory server that can list
    /// directories and stream/dump files based on the path specified in the HTTP
    /// request. All HTTP requests are validated with a HTTP end-point in the
    /// DEA co-located in the same host at a specified port. If validation with the
    /// DEA is successful, the HTTP request is served. Otherwise, the same HTTP
    /// response from the DEA is served as response to the HTTP request.
    /// Directory listing lists sub-directories and files contained inside the
    /// directory along with the file sizes. Streaming of files uses HTTP chunked
    /// transfer encoding. The server also handles HTTP byte range requests.
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1724:TypeNamesShouldNotMatchNamespaces", Justification = "Keeping same names as Linux GO version.")]
    public sealed class DirectoryServer : IDisposable
    {
        /// <summary>
        /// The char used to fill in the rows of the directory listing table so it's properly formatted.
        /// </summary>
        private const char DirectoryListingTableFillChar = ' ';

        /// <summary>
        /// Table row size for directory listing.
        /// </summary>
        private const int TableRowSize = 46;

        /// <summary>
        /// Interface to the DEA used to grab path information.
        /// </summary>
        private IDeaClient deaClient;

        /// <summary>
        /// Period after which to timeout if a file that is tailed has not changed
        /// </summary>
        private int streamingTimeout;

        /// <summary>
        /// The http listener that serves HTTP requests.
        /// </summary>
        private HttpListener listener = new HttpListener();

        /// <summary>
        /// The host on which to listen.
        /// </summary>
        private string domain;

        /// <summary>
        /// An ID used to identify the directory server and register it with the VCAP router.
        /// </summary>
        private Guid directoryServerId;

        /// <summary>
        /// Configuration of the DEA.
        /// </summary>
        private DEAElement config;

        /// <summary>
        /// Initializes a new instance of the <see cref="DirectoryServer"/> class.
        /// </summary>
        /// <param name="domain">The domain of the cloud.</param>
        /// <param name="config">The config of the DEA.</param>
        /// <param name="deaClient">The DeaClient that can lookup paths.</param>
        public DirectoryServer(string domain, DEAElement config, IDeaClient deaClient)
        {
            if (config == null)
            {
                throw new ArgumentNullException("config");
            }

            this.directoryServerId = Guid.NewGuid();
            this.config = config;
            this.domain = domain;
            this.deaClient = deaClient;
            this.streamingTimeout = config.DirectoryServer.StreamingTimeoutMS;
        }

        /// <summary>
        /// Gets the external hostname of the directory server.
        /// </summary>
        public string ExternalHostName
        {
            get
            {
                return string.Format(CultureInfo.InvariantCulture, "{0}.{1}", this.directoryServerId.ToString("N"), this.domain);
            }
        }

        /// <summary>
        /// Gets the port of the directory server.
        /// </summary>
        public int Port
        {
            get
            {
                return this.config.DirectoryServer.V2Port;
            }
        }

        /// <summary>
        /// Starts the directory server at the specified host, port. Validates HTTP
        /// requests with the DEA's HTTP server which serves requests on the same host and
        /// specified DAE port.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "If anything bad happens, make sure the server stays online.")]
        public void Start()
        {
            this.listener.Start();
            this.listener.Prefixes.Add(string.Format(CultureInfo.InvariantCulture, "http://{0}:{1}/", NetworkInterface.GetLocalIPAddress(this.config.LocalRoute), this.config.DirectoryServer.V2Port));

            ThreadStart listenerThreadStart = new ThreadStart(
                () =>
                {
                    while (this.listener.IsListening)
                    {
                        try
                        {
                            HttpListenerContext request = this.listener.GetContext();
                            ThreadPool.QueueUserWorkItem(ServeHttp, request);
                        }
                        catch (Exception ex)
                        {
                            Logger.Error(Strings.HttpListenerError, ex.ToString());
                        }
                    }
                });

            Thread listenerThread = new Thread(listenerThreadStart);
            listenerThread.IsBackground = true;
            listenerThread.Name = "DEA Directory Server Thread";

            listenerThread.Start();
        }

        /// <summary>
        /// Stops this instance of the Directory Server.
        /// </summary>
        public void Stop()
        {
            if (this.listener != null)
            {
                this.listener.Stop();
            }
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            if (this.listener != null)
            {
                this.listener.Close();
            }
        }

        /// <summary>
        /// Returns a string representation of the file size.
        /// </summary>
        /// <param name="size">The size.</param>
        /// <returns>A string with a human readable size.</returns>
        private static string GetFileSizeFormat(long size)
        {
            if (size >= ((long)1 << 40))
            {
                return ((double)size / (1 << 40)).ToString("0.00T", CultureInfo.InvariantCulture);
            }

            if (size >= ((long)1 << 30))
            {
                return ((double)size / (1 << 30)).ToString("0.00G", CultureInfo.InvariantCulture);
            }

            if (size >= ((long)1 << 20))
            {
                return ((double)size / (1 << 20)).ToString("0.00M", CultureInfo.InvariantCulture);
            }

            if (size >= ((long)1 << 10))
            {
                return ((double)size / (1 << 10)).ToString("0.00K", CultureInfo.InvariantCulture);
            }

            return size.ToString("0B", CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// Writes the entity not found response in the HTTP response and sets the HTTP
        /// response status code to 400. 
        /// </summary>
        /// <param name="context">Http context to respond to.</param>
        private static void WriteEntityNotFound(HttpListenerContext context)
        {
            try
            {
                string response = Strings.EntityNotFound;

                context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                context.Response.ContentType = MediaTypeNames.Text.Plain;
                context.Response.Headers["X-Cascade"] = "pass";

                using (StreamWriter writer = new StreamWriter(context.Response.OutputStream))
                {
                    writer.Write(response);
                }
            }
            finally
            {
                context.Response.Close();
            }
        }

        /// <summary>
        /// Prefixes the error message indicating an internal server error.
        /// Writes the new error message in the HTTP response and sets the HTTP response
        /// status code to 500.
        /// </summary>
        /// <param name="error">The error.</param>
        /// <param name="context">Http context to respond to.</param>
        private static void WriteServerError(string error, HttpListenerContext context)
        {
            try
            {
                string response = string.Format(CultureInfo.InvariantCulture, Strings.CantServeRequest, error);

                context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                context.Response.ContentType = MediaTypeNames.Text.Plain;

                using (StreamWriter writer = new StreamWriter(context.Response.OutputStream))
                {
                    writer.Write(response);
                }
            }
            finally
            {
                context.Response.Close();
            }
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
        /// Writes the directory listing of the directory path in the HTTP response.
        /// Files in the directory are reported along with their sizes.
        /// </summary>
        /// <param name="dirPath">The directory path.</param>
        /// <param name="context">Http context to respond to.</param>
        private static void ListDir(string dirPath, HttpListenerContext context)
        {
            try
            {
                StringBuilder body = new StringBuilder();

                DirectoryInfo dirInfo = new DirectoryInfo(dirPath);
                FileInfo[] fileInfos = dirInfo.GetFiles();
                DirectoryInfo[] dirInfos = dirInfo.GetDirectories();

                foreach (DirectoryInfo directory in dirInfos)
                {
                    body.AppendLine(CreateCLITableRow(directory.Name + "/", "-"));
                }

                foreach (FileInfo file in fileInfos)
                {
                    body.AppendLine(CreateCLITableRow(file.Name, GetFileSizeFormat(file.Length)));
                }

                context.Response.ContentType = MediaTypeNames.Text.Plain;

                using (StreamWriter writer = new StreamWriter(context.Response.OutputStream))
                {
                    writer.Write(body.ToString());
                }
            }
            finally
            {
                context.Response.Close();
            }
        }

        /// <summary>
        /// Dumps the contents of the specified file in the HTTP response.
        /// Returns an error if there is a problem in opening/closing the file.
        /// </summary>
        /// <param name="path">The path.</param>
        /// <param name="context">Http context to respond to.</param>
        private static void DumpFile(string path, HttpListenerContext context)
        {
            try
            {
                string mimeType = MimeTypeDetection.GetMimeFromFile(path);

                context.Response.ContentType = mimeType;
                context.Response.SendChunked = true;

                using (FileStream handle = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                {
                    handle.CopyTo(context.Response.OutputStream);
                }
            }
            finally
            {
                context.Response.Close();
            }
        }

        /// <summary>
        /// Detects the type of the content in a file.
        /// </summary>
        /// <param name="path">The path to the file.</param>
        /// <returns>A MIME type.</returns>
        private static string DetectContentType(string path)
        {
            return MimeTypeDetection.GetMimeFromFile(path);
        }

        /// <summary>
        /// Lists directory, or writes file contents in the HTTP response as per the
        /// the response received from the DEA. If the "tail" parameter is part of
        /// the HTTP request, then the file contents are streamed through chunked
        /// HTTP transfer encoding. Otherwise, the entire file is dumped in the HTTP
        /// response.
        /// Writes appropriate errors and status codes in the HTTP response if there is
        /// a problem in reading the file or directory.
        /// </summary>
        /// <param name="path">The path.</param>
        /// <param name="tail">if set to <c>true</c> it means we tail the file live.</param>
        /// <param name="context">Http context to respond to.</param>
        private void ListPath(string path, bool tail, HttpListenerContext context)
        {
            if (File.Exists(path))
            {
                this.WriteFile(path, tail, context);
            }
            else if (Directory.Exists(path))
            {
                DirectoryServer.ListDir(path, context);
            }
            else
            {
                Logger.Warning(Strings.PathNotFound, path);
                DirectoryServer.WriteEntityNotFound(context);
            }
        }

        /// <summary>
        /// If validation with the DEA is successful, the HTTP request is served.
        /// Otherwise, the same HTTP response from the DEA is served as response to
        /// the HTTP request.
        /// </summary>
        /// <param name="listenerContext">Http context to respond to.</param>
        private void ServeHttp(object listenerContext)
        {
            HttpListenerContext context = (HttpListenerContext)listenerContext;

            Uri uri = context.Request.Url;

            PathLookupResponse response = this.deaClient.LookupPath(uri);

            if (response.Error != null)
            {
                Logger.Warning(Strings.ErrorInLookupPath, response.Error);
                DirectoryServer.WriteServerError(Strings.WinDEADidNotRespondProperly, context);
            }
            else
            {
                NameValueCollection queryStrings = System.Web.HttpUtility.ParseQueryString(uri.Query);
                bool tail = queryStrings.AllKeys.Select(key => queryStrings.GetValues(key).Contains("tail")).Any(v => v);

                this.ListPath(response.Path, tail, context);
            }
        }

        /// <summary>
        /// Writes the file to the HTTP connection.
        /// </summary>
        /// <param name="path">The path.</param>
        /// <param name="tail">If set to <c>true</c> it means we stram the contents of the file live.</param>
        /// <param name="context">Http context to respond to.</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "If anything bad happens, fail gracefully and tell the client.")]
        private void WriteFile(string path, bool tail, HttpListenerContext context)
        {
            try
            {
                if (tail)
                {
                    string mimeType = DetectContentType(path);

                    context.Response.StatusCode = (int)System.Net.HttpStatusCode.OK;
                    context.Response.ContentType = mimeType;

                    StreamHandler streamHandler = new StreamHandler(path, TimeSpan.FromMilliseconds(50), TimeSpan.FromMilliseconds(this.streamingTimeout), context);
                    streamHandler.Start();
                }
                else
                {
                    DumpFile(path, context);
                }
            }
            catch (Exception ex)
            {
                DirectoryServer.WriteServerError(ex.Message, context);
            }
        }
    }
}