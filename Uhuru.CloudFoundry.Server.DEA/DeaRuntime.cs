// -----------------------------------------------------------------------
// <copyright file="DeaRuntime.cs" company="Uhuru Software">
// Copyright (c) 2011 Uhuru Software, Inc., All Rights Reserved
// </copyright>
// -----------------------------------------------------------------------

namespace Uhuru.CloudFoundry.DEA
{
    using System.Collections.Generic;
    
    public class DeaRuntime
    {
        public string Executable { get; set; }
        public string Version { get; set; }
        public string VersionFlag { get; set; }
        public string AdditionalChecks { get; set; }
        public Dictionary<string, List<string>> DebugEnv { get; set; }
        public Dictionary<string, string> Environment { get; set; }
        public bool Enabled { get; set; }

        /// <summary>
        /// Initializes a new instance of the DeaRuntime class.
        /// </summary>
        public DeaRuntime()
        { 
            this.DebugEnv = new Dictionary<string,List<string>>();
            this.Environment = new Dictionary<string,string>();
        }
    }
}
