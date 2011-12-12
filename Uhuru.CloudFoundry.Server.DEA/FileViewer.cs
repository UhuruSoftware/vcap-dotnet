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
            Credentials = new string[] { Utilities.Credentials.GenerateCredential(), Utilities.Credentials.GenerateCredential() };
            IsRunning = false;
        }

        public void Start(string dropletsPath)
        {
            FilerStartTimer = new System.Timers.Timer(500);
            FilerStartTimer.AutoReset = false;
            FilerStartTimer.Elapsed += new System.Timers.ElapsedEventHandler(delegate(object sender, System.Timers.ElapsedEventArgs args)
            {
                bool success = false;

                try
                {
                    FileViewerServer = new FileServer(Port, dropletsPath, @"/droplets", Credentials[0], Credentials[1]);
                    FileViewerServer.Start();

                    Logger.Info(Strings.FileServiceStartedOnPort, Port);
                    StartAttempts += 1;
                    success = true;
                }
                catch (Exception ex)
                {
                    Logger.Fatal(Strings.FilerServiceFailedToStart, Port, ex.ToString());
                    StartAttempts += 1;
                    if (StartAttempts >= 5)
                    {
                        Logger.Fatal(Strings.GivingUpOnTryingToStartFiler);
                        OnStartError(null, null);
                    }
                }

                if (success)
                {
                    IsRunning = true;

                    FilerStartTimer.Enabled = false;
                    FilerStartTimer = null;
                }
                else
                {
                    FilerStartTimer.Enabled = true;
                }
            });

            FilerStartTimer.Enabled = true;
        }

        public void Stop()
        {
            if(FileViewerServer != null)
                FileViewerServer.Stop();
        }

        ~FileViewer()
        {
            Stop();
        }

    }
}
