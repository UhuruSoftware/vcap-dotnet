// -----------------------------------------------------------------------
// <copyright file="EnvironmentCollection.cs" company="Uhuru Software, Inc.">
// Copyright (c) 2011 Uhuru Software, Inc., All Rights Reserved
// </copyright>
// -----------------------------------------------------------------------

namespace Uhuru.Configuration.DEA
{
    using System.Configuration;
    
    /// <summary>
    /// This class is a collection of EnvironmentElement items.
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1010:CollectionsShouldImplementGenericInterface", Justification = "This is a configuration class, no need to implement ICollection"), 
    ConfigurationCollection(typeof(EnvironmentElement), CollectionType = ConfigurationElementCollectionType.BasicMap)]
    public class EnvironmentCollection : ConfigurationElementCollection
    {
        /// <summary>
        /// Configuration properties collection.
        /// </summary>
        private static ConfigurationPropertyCollection properties;

        /// <summary>
        /// Initializes static members of the <see cref="EnvironmentCollection"/> class.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1810:InitializeReferenceTypeStaticFieldsInline", Justification = "Code is cleaner this way.")]
        static EnvironmentCollection()
        {
            properties = new ConfigurationPropertyCollection();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="EnvironmentCollection"/> class.
        /// </summary>
        public EnvironmentCollection()
        {
        }

        /// <summary>
        /// Defines the collection type (BasicMap) for the current object.
        /// </summary>
        public override ConfigurationElementCollectionType CollectionType
        {
            get { return ConfigurationElementCollectionType.BasicMap; }
        }

        /// <summary>
        /// Gets the element name for the current object.
        /// </summary>
        protected override string ElementName
        {
            get { return "env"; }
        }

        /// <summary>
        /// Defines the configuration properties available for the current object.
        /// </summary>
        protected override ConfigurationPropertyCollection Properties
        {
            get { return properties; }
        }
        
        /// <summary>
        /// Gets a environment variable by index.
        /// </summary>
        /// <param name="index">The zero-based index of an environment variable.</param>
        /// <returns>The EnvironmentElement at the specified index.</returns>
        public EnvironmentElement this[int index]
        {
            get 
            { 
                return (EnvironmentElement)this.BaseGet(index); 
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
        /// Gets an environment variable by its name.
        /// </summary>
        /// <param name="name">String specifying the environment variable name.</param>
        /// <returns>The EnvironmentElement with the specified name.</returns>
        public new EnvironmentElement this[string name]
        {
            get { return (EnvironmentElement)this.BaseGet(name); }
        }

        /// <summary>
        /// This method creates a new EnvironmentElement.
        /// </summary>
        /// <returns>A new EnvironmentElement.</returns>
        protected override ConfigurationElement CreateNewElement()
        {
            return new EnvironmentElement();
        }

        /// <summary>
        /// This method gets an element key name for a EnvironmentElement.
        /// </summary>
        /// <param name="element">The EnvironmentElement for which to get the key.</param>
        /// <returns>A string that is the name of the environment variable.</returns>
        protected override object GetElementKey(ConfigurationElement element)
        {
            return (element as EnvironmentElement).Name;
        }
    }
}
