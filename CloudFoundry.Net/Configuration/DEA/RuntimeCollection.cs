using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;

namespace CloudFoundry.Net.Configuration.DEA
{
    [ConfigurationCollection(typeof(RuntimeElement), CollectionType = ConfigurationElementCollectionType.BasicMap)]
    public class RuntimeCollection : ConfigurationElementCollection
    {
        #region Constructors
        static RuntimeCollection()
        {
            properties = new ConfigurationPropertyCollection();
        }

        public RuntimeCollection()
        {
        }
        #endregion

        #region Fields
        private static ConfigurationPropertyCollection properties;
        #endregion

        #region Properties
        protected override ConfigurationPropertyCollection Properties
        {
            get { return properties; }
        }

        public override ConfigurationElementCollectionType CollectionType
        {
            get { return ConfigurationElementCollectionType.BasicMap; }
        }

        protected override string ElementName
        {
            get { return "runtime"; }
        }
        #endregion

        #region Indexers
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

        public new RuntimeElement this[string name]
        {
            get { return (RuntimeElement)base.BaseGet(name); }
        }
        #endregion

        #region Overrides
        protected override ConfigurationElement CreateNewElement()
        {
            return new RuntimeElement();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return (element as RuntimeElement).Name;
        }
        #endregion
    }
}