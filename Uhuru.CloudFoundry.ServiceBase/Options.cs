// -----------------------------------------------------------------------
// <copyright file="Options.cs" company="Uhuru Software">
// Copyright (c) 2011 Uhuru Software, Inc., All Rights Reserved
// </copyright>
// -----------------------------------------------------------------------

namespace Uhuru.CloudFoundry.ServiceBase
{
    /// <summary>
    /// This class contains configuration options for a Cloud Foundry system service.
    /// </summary>
    public class Options
    {
        private int index = 0;
        private int zInterval = 30000;

        /// <summary>
        /// Gets or sets the Node Id of the service.
        /// </summary>
        public string NodeId
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the network file system used for migrating provisioned services.
        /// </summary>
        public string MigrationNFS
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the NATS message bus uri.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1056:UriPropertiesShouldNotBeStrings")]
        public string Uri
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the node index of the service node.
        /// </summary>
        public int Index 
        { 
            get
            {
                return this.index;
            }

            set
            {
                this.index = value;
            }
        }

        /// <summary>
        /// Gets or sets the interval at which the varz and healthz are updated.
        /// </summary>
        public int ZInterval
        {
            get
            {
                return this.zInterval;
            }

            set
            {
                this.zInterval = value;
            }
        }

        /// <summary>
        /// Gets or sets the maximum database size for a provisioned service.
        /// </summary>
        public int MaxDBSize
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the maximum length for a running query.
        /// </summary>
        public int MaxLengthyQuery
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the maximum length for a running transaction.
        /// </summary>
        public int MaxLengthyTX
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the base directory for the service.
        /// </summary>
        public string BaseDir
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the maximum available storage for the service.
        /// </summary>
        public int AvailableStorage
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the local db in which to save the provisioned services.
        /// </summary>
        public string LocalDB 
        { 
            get; 
            set; 
        }

        /// <summary>
        /// Gets or sets the IP address of a well known server on your network; it
        /// is used to choose the right ip address (think of hosts that have multiple nics
        /// and IP addresses assigned to them) of the host running the DEA. Default
        /// value of null, should work in most cases.
        /// </summary>
        public string LocalRoute 
        { 
            get; 
            set; 
        }
    }
}
