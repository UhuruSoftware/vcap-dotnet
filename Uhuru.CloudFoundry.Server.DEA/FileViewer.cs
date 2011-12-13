// -----------------------------------------------------------------------
// <copyright file="FileViewer.cs" company="Uhuru Software">
// Copyright (c) 2011 Uhuru Software, Inc., All Rights Reserved
// </copyright>
// -----------------------------------------------------------------------

namespace Uhuru.CloudFoundry.DEA
{
    using System;
    using Uhuru.Utilities;

    public class FileViewer
    {
        private int StartAttempts;

        public event EventHandler OnStartError;

        System.Timers.Timer FilerStartTimer;

        FileServer FileViewerServer;

        public bool IsRunning
        {
            get;
            set;
        }

        public int Port
        {
            get;
            set;
        }

        public string[] Credentials
        {
            get;
            set;
        }

        public string Uri
        {
            get;
            set;
        }

        public FileViewer()
        {
            this.Credentials = new string[] { Utilities.Credentials.GenerateCredential(), Utilities.Credentials.GenerateCredential() };
            this.IsRunning = false;
        }

        public void Start(string dropletsPath)
        {
            this.FilerStartTimer = new System.Timers.Timer(500);
            this.FilerStartTimer.AutoReset = false;
            this.FilerStartTimer.Elapsed += new System.Timers.ElapsedEventHandler(delegate(object sender, System.Timers.ElapsedEventArgs args)
            {
                bool success = false;

                try
                {
                    this.FileViewerServer = new FileServer(this.Port, dropletsPath, @"/droplets", this.Credentials[0], this.Credentials[1]);
                    this.FileViewerServer.Start();

                    Logger.Info(Strings.FileServiceStartedOnPort, this.Port);
                    this.StartAttempts += 1;
                    success = true;
                }
                catch (Exception ex)
                {
                    Logger.Fatal(Strings.FilerServiceFailedToStart, this.Port, ex.ToString());
                    this.StartAttempts += 1;
                    if (this.StartAttempts >= 5)
                    {
                        Logger.Fatal(Strings.GivingUpOnTryingToStartFiler);
                        this.OnStartError(null, null);
                    }
                }

                if (success)
                {
                    this.IsRunning = true;

                    this.FilerStartTimer.Enabled = false;
                    this.FilerStartTimer = null;
                }
                else
                {
                    this.FilerStartTimer.Enabled = true;
                }
            });

            this.FilerStartTimer.Enabled = true;
        }

        public void Stop()
        {
            if (this.FileViewerServer != null)
            {
                this.FileViewerServer.Stop();
            }
        }

        ~FileViewer()
        {
            this.Stop();
        }
    }
}
