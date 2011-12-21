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

        /// <summary>
        /// Detects the platform.
        /// </summary>
        /// <param name="assemblyPath">The assembly path.</param>
        /// <returns>CPU target</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "This method is called through reflection.")]
        public CpuTarget DetectPlatform(string assemblyPath)
        {
            Assembly a = Assembly.ReflectionOnlyLoadFrom(assemblyPath);
            PortableExecutableKinds kind;
            ImageFileMachine machine;
            a.ManifestModule.GetPEKind(out kind, out machine);

            switch (kind)
            {
                case PortableExecutableKinds.ILOnly:
                    return CpuTarget.AnyCpu;
                case PortableExecutableKinds.PE32Plus:
                    return CpuTarget.X64;
                case PortableExecutableKinds.PE32Plus | PortableExecutableKinds.ILOnly:
                    return CpuTarget.X64;
                case PortableExecutableKinds.Required32Bit:
                    return CpuTarget.X86;
                case PortableExecutableKinds.Required32Bit | PortableExecutableKinds.ILOnly:
                    return CpuTarget.X86;
                default:
                    return CpuTarget.Unknown;
            }
        }
    }
}
