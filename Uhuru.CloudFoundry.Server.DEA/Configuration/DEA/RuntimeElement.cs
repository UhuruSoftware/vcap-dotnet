using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;

namespace Uhuru.CloudFoundry.Server.DEA.Configuration.DEA
{
    public class RuntimeElement : ConfigurationElement
    {

        #region Constructors

        static RuntimeElement()
        {
            propertyName = new ConfigurationProperty(
                "name",
                typeof(string),
                null,
                ConfigurationPropertyOptions.IsRequired
            );

            propertyExecutable = new ConfigurationProperty(
                "executable",
                typeof(string),
                null,
                ConfigurationPropertyOptions.IsRequired
            );

            propertyVersion = new ConfigurationProperty(
                "version",
                typeof(string),
                null,
                ConfigurationPropertyOptions.IsRequired
            );

            propertyVersionFlag = new ConfigurationProperty(
                "versionFlag",
                typeof(string),
                "-v"
            );

            propertyAdditionalChecks = new ConfigurationProperty(
                "additionalChecks",
                typeof(string),
                null
            );

            propertyEnvironment = new ConfigurationProperty(
                "environment",
                typeof(EnvironmentCollection),
                new EnvironmentCollection()
            );

            propertyDebug = new ConfigurationProperty(
                "debug",
                typeof(DebugCollection),
                new DebugCollection()
            );

            properties = new ConfigurationPropertyCollection();

            properties.Add(propertyName);
            properties.Add(propertyExecutable);
            properties.Add(propertyVersion);
            properties.Add(propertyVersionFlag);
            properties.Add(propertyAdditionalChecks);
            properties.Add(propertyEnvironment);
            properties.Add(propertyDebug);
        }

        #endregion

        #region Static Fields

        private static ConfigurationProperty propertyName;
        private static ConfigurationProperty propertyExecutable;
        private static ConfigurationProperty propertyVersion;
        private static ConfigurationProperty propertyVersionFlag;
        private static ConfigurationProperty propertyAdditionalChecks;
        private static ConfigurationProperty propertyEnvironment;
        private static ConfigurationProperty propertyDebug;

        private static ConfigurationPropertyCollection properties;
        #endregion

        #region Properties


        [ConfigurationProperty("name", IsRequired = true, DefaultValue = null)]
        public string Name
        {
            get
            {
                return (string)base[propertyName];
            }
        }

        [ConfigurationProperty("executable", IsRequired = true, DefaultValue = null)]
        public string Executable
        {
            get
            {
                return (string)base[propertyExecutable];
            }
        }

        [ConfigurationProperty("version", IsRequired = true, DefaultValue = null)]
        public string Version
        {
            get
            {
                return (string)base[propertyVersion];
            }
        }

        [ConfigurationProperty("versionFlag", IsRequired = false, DefaultValue = "-v")]
        public string VersionFlag
        {
            get
            {
                return (string)base[propertyVersionFlag];
            }
        }

        [ConfigurationProperty("additionalChecks", IsRequired = false, DefaultValue = null)]
        public string AdditionalChecks
        {
            get
            {
                return (string)base[propertyAdditionalChecks];
            }
        }

        [ConfigurationProperty("environment", IsRequired = false, DefaultValue = null)]
        public EnvironmentCollection Environment
        {
            get
            {
                return (EnvironmentCollection)base[propertyEnvironment];
            }
        }

        [ConfigurationProperty("debug", IsRequired = false, DefaultValue = null)]
        public DebugCollection Debug
        {
            get
            {
                return (DebugCollection)base[propertyDebug];
            }
        }

        /// <summary>
        /// Override the Properties collection and return our custom one.
        /// </summary>
        protected override ConfigurationPropertyCollection Properties
        {
            get { return properties; }
        }

        #endregion
    }
}
