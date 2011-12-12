// -----------------------------------------------------------------------
// <copyright file="RuntimeCollection.cs" company="Uhuru Software">
// Copyright (c) 2011 Uhuru Software, Inc., All Rights Reserved
// </copyright>
// -----------------------------------------------------------------------

namespace Uhuru.Configuration.DEA
{
    using System.Configuration;
    
    /// <summary>
    /// This class is a collection of RuntimeElement.
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1010:CollectionsShouldImplementGenericInterface", Justification = "This is a configuration class, no need to implement ICollection"), 
    ConfigurationCollection(typeof(RuntimeElement), CollectionType = ConfigurationElementCollectionType.BasicMap)]
    public class RuntimeCollection : ConfigurationElementCollection
    {
        /// <summary>
        /// Configuration properties collection.
        /// </summary>
        private static ConfigurationPropertyCollection properties;

        /// <summary>
        /// Initializes static members of the <see cref="RuntimeCollection"/> class.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1810:InitializeReferenceTypeStaticFieldsInline", Justification = "Code is cleaner this way")]
        static RuntimeCollection()
        {
            properties = new ConfigurationPropertyCollection();
        }

        /// <summary>
        /// Initializes a new instance of the RuntimeCollection class.
        /// </summary>
        public RuntimeCollection()
        {
        }

        /// <summary>
        /// Defines the collection type (BasicMap) for RuntimeCollection
        /// </summary>
        public override ConfigurationElementCollectionType CollectionType
        {
            get { return ConfigurationElementCollectionType.BasicMap; }
        }

        /// <summary>
        /// Gets the configuration properties available for a RuntimeCollection
        /// </summary>
        /// <returns>The <see cref="T:System.Configuration.ConfigurationPropertyCollection"/> of properties for the element.</returns>
        protected override ConfigurationPropertyCollection Properties
        {
            get { return properties; }
        }

        /// <summary>
        /// Gets the element name for a RuntimeCollection
        /// </summary>
        protected override string ElementName
        {
            get { return "runtime"; }
        }

        /// <summary>
        /// Gets a runtime configuration by index.
        /// </summary>
        /// <param name="index">Zero-based index of a runtime configuration.</param>
        /// <returns>The RuntimeElement at the specified index.</returns>
        public RuntimeElement this[int index]
        {
            get 
            { 
                return (RuntimeElement)this.BaseGet(index); 
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
        /// <returns>The RuntimeElement with the specified name.</returns>
        public new RuntimeElement this[string name]
        {
            get { return (RuntimeElement)this.BaseGet(name); }
        }

        /// <summary>
        /// This method gets an element key name for a RuntimeElement.
        /// </summary>
        /// <param name="element">The RuntimeElement for which to get the key.</param>
        /// <returns>A string that is the name of the Runtime configuration.</returns>
        protected override object GetElementKey(ConfigurationElement element)
        {
            return (element as RuntimeElement).Name;
        }

        /// <summary>
        /// This method creates a new RuntimeElement.
        /// </summary>
        /// <returns>A new RuntimeElement.</returns>
        protected override ConfigurationElement CreateNewElement()
        {
            return new RuntimeElement();
        }
    }
}