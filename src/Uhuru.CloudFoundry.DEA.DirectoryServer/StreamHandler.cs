// -----------------------------------------------------------------------
// <copyright file="StreamHandler.cs" company="Uhuru Software, Inc.">
// Copyright (c) 2011 Uhuru Software, Inc., All Rights Reserved
// </copyright>
// -----------------------------------------------------------------------

namespace Uhuru.CloudFoundry.DEA.DirectoryServer
{
    using System;
    using System.Globalization;
    using System.IO;
    using System.Net;
    using System.Net.Mime;
    using System.Threading;
    using Uhuru.Utilities;

    /// <summary>
    /// This class is used to tail a file and stream it to a client.
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1710:IdentifiersShouldHaveCorrectSuffix", Justification = "Keeping the same name as Linux GO version.")]
    public class StreamHandler
    {
        /// <summary>
        /// The file to tail.
        /// </summary>
        private string filePath;

        /// <summary>
        /// Interval in which to try and read from the file.
        /// </summary>
        private TimeSpan flushInterval;

        /// <summary>
        /// Timeout interval if file has not changed.
        /// </summary>
        private TimeSpan idleTimeout;

        /// <summary>
        /// The HTTP context this instance of StreamHandler is bound to.
        /// </summary>
        private HttpListenerContext context;

        /// <summary>
        /// Initializes a new instance of the <see cref="StreamHandler"/> class.
        /// </summary>
        /// <param name="filePath">The file path to stream.</param>
        /// <param name="flushInterval">The flush interval.</param>
        /// <param name="idleTimeout">The idle timeout.</param>
        /// <param name="context">Http context to respond to.</param>
        public StreamHandler(string filePath, TimeSpan flushInterval, TimeSpan idleTimeout, HttpListenerContext context)
        {
            this.filePath = filePath;
            this.flushInterval = flushInterval;
            this.idleTimeout = idleTimeout;
            this.context = context;
        }

        /// <summary>
        /// Starts the file tail.
        /// </summary>
        public void Start()
        {
            ThreadPool.QueueUserWorkItem((_) => this.WatchFile());
        }

        /// <summary>
        /// Watches the file that is tailed and sets the proper flags, so when WCF reads we know how to reply.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "If anything bad happens, we log the error and mark the current stream as being in an error state.")]
        private void WatchFile()
        {
            try
            {
                string dir = Path.GetDirectoryName(this.filePath);
                string file = Path.GetFileName(this.filePath);

                FileInfo fileInfo = new FileInfo(filePath);
                fileInfo.Refresh();
                long fileLength = fileInfo.Length;

                using (FileSystemWatcher watcher = new FileSystemWatcher(dir, file))
                {
                    watcher.EnableRaisingEvents = false;
                    bool stop = false;

                    this.context.Response.SendChunked = true;
                    this.context.Response.OutputStream.Flush();

                    while (!stop)
                    {
                        WaitForChangedResult result = watcher.WaitForChanged(WatcherChangeTypes.All, (int)this.idleTimeout.TotalMilliseconds);
                        if (result.TimedOut)
                        {
                            stop = true;
                            break;
                        }

                        if (result.ChangeType == WatcherChangeTypes.Renamed || result.ChangeType == WatcherChangeTypes.Deleted)
                        {
                            stop = true;
                            break;
                        }
                        else if (result.ChangeType == WatcherChangeTypes.Changed || result.ChangeType == WatcherChangeTypes.Created)
                        {
                            try
                            {
                                long newFileLength = new FileInfo(filePath).Length;
                                if (newFileLength != 0)
                                {
                                    int bytesToRead = (int)(newFileLength - fileLength);
                                    if (bytesToRead > 0)
                                    {
                                        using (FileStream tailFile = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete))
                                        {
                                            tailFile.Seek(bytesToRead * -1, SeekOrigin.End);
                                            try
                                            {
                                                tailFile.CopyTo(this.context.Response.OutputStream);
                                                this.context.Response.OutputStream.Flush();
                                            }
                                            catch (HttpListenerException)
                                            {
                                                stop = true;
                                                break;
                                            }
                                        }
                                    }
                                    fileLength = newFileLength;
                                }
                            }
                            catch
                            {
                                stop = true;
                                break;
                            }
                        }
                    }
                }                
            }
            catch (Exception ex)
            {
                Logger.Error(Strings.ErrorSettingUpFileSystemWatcher, ex.ToString());
                this.context.Response.ContentType = MediaTypeNames.Text.Plain;
                this.context.Response.StatusCode = (int)System.Net.HttpStatusCode.InternalServerError;

                using (StreamWriter writer = new StreamWriter(this.context.Response.OutputStream))
                {
                    writer.Write(string.Format(CultureInfo.InvariantCulture, Strings.ErrorSettingUpFileSystemWatcher, ex.Message));
                }
            }
            finally
            {
                this.context.Response.OutputStream.Flush();
                this.context.Response.Close();
            }
        }
    }
}