// -----------------------------------------------------------------------
// <copyright file="RuntimeElement.cs" company="Uhuru Software, Inc.">
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
        /// <summary>
        /// Name configuration property.
        /// </summary>
        private static ConfigurationProperty propertyName;
      
        /// <summary>
        /// Executable configuration property. 
        /// </summary>
        private static ConfigurationProperty propertyExecutable;
      
        /// <summary>
        /// Version configuration property.
        /// </summary>
        private static ConfigurationProperty propertyVersion;
      
        /// <summary>
        /// Version argument configuration property.
        /// </summary>
        private static ConfigurationProperty propertyVersionArgument;
        
        /// <summary>
        /// Additional checks configuration property.
        /// </summary>
        private static ConfigurationProperty propertyAdditionalChecks;
       
        /// <summary>
        /// Environment configuration property.
        /// </summary>
        private static ConfigurationProperty propertyEnvironment;
       
        /// <summary>
        /// Debug configuration property.
        /// </summary>
        private static ConfigurationProperty propertyDebug;
       
        /// <summary>
        /// Configuration properties collection.
        /// </summary>
        private static ConfigurationPropertyCollection properties;

        /// <summary>
        /// Initializes static members of the <see cref="RuntimeElement"/> class.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1810:InitializeReferenceTypeStaticFieldsInline", Justification = "Code is cleaner this way")]
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

            propertyVersionArgument = new ConfigurationProperty(
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
            properties.Add(propertyVersionArgument);
            properties.Add(propertyAdditionalChecks);
            properties.Add(propertyEnvironment);
            properties.Add(propertyDebug);
        }
        
        /// <summary>
        /// Gets or sets the name of the runtime.
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
        /// Gets or sets the executable for the runtime, used to get the version.
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
        /// Gets or sets the expected runtime version.
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
        /// Gets or sets the flag that is passed to the runtime executable, to get the version.
        /// </summary>
        [ConfigurationProperty("versionFlag", IsRequired = false, DefaultValue = "-v")]
        public string VersionArgument
        {
            get
            {
                return (string)base[propertyVersionArgument];
            }

            set
            {
                base[propertyVersionArgument] = value;
            }
        }

        /// <summary>
        /// Gets or sets any additional checks to be done for the runtime.
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
    }
}
