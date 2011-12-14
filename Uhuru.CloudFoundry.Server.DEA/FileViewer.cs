// -----------------------------------------------------------------------
// <copyright file="FileViewer.cs" company="Uhuru Software, Inc.">
// Copyright (c) 2011 Uhuru Software, Inc., All Rights Reserved
// </copyright>
// -----------------------------------------------------------------------

namespace Uhuru.CloudFoundry.DEA
{
    using System;
    using Uhuru.Utilities;

    /// <summary>
    /// A http file viewer.
    /// </summary>
    public class FileViewer : IDisposable
    {
        /// <summary>
        /// The number of times the file viewer tried to start.
        /// </summary>
        private int startAttempts;

        /// <summary>
        /// Start timestamp.
        /// </summary>
        private System.Timers.Timer filerStartTimer;

        /// <summary>
        /// The file server used for the file viewer.
        /// </summary>
        private FileServer fileViewerServer;
        
        /// <summary>
        /// Initializes a new instance of the <see cref="FileViewer"/> class.
        /// </summary>
        public FileViewer()
        {
            this.Credentials = new string[] { Utilities.Credentials.GenerateCredential(), Utilities.Credentials.GenerateCredential() };
            this.IsRunning = false;
        }

        /// <summary>
        /// Occurs when the file viewer was unable to start.
        /// </summary>
        public event EventHandler OnStartError;

        /// <summary>
        /// Gets or sets a value indicating whether this instance is running.
        /// </summary>
        /// <value>
        ///     <c>true</c> if this instance is running; otherwise, <c>false</c>.
        /// </value>
        public bool IsRunning
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the TCP port.
        /// </summary>
        public int Port
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the credentials to the file viewer.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "It is used for JSON (de)serialization.")]
        public string[] Credentials
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the URI.
        /// </summary>
        public Uri Uri
        {
            get;
            set;
        }

        /// <summary>
        /// Starts the file viewer. This method is non-blocking.
        /// </summary>
        /// <param name="dropletsPath">The droplets path.</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "We expect failure, and error must not bubble up.")]
        public void Start(string dropletsPath)
        {
            this.filerStartTimer = new System.Timers.Timer(1000);
            this.filerStartTimer.AutoReset = false;
            this.filerStartTimer.Elapsed += new System.Timers.ElapsedEventHandler(delegate(object sender, System.Timers.ElapsedEventArgs args)
            {
                bool success = false;

                try
                {
                    this.fileViewerServer = new FileServer(this.Port, dropletsPath, @"/droplets", this.Credentials[0], this.Credentials[1]);
                    this.fileViewerServer.Start();

                    Logger.Info(Strings.FileServiceStartedOnPort, this.Port);
                    this.startAttempts += 1;
                    success = true;
                }
                catch (Exception ex)
                {
                    Logger.Fatal(Strings.FilerServiceFailedToStart, this.Port, ex.ToString());
                    this.startAttempts += 1;
                    if (this.startAttempts >= 5)
                    {
                        Logger.Fatal(Strings.GivingUpOnTryingToStartFiler);
                        this.OnStartError(null, null);
                    }
                }

                if (success)
                {
                    this.IsRunning = true;

                    this.filerStartTimer.Enabled = false;
                    this.filerStartTimer = null;
                }
                else
                {
                    this.filerStartTimer.Enabled = true;
                }
            });

            this.filerStartTimer.Enabled = true;
        }

        /// <summary>
        /// Stops the file viewer.
        /// </summary>
        public void Stop()
        {
            if (this.fileViewerServer != null)
            {
                this.fileViewerServer.Stop();
            }
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources
        /// </summary>
        /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (this.filerStartTimer != null)
                {
                    this.filerStartTimer.Close();
                }
            }
        }
    }
}
