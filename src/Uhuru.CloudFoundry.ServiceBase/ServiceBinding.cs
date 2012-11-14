// -----------------------------------------------------------------------
// <copyright file="ServiceBinding.cs" company="Uhuru Software, Inc.">
// Copyright (c) 2011 Uhuru Software, Inc., All Rights Reserved
// </copyright>
// -----------------------------------------------------------------------

namespace Uhuru.CloudFoundry.ServiceBase
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    /// <summary>
    /// Service binding information.
    /// </summary>
    [Serializable]
    public class ServiceBinding
    {
        /// <summary>
        /// Gets or sets the username for the binding.
        /// </summary>
        public string User
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the password for the binding.
        /// </summary>
        public string Password
        {
            get;
            set;
        }
    }
}
