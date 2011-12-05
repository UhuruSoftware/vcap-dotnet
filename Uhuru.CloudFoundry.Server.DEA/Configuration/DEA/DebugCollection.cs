using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;

namespace Uhuru.CloudFoundry.DEA.Configuration.DEA
{
    [ConfigurationCollection(typeof(DebugElement), CollectionType = ConfigurationElementCollectionType.BasicMap)]
    public class DebugCollection : ConfigurationElementCollection
    {
        #region Constructors
        static DebugCollection()
        {
            properties = new ConfigurationPropertyCollection();
        }

        public DebugCollection()
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
            get { return "debugConfiguration"; }
        }

        #endregion

        #region Indexers
        public DebugElement this[int index]
        {
            get { return (DebugElement)base.BaseGet(index); }
            set
            {
                if (base.BaseGet(index) != null)
                {
                    base.BaseRemoveAt(index);
                }
                base.BaseAdd(index, value);
            }
        }

        public new DebugElement this[string name]
        {
            get { return (DebugElement)base.BaseGet(name); }
        }
        #endregion

        #region Overrides
        protected override ConfigurationElement CreateNewElement()
        {
            return new DebugElement();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return (element as DebugElement).Name;
        }
        #endregion
    }
}
