using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Uhuru.Utilities;

namespace Uhuru.CloudFoundry.DEA
{
    class FileViewer
    {
        private int StartAttempts;
        public event EventHandler OnStartError;

        System.Timers.Timer FilerStartTimer;

        //Cassini.Server FileViwerServer;

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
            Credentials = new string[] { Guid.NewGuid().ToString("N"), Guid.NewGuid().ToString("N") };
            IsRunning = false;
        }

        public void Start(string DropletsPath)
        {

            FilerStartTimer = new System.Timers.Timer(1000);
            FilerStartTimer.AutoReset = false;
            FilerStartTimer.Elapsed += new System.Timers.ElapsedEventHandler(delegate(object sender, System.Timers.ElapsedEventArgs args)
            {
                bool success = false;

                try
                {

                    //FileViwerServer = new Cassini.Server(Port, "/droplets", DropletsPath);
                    //FileViwerServer.Start();

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
                        throw new ApplicationException();
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
            //throw new NotImplementedException();
        }
    }
}
