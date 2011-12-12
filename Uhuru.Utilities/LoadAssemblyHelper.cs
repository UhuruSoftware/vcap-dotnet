// -----------------------------------------------------------------------
// <copyright file="LoadAssemblyHelper.cs" company="Uhuru Software, Inc.">
// Copyright (c) 2011 Uhuru Software, Inc., All Rights Reserved
// </copyright>
// -----------------------------------------------------------------------

namespace Uhuru.Utilities
{
    using System;
    using System.Reflection;

    /// <summary>
    /// This class is injected at runtime in a new app domain, and used to get the .Net version of an assembly.
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1812:AvoidUninstantiatedInternalClasses", Justification = "This class is loaded through reflection at runtime.")]
    public class LoadAssemblyHelper : MarshalByRefObject
    {
        /// <summary>
        /// Gets the dot net version of a sepcified assembly.
        /// </summary>
        /// <param name="assemblyPath">The assembly path.</param>
        /// <returns>A string containing the .Net version of the assembly.</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "This method is called through reflection.")]
        public string GetDotNetVersion(string assemblyPath)
        {
            Assembly a = Assembly.ReflectionOnlyLoadFrom(assemblyPath);
            return a.ImageRuntimeVersion.Split('.')[0].Replace("v", string.Empty);
        }
    }
}
