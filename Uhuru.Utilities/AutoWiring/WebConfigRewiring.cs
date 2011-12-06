// -----------------------------------------------------------------------
// <copyright file="WebConfigRewiring.cs" company="Uhuru Software">
// Copyright (c) 2011 Uhuru Software, Inc., All Rights Reserved
// </copyright>
// -----------------------------------------------------------------------

namespace Uhuru.Autowiring
{
    using System;
    using System.Xml;
    using System.Xml.XPath;
    
    public enum ParentSection
    { 
        SystemWeb 
    }

    public interface ISiteConfigManager
    {
        bool AllowExternalSource
        {
            get;
            set;
        }
        
        void RegisterSectionRewire(INodeConfigRewireBase nodeConfig);

        IXPathNavigable CreateNewSection(INodeConfigRewireBase nodeConfig);

        void Rewire(bool backupOriginal);

        void CommitChanges();
    }

    public interface INodeConfigRewireBase
    {
        string ConfigSectionName
        {
            get;
            set;
        }

        ParentSection ConfigParent
        {
            get;
            set;
        }

        bool HasExternalSource
        {
            get;
            set;
        }

        string ExternalSource
        {
            get;
            set;
        }

        void Register(ISiteConfigManager siteConfigManager);

        IXPathNavigable RewireConfig(IXPathNavigable configNode, bool createNewIfNotPresent);
    }
    
    public class HealthMonRewire : INodeConfigRewireBase
    {
        private string healthMonExtSource = "UhuruAspNetEventProvider.config";
        private ISiteConfigManager configmanager;
                
        public string ConfigSectionName
        {
            get { return "healthMonitoring"; }
            set { }
        }

        public ParentSection ConfigParent
        {
            get { return ParentSection.SystemWeb; }
            set { }
        }

        public bool HasExternalSource
        {
            get { return true; }
            set { }
        }

        public string ExternalSource
        {
            get { return this.healthMonExtSource; }
            set { }
        }

        public void Register(ISiteConfigManager siteConfigManager)
        {
            if (siteConfigManager == null)
            {
                throw new ArgumentNullException("siteConfigManager");
            }

            this.configmanager = siteConfigManager;
            siteConfigManager.RegisterSectionRewire(this);
        }

        public IXPathNavigable RewireConfig(IXPathNavigable configNode, bool createNewIfNotPresent)
        {
            XmlNode tempConfig = (XmlNode)configNode;

            if (tempConfig == null)
            {
                if (createNewIfNotPresent)
                {
                    tempConfig = (XmlNode)this.configmanager.CreateNewSection(this);
                }
                else
                {
                    throw new ArgumentException("Creation of a new config section not allowed");
                }
            }
            else
            {
                tempConfig.RemoveAll();
            }

            if (this.configmanager.AllowExternalSource)
            {
                XPathNavigator nav = tempConfig.CreateNavigator();
                nav.CreateAttribute(null, "configSource", null, this.healthMonExtSource);
            }
            else
            {
                throw new NotImplementedException("logging auto-wiring only supports configuration from an external source at this time");
            }

            return tempConfig;
        }
    }


}