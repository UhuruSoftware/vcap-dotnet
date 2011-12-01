using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Configuration;
using System.IO;
using CloudFoundry.Net.IIS.Utilities;

namespace mssqlnode
{
    partial class RubyService : ServiceBase
    {
        uint rubyProcessId;
        ConsoleTools consoleTools = new ConsoleTools(null);
        Process rubyProcess = new Process();
        

        public RubyService()
        {
            InitializeComponent();
        }

        public void Start(string[] args)
        {
            ConsoleTools.AllocConsole();

            string devKitPath = ConfigurationManager.AppSettings["rubyDevkitPath"];

            string pathEnv = Environment.GetEnvironmentVariable("path");
            pathEnv = String.Format(@"{0}\bin;{0}\mingw\bin;{1}", devKitPath, pathEnv);
            Environment.SetEnvironmentVariable("path", pathEnv);

            string envRiDevKit = devKitPath;
            Environment.SetEnvironmentVariable("RI_DEVKIT", envRiDevKit);

            string processArguments = ConfigurationManager.AppSettings[Program.RubyServiceType];
            string processFileName = ConfigurationManager.AppSettings["rubyPath"];

            ProcessTools pt = new ProcessTools();

            rubyProcessId = pt.StartProcess(processFileName, "\"" + processArguments + "\"", 
                ConfigurationManager.AppSettings[Program.RubyServiceType + "Dir"],
                ConfigurationManager.AppSettings[Program.RubyServiceType + "LogType"],
                ConfigurationManager.AppSettings[Program.RubyServiceType + "LogSource"]);


            rubyProcess = Process.GetProcessById((int)rubyProcessId);
            rubyProcess.EnableRaisingEvents = true;
            rubyProcess.Exited += new EventHandler(rubyProcess_Exited);

        }

        protected override void OnStart(string[] args)
        {
            Start(args);
        }

        void rubyProcess_Exited(object sender, EventArgs e)
        {
            Process rubyProcess = (Process)sender;
            if (rubyProcess.ExitCode != 0)
            {
                Process.GetCurrentProcess().Kill();
            }
            else
            {
                this.Stop();
            }
        }

        protected override void OnStop()
        {

            rubyProcess.EnableRaisingEvents = false;

            ConsoleTools.GenerateConsoleCtrlEvent(ConsoleTools.Ctrl_Break_Event, rubyProcessId);
            ConsoleTools.GenerateConsoleCtrlEvent(ConsoleTools.Ctrl_Break_Event, rubyProcessId);

            //TODO: vladi: we need to kill the process if it won't shut down after x seconds
            //Process.Start("taskkill", "/pid " + rubyProcess.Id.ToString());
        }
    }
}
