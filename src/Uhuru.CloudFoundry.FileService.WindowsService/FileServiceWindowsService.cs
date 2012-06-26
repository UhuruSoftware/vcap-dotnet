// -----------------------------------------------------------------------
// <copyright file="FileServiceWindowsService.cs" company="Uhuru Software, Inc.">
// Copyright (c) 2011 Uhuru Software, Inc., All Rights Reserved
// </copyright>
// -----------------------------------------------------------------------

namespace Uhuru.CloudFoundry.FileService.WindowsService
{
    using Uhuru.CloudFoundry.FileService;
    using Uhuru.CloudFoundry.ServiceBase;
    using Uhuru.Configuration;
    using Uhuru.Configuration.Service;

    /// <summary>
    /// This is the Windows Service class that hosts an MS SQL Node.
    /// </summary>
    internal partial class FileServiceWindowsService : System.ServiceProcess.ServiceBase
    {
        /// <summary>
        /// The Uhurufs Service Node.
        /// </summary>
        private FileService.FileServiceNode node;

        /// <summary>
        /// Initializes a new instance of the <see cref="FileServiceWindowsService"/> class.
        /// </summary>
        public FileServiceWindowsService()
        {
            this.InitializeComponent();
        }

        /// <summary>
        /// Starts the MS SQL Node using the specified arguments.
        /// </summary>
        internal void Start()
        {
            ServiceElement serviceConfig = UhuruSection.GetSection().Service;

            this.node = new FileServiceNode();
            this.node.Start(serviceConfig);
        }

        /// <summary>
        /// When implemented in a derived class, executes when a Start command is sent to the service by the Service Control Manager (SCM) or when the operating system starts (for a service that starts automatically). Specifies actions to take when the service starts.
        /// </summary>
        /// <param name="args">Data passed by the start command.</param>
        protected override void OnStart(string[] args)
        {
            this.Start();
        }

        /// <summary>
        /// When implemented in a derived class, executes when a Stop command is sent to the service by the Service Control Manager (SCM). Specifies actions to take when a service stops running.
        /// </summary>
        protected override void OnStop()
        {
            this.node.Shutdown();
        }
    }
}
