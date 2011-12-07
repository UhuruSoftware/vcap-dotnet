// -----------------------------------------------------------------------
// <copyright file="EnvironmentCollection.cs" company="Uhuru Software">
// Copyright (c) 2011 Uhuru Software, Inc., All Rights Reserved
// </copyright>
// -----------------------------------------------------------------------

namespace Uhuru.Configuration.DEA
{
    using System.Configuration;
    
    /// <summary>
    /// This class is a collection of EnvironmentElement items.
    /// </summary>
    [ConfigurationCollection(typeof(EnvironmentElement), CollectionType = ConfigurationElementCollectionType.BasicMap)]
    public class EnvironmentCollection : ConfigurationElementCollection
    {
        #region Fields
        private static ConfigurationPropertyCollection properties;
        #endregion

        #region Constructors

        /// <summary>
        /// Initializes static members of EnvironmentCollection class.
        /// </summary>
        static EnvironmentCollection()
        {
            properties = new ConfigurationPropertyCollection();
        }

        /// <summary>
        /// Initializes a new instance of the EnvironmentCollection class.
        /// </summary>
        public EnvironmentCollection()
        {
        }

        #endregion

        #region Properties
        
        /// <summary>
        /// Defines the configuration properties available for the current object.
        /// </summary>
        protected override ConfigurationPropertyCollection Properties
        {
            get { return properties; }
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

        #endregion

        #region Indexers

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
            get { return (EnvironmentElement)base.BaseGet(name); }
        }

        #endregion

        #region Overrides

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
        #endregion
    }
}
