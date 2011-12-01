using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.ComponentModel;
using System.Runtime.InteropServices;
using EnvDTE;
using Uhuru.CloudFoundry.UI.Packaging;
using Uhuru.CloudFoundry.UI.VS2010;

namespace Uhuru.CloudFoundry.UI.VS2010.Extensions
{
    [CLSCompliant(false)]
    public class CloudProjectExtenderProvider : EnvDTE.IExtenderProvider
    {

        public static string[] ProjectTypesToExtend = new string[]
        {
            "{4EF9F003-DE95-4d60-96B0-212979F2A857}",    //VSLangProj.PrjBrowseObjectCATID.prjCATIDCSharpProjectBrowseObject
            "{E0FDC879-C32A-4751-A3D3-0B3824BD575F}",    //VSLangProj.PrjBrowseObjectCATID.prjCATIDVBProjectBrowseObject
            "{EEF81A81-D390-4725-B16D-E103E0F967B4}",    //VsWebSite.PrjBrowseObjectCATID.prjCATIDWebSiteProjectBrowseObject
        };

        private static string dynamicExtenderName = "UhuruCloudProjectPropertiesExtender";


        public static string DynamicExtenderName
        {
            get
            {
                return dynamicExtenderName;
            }
        }

        public bool CanExtend(string ExtenderCATID, string ExtenderName, object ExtendeeObject)
        {
            System.ComponentModel.PropertyDescriptor extendeeCATIDProp = TypeDescriptor.GetProperties(ExtendeeObject)["ExtenderCATID"];

            bool IfCanExtend = ExtenderName == dynamicExtenderName &&
                 ProjectTypesToExtend.Any(row => row.ToLower() == ExtenderCATID.ToLower()) &&
                 extendeeCATIDProp != null &&
                 ProjectTypesToExtend.Any(row => row.ToLower() == extendeeCATIDProp.GetValue(ExtendeeObject).ToString().ToLower());

            return IfCanExtend;
        }

        public object GetExtender(string ExtenderCATID, string ExtenderName, object ExtendeeObject, EnvDTE.IExtenderSite ExtenderSite, int Cookie)
        {
            
            CloudProjectProperties dynamicExtender = null;

            EnvDTE.Project proj = null;

            proj = ExtendeeObject as EnvDTE.Project;
            if (proj == null)
            {
                object[] selectedProjects = (object[])((EnvDTE.DTE)ExtenderSite.GetObject("")).ActiveSolutionProjects;

                if (selectedProjects.Length == 1)
                {
                    proj = (Project)selectedProjects[0];
                }
            }

            if (CanExtend(ExtenderCATID, ExtenderName, ExtendeeObject) && proj != null)
            {
                dynamicExtender = new CloudProjectProperties();
                dynamicExtender.Initialize(proj);
            }

            return dynamicExtender;
        }
    }
}