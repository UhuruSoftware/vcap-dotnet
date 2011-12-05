using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

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

                    Logger.info(String.Format("File service started on port: {0}", Port));
                    StartAttempts += 1;
                    success = true;
                }
                catch (Exception ex)
                {
                    Logger.fatal(String.Format("Filer service failed to start: {0} already in use?: {1}", Port, ex.ToString()));
                    StartAttempts += 1;
                    if (StartAttempts >= 5)
                    {
                        Logger.fatal("Giving up on trying to start filer, exiting...");
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
