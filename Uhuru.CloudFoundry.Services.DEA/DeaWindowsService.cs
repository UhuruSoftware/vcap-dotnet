using System.ServiceProcess;

namespace Uhuru.CloudFoundry.DEA.WindowsService
{
    partial class DeaWindowsService : ServiceBase
    {
        Agent agent;

        public DeaWindowsService()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            // TODO: Add code here to start your service.
        }

        protected override void OnStop()
        {
            agent.Shutdown();
        }

        internal void Start(string[] p)
        {
            agent = new Agent();
            agent.Run();
        }
    }
}
