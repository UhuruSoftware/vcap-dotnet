// -----------------------------------------------------------------------
// <copyright file="Buildpack.cs" company="Uhuru Software, Inc.">
// Copyright (c) 2013 Uhuru Software, Inc., All Rights Reserved
// </copyright>
// -----------------------------------------------------------------------

namespace Uhuru.CloudFoundry.DEA
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    class Buildpack
    {
        public string Name
        {
            get 
            {
                return detectOutput.Trim();
            }
        }

        private string detectOutput;
        private string path;

        public Buildpack(string path, string appDir, string cacheDir)
        {
            this.path = path;
        }

        public bool Detect()
        {
            detectOutput = "dotNet";
            return true;
        }

        public void Compile() { }

        public string ReleaseInfo() 
        {
            return string.Empty;
        }
    }
}
