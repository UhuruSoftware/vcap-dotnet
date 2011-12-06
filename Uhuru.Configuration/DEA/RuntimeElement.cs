// -----------------------------------------------------------------------
// <copyright file="RuntimeElement.cs" company="Uhuru Software">
// Copyright (c) 2011 Uhuru Software, Inc., All Rights Reserved
// </copyright>
// -----------------------------------------------------------------------

namespace Uhuru.Configuration.DEA
{
    using System.Configuration;
    
    /// <summary>
    /// This is a configuration class that defines the possible runtimes.
    /// </summary>
    public class RuntimeElement : ConfigurationElement
    {
        #region Constructors

        static RuntimeElement()
        {
            propertyName = new ConfigurationProperty(
                "name",
                typeof(string),
                null,
                ConfigurationPropertyOptions.IsRequired);

            propertyExecutable = new ConfigurationProperty(
                "executable",
                typeof(string),
                null,
                ConfigurationPropertyOptions.IsRequired);

            propertyVersion = new ConfigurationProperty(
                "version",
                typeof(string),
                null,
                ConfigurationPropertyOptions.IsRequired);

            propertyVersionFlag = new ConfigurationProperty(
                "versionFlag",
                typeof(string),
                "-v");

            propertyAdditionalChecks = new ConfigurationProperty(
                "additionalChecks",
                typeof(string),
                null);

            propertyEnvironment = new ConfigurationProperty(
                "environment",
                typeof(EnvironmentCollection),
                new EnvironmentCollection());

            propertyDebug = new ConfigurationProperty(
                "debug",
                typeof(DebugCollection),
                new DebugCollection());

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

        /// <summary>
        /// Gets the name of the runtime.
        /// </summary>
        [ConfigurationProperty("name", IsRequired = true, DefaultValue = null)]
        public string Name
        {
            get
            {
                return (string)base[propertyName];
            }

            set
            {
                base[propertyName] = value;
            }
        }

        /// <summary>
        /// Gets the executable for the runtime, used to get the version.
        /// </summary>
        [ConfigurationProperty("executable", IsRequired = true, DefaultValue = null)]
        public string Executable
        {
            get
            {
                return (string)base[propertyExecutable];
            }

            set
            {
                base[propertyExecutable] = value;
            }
        }

        /// <summary>
        /// Gets the expected runtime version.
        /// </summary>
        [ConfigurationProperty("version", IsRequired = true, DefaultValue = null)]
        public string Version
        {
            get
            {
                return (string)base[propertyVersion];
            }

            set
            {
                base[propertyVersion] = value;
            }
        }

        /// <summary>
        /// Gets the flag that is passed to the runtime executable, to get the version.
        /// </summary>
        [ConfigurationProperty("versionFlag", IsRequired = false, DefaultValue = "-v")]
        public string VersionFlag
        {
            get
            {
                return (string)base[propertyVersionFlag];
            }

            set
            {
                base[propertyVersionFlag] = value;
            }
        }

        /// <summary>
        /// Gets any additional checks to be done for the runtime.
        /// </summary>
        [ConfigurationProperty("additionalChecks", IsRequired = false, DefaultValue = null)]
        public string AdditionalChecks
        {
            get
            {
                return (string)base[propertyAdditionalChecks];
            }

            set
            {
                base[propertyAdditionalChecks] = value;
            }
        }

        /// <summary>
        /// Gets a collection of environment variables for this runtime.
        /// </summary>
        [ConfigurationProperty("environment", IsRequired = false, DefaultValue = null)]
        public EnvironmentCollection Environment
        {
            get
            {
                return (EnvironmentCollection)base[propertyEnvironment];
            }
        }

        /// <summary>
        /// Gets a collection of debug configurations for this runtime.
        /// </summary>
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

        #region Overrides

        /// <summary>
        /// Gets a value indicating whether the <see cref="T:System.Configuration.ConfigurationElement"/> object is read-only.
        /// </summary>
        /// <returns>
        /// true if the <see cref="T:System.Configuration.ConfigurationElement"/> object is read-only; otherwise, false.
        /// </returns>
        public override bool IsReadOnly()
        {
            return false;
        }

        #endregion
    }
}
