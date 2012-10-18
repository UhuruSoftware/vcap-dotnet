// -----------------------------------------------------------------------
// <copyright file="SupportedVersionElement.cs" company="Uhuru Software, Inc.">
// Copyright (c) 2011 Uhuru Software, Inc., All Rights Reserved
// </copyright>
// -----------------------------------------------------------------------

namespace Uhuru.Configuration.Service
{
    using System.Configuration;

    /// <summary>
    /// This is a configuration class that defines the supported service versions.
    /// </summary>
    public class SupportedVersionElement : ConfigurationElement
    {
        /// <summary>
        /// Name configuration property.
        /// </summary>
        private static ConfigurationProperty propertyName;
      
        /// <summary>
        /// Configuration properties collection.
        /// </summary>
        private static ConfigurationPropertyCollection properties;

        /// <summary>
        /// Initializes static members of the <see cref="SupportedVersionElement"/> class.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1810:InitializeReferenceTypeStaticFieldsInline", Justification = "Code is cleaner this way")]
        static SupportedVersionElement()
        {
            propertyName = new ConfigurationProperty(
                "name",
                typeof(string),
                null,
                ConfigurationPropertyOptions.IsRequired);

            properties = new ConfigurationPropertyCollection();

            properties.Add(propertyName);
        }
        
        /// <summary>
        /// Gets or sets the version.
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
