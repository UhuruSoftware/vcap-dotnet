// -----------------------------------------------------------------------
// <copyright file="EnvironmentElement.cs" company="Uhuru Software">
// Copyright (c) 2011 Uhuru Software, Inc., All Rights Reserved
// </copyright>
// -----------------------------------------------------------------------

namespace Uhuru.Configuration.DEA
{
    using System.Configuration;
    
    /// <summary>
    /// This configuration class contains environment variable settings for a runtime.
    /// </summary>
    public class EnvironmentElement : ConfigurationElement
    {
        /// <summary>
        /// Name property collection.
        /// </summary>
        private static ConfigurationProperty propertyName;

        /// <summary>
        /// Value property collection.
        /// </summary>
        private static ConfigurationProperty propertyValue;

        /// <summary>
        /// Configuration properties collection.
        /// </summary>
        private static ConfigurationPropertyCollection properties;

        /// <summary>
        /// Initializes static members of the <see cref="EnvironmentElement"/> class.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1810:InitializeReferenceTypeStaticFieldsInline", Justification = "Code is cleaner this way.")]
        static EnvironmentElement()
        {
            propertyName = new ConfigurationProperty(
                "name",
                typeof(string),
                null,
                ConfigurationPropertyOptions.IsRequired);

            propertyValue = new ConfigurationProperty(
                "value",
                typeof(string),
                null,
                ConfigurationPropertyOptions.IsRequired);

            properties = new ConfigurationPropertyCollection();

            properties.Add(propertyName);
            properties.Add(propertyValue);
        }

        /// <summary>
        /// Gets or sets the name of a Environment variable.
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
        /// Gets or sets the value of a Environment variable.
        /// </summary>
        [ConfigurationProperty("value", IsRequired = true, DefaultValue = null)]
        public string Value
        {
            get
            {
                return (string)base[propertyValue];
            }

            set
            {
                base[propertyValue] = value;
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
