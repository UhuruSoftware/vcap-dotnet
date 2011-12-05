// -----------------------------------------------------------------------
// <copyright file="NetFrameworkVersion.cs" company="Uhuru Software">
// </copyright>
// -----------------------------------------------------------------------

namespace Uhuru.Utilities
{
    using System;
    using System.Globalization;
    using System.IO;
    using System.Reflection;
    using System.Security.Permissions;
    
    /// <summary>
    /// a DotNet version
    /// </summary>
    public enum DotNetVersion
    {
        /// <summary>
        /// version 2.0
        /// </summary>
        Two,

        /// <summary>
        /// version 4.0
        /// </summary>
        Four
    }

    /// <summary>
    /// class used for dot net framework version detection
    /// </summary>
    public static class NetFrameworkVersion
    {
        /// <summary>
        /// returns the dot net framework version of an assembly
        /// </summary>
        /// <param name="assemblyPath">the path to the assembly</param>
        /// <returns>the dot net framewrok version</returns>
        [PermissionSet(SecurityAction.Demand, Name = "FullTrust")]
        public static DotNetVersion GetVersion(string assemblyPath)
        {
            if (string.IsNullOrEmpty(assemblyPath))
            {
                throw new ArgumentException("Argument null or empty", "assemblyPath");
            }
            try
            {
                string fileName = assemblyPath.Normalize();
                
                if (!(System.IO.File.Exists(fileName))) 
                { 
                    return DotNetVersion.Two;
                }

                //TODO: florind: find a safer way to do this, without loading the assembly in RAM
                AppDomainSetup setup = AppDomain.CurrentDomain.SetupInformation;
                setup.ApplicationBase = Path.GetDirectoryName(assemblyPath);
                string domainName = Guid.NewGuid().ToString();
                AppDomain domain = AppDomain.CreateDomain(domainName, null, setup);

                LoadAssembly obj = (LoadAssembly)domain.CreateInstanceFromAndUnwrap(Assembly.GetExecutingAssembly().Location, "CloudFoundry.Net.IIS.Utilities.LoadAssembly");

                string version = obj.GetDotNetVersion(assemblyPath); //a.ImageRuntimeVersion.Split('.')[0].Replace("v", "");

                AppDomain.Unload(domain);
                
                if (Convert.ToInt32(version, CultureInfo.InvariantCulture) < 4)
                {
                    return DotNetVersion.Two;
                }
                else
                {
                    return DotNetVersion.Four;
                }
            }
            catch (System.BadImageFormatException) 
            {
                return DotNetVersion.Two;
            }
            catch (Exception) 
            {
                throw;
            }
        }
    }

    class LoadAssembly : MarshalByRefObject
    {
        public string GetDotNetVersion(string assemblyPath)
        {
            Assembly a = Assembly.ReflectionOnlyLoadFrom(assemblyPath);
            return a.ImageRuntimeVersion.Split('.')[0].Replace("v", "");
        }
    }
}
