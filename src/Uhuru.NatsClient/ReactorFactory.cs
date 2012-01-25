// -----------------------------------------------------------------------
// <copyright file="ReactorFactory.cs" company="Uhuru Software, Inc.">
// Copyright (c) 2011 Uhuru Software, Inc., All Rights Reserved
// </copyright>
// -----------------------------------------------------------------------

namespace Uhuru.NatsClient
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    /// <summary>
    /// Factory class for reactor
    /// </summary>
    public static class ReactorFactory
    {
        /// <summary>
        /// Factory method for reactor
        /// </summary>
        /// <param name="type">The type of the object.</param>
        /// <returns>An instance of the object depending on the provisioned type</returns>
        public static IReactor GetReactor(Type type)
        {
            return (IReactor)Activator.CreateInstance(type);
        }
    }
}
