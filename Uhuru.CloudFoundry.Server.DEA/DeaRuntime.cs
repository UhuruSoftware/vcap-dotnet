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
        public string Executable;
        public string Version;
        public string VersionFlag;
        public string AdditionalChecks;
        public Dictionary<string, List<string>> DebugEnv = new Dictionary<string,List<string>>();
        public Dictionary<string, string> Environment = new Dictionary<string,string>();
        public bool Enabled;
    }

}
