// -----------------------------------------------------------------------
// <copyright file="EnvironmentCollection.cs" company="Uhuru Software">
// Copyright (c) 2011 Uhuru Software, Inc., All Rights Reserved
// </copyright>
// -----------------------------------------------------------------------

namespace Uhuru.Configuration.DEA
{
    using System.Configuration;
    
    /// <summary>
    /// This class is a collection of EnvironmentElement.
    /// </summary>
    [ConfigurationCollection(typeof(EnvironmentElement), CollectionType = ConfigurationElementCollectionType.BasicMap)]
    public class EnvironmentCollection : ConfigurationElementCollection
    {
        #region Constructors
        static EnvironmentCollection()
        {
            properties = new ConfigurationPropertyCollection();
        }

        /// <summary>
        /// Public parameterless constructor.
        /// </summary>
        public EnvironmentCollection()
        {
        }
        #endregion

        #region Fields
        private static ConfigurationPropertyCollection properties;
        #endregion

        #region Properties
        /// Defines the configuration properties available for a EnvironmentCollection
        protected override ConfigurationPropertyCollection Properties
        {
            get { return properties; }
        }

        /// <summary>
        /// Defines the collection type (BasicMap) for EnvironmentCollection
        /// </summary>
        public override ConfigurationElementCollectionType CollectionType
        {
            get { return ConfigurationElementCollectionType.BasicMap; }
        }

        /// <summary>
        /// Gets the element name for a EnvironmentCollection
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
        /// <param name="index">Zero-based index of a environment variable.</param>
        /// <returns>The EnvironmentElement at the specified index.</returns>
        public EnvironmentElement this[int index]
        {
            get { return (EnvironmentElement)base.BaseGet(index); }
            set
            {
                if (base.BaseGet(index) != null)
                {
                    base.BaseRemoveAt(index);
                }
                base.BaseAdd(index, value);
            }
        }

        /// <summary>
        /// Gets a environment variable by its name.
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
