// -----------------------------------------------------------------------
// <copyright file="SupportedVersionsCollection.cs" company="Uhuru Software, Inc.">
// Copyright (c) 2011 Uhuru Software, Inc., All Rights Reserved
// </copyright>
// -----------------------------------------------------------------------

namespace Uhuru.Configuration.Service
{
    using System.Configuration;

    /// <summary>
    /// TODO: Update summary.
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1010:CollectionsShouldImplementGenericInterface", Justification = "This is a configuration class, no need to implement ICollection"),
    ConfigurationCollection(typeof(SupportedVersionElement), CollectionType = ConfigurationElementCollectionType.BasicMap)]
    public class SupportedVersionsCollection : ConfigurationElementCollection
    {
        /// <summary>
        /// Configuration properties collection.
        /// </summary>
        private static ConfigurationPropertyCollection properties;

        /// <summary>
        /// Default Version configuration property.
        /// </summary>
        private static ConfigurationProperty propertyDefaultVersion;

        /// <summary>
        /// Supported Version Collection configuration property
        /// </summary>
        private static ConfigurationProperty propertySupportedVersions;

        /// <summary>
        /// Initializes static members of the <see cref="SupportedVersionsCollection"/> class.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1810:InitializeReferenceTypeStaticFieldsInline", Justification = "Code is cleaner this way")]
        static SupportedVersionsCollection()
        {
            properties = new ConfigurationPropertyCollection();
            propertyDefaultVersion = new ConfigurationProperty(
                "defaultVersion", 
                typeof(string),
                null, 
                ConfigurationPropertyOptions.IsRequired);

            propertySupportedVersions = new ConfigurationProperty(
                "supportedVersions",
                typeof(SupportedVersionsCollection),
                new SupportedVersionsCollection());

            properties.Add(propertyDefaultVersion);
            properties.Add(propertySupportedVersions);
        }

        /// <summary>
        /// Initializes a new instance of the SupportedVersionsCollection class.
        /// </summary>
        public SupportedVersionsCollection()
        {
        }

        /// <summary>
        /// Gets or sets the default version
        /// </summary>
        [ConfigurationProperty("defaultVersion", IsRequired = true, DefaultValue = null)]
        public string DefaultVersion
        {
            get
            {
                return (string)base[propertyDefaultVersion];
            }

            set
            {
                base[propertyDefaultVersion] = value;
            }
        }

        /// <summary>
        /// Defines the collection type (BasicMap) for SupportedVersionsCollection
        /// </summary>
        public override ConfigurationElementCollectionType CollectionType
        {
            get { return ConfigurationElementCollectionType.BasicMap; }
        }

        /// <summary>
        /// Gets the configuration properties available for a SupportedVersionsCollection
        /// </summary>
        /// <returns>The <see cref="T:System.Configuration.ConfigurationPropertyCollection"/> of properties for the element.</returns>
        protected override ConfigurationPropertyCollection Properties
        {
            get { return properties; }
        }

        /// <summary>
        /// Gets the element name for a SupportedVersionsCollection
        /// </summary>
        protected override string ElementName
        {
            get { return "supportedVersion"; }
        }

        /// <summary>
        /// Gets a runtime configuration by index.
        /// </summary>
        /// <param name="index">Zero-based index of a runtime configuration.</param>
        /// <returns>The SupportedVersionElement at the specified index.</returns>
        public SupportedVersionElement this[int index]
        {
            get 
            {
                return (SupportedVersionElement)this.BaseGet(index); 
            }

            set
            {
                if (this.BaseGet(index) != null)
                {
                    this.BaseRemoveAt(index);
                }

                this.BaseAdd(index, value);
            }
        }

        /// <summary>
        /// Gets a runtime configuration by its name.
        /// </summary>
        /// <param name="name">String specifying the runtime name.</param>
        /// <returns>The SupportedVersionElement with the specified name.</returns>
        public new SupportedVersionElement this[string name]
        {
            get { return (SupportedVersionElement)this.BaseGet(name); }
        }

        /// <summary>
        /// This method gets an element key name for a SupportedVersionElement.
        /// </summary>
        /// <param name="element">The SupportedVersionElement for which to get the key.</param>
        /// <returns>A string that is the name of the Version configuration.</returns>
        protected override object GetElementKey(ConfigurationElement element)
        {
            return (element as SupportedVersionElement).Name;
        }

        /// <summary>
        /// This method creates a new SupportedVersionElement.
        /// </summary>
        /// <returns>A new SupportedVersionElement.</returns>
        protected override ConfigurationElement CreateNewElement()
        {
            return new SupportedVersionElement();
        }
    }
}
