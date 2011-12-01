using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;

namespace CloudFoundry.Net.Configuration.DEA
{
    [ConfigurationCollection(typeof(EnvironmentElement), CollectionType = ConfigurationElementCollectionType.BasicMap)]
    public class EnvironmentCollection : ConfigurationElementCollection
    {
        #region Constructors
        static EnvironmentCollection()
        {
            properties = new ConfigurationPropertyCollection();
        }

        public EnvironmentCollection()
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
            get { return "env"; }
        }

        #endregion

        #region Indexers
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

        public new EnvironmentElement this[string name]
        {
            get { return (EnvironmentElement)base.BaseGet(name); }
        }
        #endregion

        #region Overrides
        protected override ConfigurationElement CreateNewElement()
        {
            return new EnvironmentElement();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return (element as EnvironmentElement).Name;
        }
        #endregion
    }
}
