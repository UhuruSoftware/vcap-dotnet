// -----------------------------------------------------------------------
// <copyright file="Resource.cs" company="Uhuru Software, Inc.">
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
        /// <summary>
        /// The format of the ping request
        /// </summary>
        internal readonly string PINGREQUEST = string.Format(CultureInfo.InvariantCulture, "PING{0}", "\r\n");
        
        /// <summary>
        /// The format of the ping response
        /// </summary>
        internal readonly string PONGRESPONSE = string.Format(CultureInfo.InvariantCulture, "PONG{0}", "\r\n");
        
        /// <summary>
        /// The format for a new line
        /// </summary>
        internal const string CRLF = "\r\n";
        
        /// <summary>
        /// The format for an empty message
        /// </summary>
        internal const string EMPTYMSG = "";

        /// <summary>
        /// Locker for the instance 
        /// </summary>
        private static readonly object resourceInstanceLock = new object();
        
        /// <summary>
        /// The instance of the resource. Used for building a singleton of the Resource object
        /// </summary>
        private static Resource instance;

        /// <summary>
        /// Regex for a received message
        /// </summary>
        private Regex msg;
        
        /// <summary>
        /// Regex for an OK message
        /// </summary>
        private Regex ok;
        
        /// <summary>
        /// Regex for an error message
        /// </summary>
        private Regex err;
        
        /// <summary>
        /// Regex for a ping message
        /// </summary>
        private Regex ping;
        
        /// <summary>
        /// Regex for a pong message
        /// </summary>
        private Regex pong;
        
        /// <summary>
        /// Regex for an info message
        /// </summary>
        private Regex info;
        
        /// <summary>
        /// Regex for an unknown message
        /// </summary>
        private Regex unknown;

        /// <summary>
        /// Prevents a default instance of the <see cref="Resource"/> class from being created.
        /// </summary>
        private Resource()
        {
            this.msg = new Regex(Uhuru.NatsClient.Resources.ClientConnection.MSG);
            this.ok = new Regex(Uhuru.NatsClient.Resources.ClientConnection.OK);
            this.err = new Regex(Uhuru.NatsClient.Resources.ClientConnection.ERR);
            this.ping = new Regex(Uhuru.NatsClient.Resources.ClientConnection.PING);
            this.pong = new Regex(Uhuru.NatsClient.Resources.ClientConnection.PONG);
            this.info = new Regex(Uhuru.NatsClient.Resources.ClientConnection.INFO);
            this.unknown = new Regex(Uhuru.NatsClient.Resources.ClientConnection.UNKNOWN);
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

        /// <summary>
        /// Gets the MSG.
        /// </summary>
        internal Regex MSG
        {
            get
            {
                return this.msg;
            }
        }

        /// <summary>
        /// Gets the OK.
        /// </summary>
        internal Regex OK
        {
            get
            {
                return this.ok;
            }
        }

        /// <summary>
        /// Gets the ERR.
        /// </summary>
        internal Regex ERR
        {
            get
            {
                return this.err;
            }
        }

        /// <summary>
        /// Gets the PING.
        /// </summary>
        internal Regex PING
        {
            get
            {
                return this.ping;
            }
        }

        /// <summary>
        /// Gets the PONG.
        /// </summary>
        internal Regex PONG
        {
            get
            {
                return this.pong;
            }
        }

        /// <summary>
        /// Gets the INFO.
        /// </summary>
        internal Regex INFO
        {
            get
            {
                return this.info;
            }
        }

        /// <summary>
        /// Gets the UNKNOWN.
        /// </summary>
        internal Regex UNKNOWN
        {
            get
            {
                return this.unknown;
            }
        }
    }
}
