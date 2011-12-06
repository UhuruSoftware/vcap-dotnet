﻿// -----------------------------------------------------------------------
// <copyright file="ApplicationVariable.cs" company="Uhuru Software">
// Copyright (c) 2011 Uhuru Software, Inc., All Rights Reserved
// </copyright>
// -----------------------------------------------------------------------

namespace Uhuru.CloudFoundry.Server.DEA.PluginBase
{
    using System;
    
    /// <summary>
    /// application variable basic data
    /// </summary>
    public class ApplicationVariable : MarshalByRefObject
    {
        /// <summary>
        /// the name of the variable
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// the value of the variable
        /// </summary>
        public string Value { get; set; }
    }

}
