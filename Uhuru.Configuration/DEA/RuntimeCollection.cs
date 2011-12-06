﻿// -----------------------------------------------------------------------
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
    [ConfigurationCollection(typeof(RuntimeElement), CollectionType = ConfigurationElementCollectionType.BasicMap)]
    public class RuntimeCollection : ConfigurationElementCollection
    {
        #region Fields
        private static ConfigurationPropertyCollection properties;
        #endregion

        #region Constructors
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

        #endregion

        #region Properties
        /// Defines the configuration properties available for a RuntimeCollection
        protected override ConfigurationPropertyCollection Properties
        {
            get { return properties; }
        }

        /// <summary>
        /// Defines the collection type (BasicMap) for RuntimeCollection
        /// </summary>
        public override ConfigurationElementCollectionType CollectionType
        {
            get { return ConfigurationElementCollectionType.BasicMap; }
        }

        /// <summary>
        /// Gets the element name for a RuntimeCollection
        /// </summary>
        protected override string ElementName
        {
            get { return "runtime"; }
        }
        #endregion

        #region Indexers

        /// <summary>
        /// Gets a runtime configuration by index.
        /// </summary>
        /// <param name="index">Zero-based index of a runtime configuration.</param>
        /// <returns>The RuntimeElement at the specified index.</returns>
        public RuntimeElement this[int index]
        {
            get { return (RuntimeElement)base.BaseGet(index); }
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
        /// Gets a runtime configuration by its name.
        /// </summary>
        /// <param name="name">String specifying the runtime name.</param>
        /// <returns>The RuntimeElement with the specified name.</returns>
        public new RuntimeElement this[string name]
        {
            get { return (RuntimeElement)base.BaseGet(name); }
        }
        #endregion

        #region Overrides
        /// <summary>
        /// This method creates a new RuntimeElement.
        /// </summary>
        /// <returns>A new RuntimeElement.</returns>
        protected override ConfigurationElement CreateNewElement()
        {
            return new RuntimeElement();
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
        #endregion
    }
}