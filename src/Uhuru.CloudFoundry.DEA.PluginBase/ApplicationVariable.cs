// -----------------------------------------------------------------------
// <copyright file="ApplicationVariable.cs" company="Uhuru Software, Inc.">
// Copyright (c) 2011 Uhuru Software, Inc., All Rights Reserved
// </copyright>
// -----------------------------------------------------------------------

namespace Uhuru.CloudFoundry.DEA.PluginBase
{
    using System;
    
    /// <summary>
    /// application variable basic data
    /// </summary>
    public class ApplicationVariable : MarshalByRefObject
    {
        /// <summary>
        /// Gets or sets the name of the variable
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the value of the variable
        /// </summary>
        public string Value { get; set; }
    }
}
