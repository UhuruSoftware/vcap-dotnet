// -----------------------------------------------------------------------
// <copyright file="PlatformTarget.cs" company="Uhuru Software, Inc.">
// Copyright (c) 2011 Uhuru Software, Inc., All Rights Reserved
// </copyright>
// -----------------------------------------------------------------------

namespace Uhuru.Utilities
{
    using System;
    using System.IO;
    using System.Reflection;

    /// <summary>
    /// CPU Platform target
    /// </summary>
    public enum CpuTarget
    {
        /// <summary>
        /// Any Cpu
        /// </summary>
        AnyCpu,

        /// <summary>
        /// 32 bit platform
        /// </summary>
        X86,

        /// <summary>
        /// 64 bit platform
        /// </summary>
        X64,

        /// <summary>
        /// cannot be determined
        /// </summary>
        Unknown
    }

    /// <summary>
    /// Class used for detecting platform target of an assembly
    /// </summary>
    public static class PlatformTarget
    {
        /// <summary>
        /// Detects the platform.
        /// </summary>
        /// <param name="assemblyPath">The assembly path.</param>
        /// <returns>CPU target</returns>
        public static CpuTarget DetectPlatform(string assemblyPath)
        {
            if (string.IsNullOrEmpty(assemblyPath))
            {
                throw new ArgumentException("Argument null or empty", "assemblyPath");
            }

            AppDomainSetup setup = AppDomain.CurrentDomain.SetupInformation;
            setup.ApplicationBase = Path.GetDirectoryName(assemblyPath);
            string domainName = Guid.NewGuid().ToString();
            AppDomain domain = AppDomain.CreateDomain(domainName, null, setup);

            try
            {
                LoadAssemblyHelper obj = (LoadAssemblyHelper)domain.CreateInstanceFromAndUnwrap(Assembly.GetExecutingAssembly().Location, "Uhuru.Utilities.LoadAssemblyHelper");

                CpuTarget target = obj.DetectPlatform(assemblyPath);

                return target;
            }
            catch (System.BadImageFormatException)
            {
                return CpuTarget.Unknown;
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                AppDomain.Unload(domain);
            }
        }
    }
}
