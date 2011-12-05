using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Security.Permissions;
using System.Reflection;
using System.Globalization;
using System.IO;
using System.Diagnostics;

namespace Uhuru.Utilities
{
    public enum DotNetVersion
    {
        Two,
        Four
    }

    public static class NetFrameworkVersion
    {
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
