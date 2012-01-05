// -----------------------------------------------------------------------
// <copyright file="JsonNameAttribute.cs" company="Uhuru Software, Inc.">
// Copyright (c) 2011 Uhuru Software, Inc., All Rights Reserved
// </copyright>
// -----------------------------------------------------------------------

namespace Uhuru.Utilities.Json
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    /// <summary>
    /// This is an attribute that is used to decorate fields/properties/enums with JSON names.
    /// The JSON name will be used instead of the member's name when serializing.
    /// This is used in conjunction <see cref="JsonConvertibleObject"/>.
    /// </summary>
    [AttributeUsage(AttributeTargets.Enum | AttributeTargets.Property | AttributeTargets.Field)]
    public sealed class JsonNameAttribute : Attribute
    {
        /// <summary>
        /// The JSON name of the member.
        /// </summary>
        private string name;

        /// <summary>
        /// Initializes a new instance of the <see cref="JsonNameAttribute"/> class.
        /// </summary>
        /// <param name="name">The JSON name of the member.</param>
        public JsonNameAttribute(string name)
        {
            this.name = name;
        }

        /// <summary>
        /// Gets the Name of the member.
        /// </summary>
        public string Name
        {
            get
            {
                return this.name;
            }
        }
    }
}
