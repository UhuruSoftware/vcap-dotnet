// -----------------------------------------------------------------------
// <copyright file="Resource.cs" company="Uhuru Software">
// Copyright (c) 2011 Uhuru Software, Inc., All Rights Reserved
// </copyright>
// -----------------------------------------------------------------------

namespace Uhuru.NatsClient
{
    using System;
    using System.Globalization;
    using System.Text.RegularExpressions;
    
    /// <summary>
    /// This is a helper class for getting parsing regex resources.
    /// </summary>
    public class Resource
    {
        internal readonly string PING_REQUEST = string.Format(CultureInfo.InvariantCulture, "PING{0}", "\r\n");
        internal readonly string PONG_RESPONSE = string.Format(CultureInfo.InvariantCulture, "PONG{0}", "\r\n");
        internal const string CR_LF = "\r\n";
        internal const string EMPTY_MSG = string.Empty;

        private static readonly object resourceInstanceLock = new object();
        private static Resource instance;
        
        // private ResourceManager resourceManager = new ResourceManager("Uhuru.NatsClient.Resources.ClientConnection",);
        private Regex msg;
        private Regex ok;
        private Regex err;
        private Regex ping;
        private Regex pong;
        private Regex info;
        private Regex unknown;
        
        private Resource()
        {
            this.msg = new Regex(Uhuru.NatsClient.Resources.ClientConnection.MSG);
            this.ok = new Regex(Uhuru.NatsClient.Resources.ClientConnection.OK);
            this.err = new Regex(Uhuru.NatsClient.Resources.ClientConnection.ERR);
            this.ping = new Regex(Uhuru.NatsClient.Resources.ClientConnection.PING);
            this.pong = new Regex(Uhuru.NatsClient.Resources.ClientConnection.PONG);
            this.info = new Regex(Uhuru.NatsClient.Resources.ClientConnection.INFO);
            this.unknown = new Regex(Uhuru.NatsClient.Resources.ClientConnection.UNKNOWN);
            // languageRM = ResourceManager.CreateFileBasedResourceManager("Language.resx", "Resouces", null);
        }

        internal Regex MSG
        {
            get
            {
                return this.msg;
            }
        }

        internal Regex OK
        {
            get
            {
                return this.ok;
            }
        }

        internal Regex ERR 
        {
            get
            {
                return this.err;
            }
        }

        internal Regex PING
        {
            get
            {
                return this.ping;
            }
        }

        internal Regex PONG
        {
            get
            {
                return this.pong;
            }
        }

        internal Regex INFO
        {
            get
            {
                return this.info;
            }
        }

        internal Regex UNKNOWN
        {
            get
            {
                return this.unknown;
            }
        }

        /// <summary>
        /// Gets the singleton instance of the Resource class.
        /// </summary>
        public static Resource Instance
        {
            get
            {
                if (instance == null)
                {
                    lock (resourceInstanceLock)
                    {
                        if (instance == null)
                        {
                            instance = new Resource();
                        }
                    }
                }

                return instance;
            }
        }
    }
}
