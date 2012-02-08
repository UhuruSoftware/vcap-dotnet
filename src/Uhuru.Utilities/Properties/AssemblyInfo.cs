// -----------------------------------------------------------------------
// <copyright file="AssemblyInfo.cs" company="Uhuru Software, Inc.">
// Copyright (c) 2011 Uhuru Software, Inc., All Rights Reserved
// </copyright>
// -----------------------------------------------------------------------

using System;
using System.Reflection;
using System.Resources;
using System.Runtime.InteropServices;

// General Information about an assembly is controlled through the following 
// set of attributes. Change these attribute values to modify the information
// associated with an assembly.
[assembly: AssemblyTitle("Uhuru Utilities Library")]
[assembly: AssemblyDescription("")]
[assembly: AssemblyCompany("Uhuru Software, Inc.")]
[assembly: AssemblyProduct("Uhuru Utilities Library")]
[assembly: AssemblyCopyright("Copyright (c) 2011 Uhuru Software, Inc., All Rights Reserved")]
[assembly: AssemblyTrademark("")]
[assembly: AssemblyCulture("")]

// Setting ComVisible to false makes the types in this assembly not visible 
// to COM components.  If you need to access a type in this assembly from 
// COM, set the ComVisible attribute to true on that type.
[assembly: ComVisible(false)]

// The following GUID is for the ID of the typelib if this project is exposed to COM
#if UTILITIES35
[assembly: Guid("D5717C45-6D8A-4A26-87AA-41A5B237B60B")]
#else
[assembly: Guid("22C817D4-7A55-4E74-8FDE-61755DA326F7")]
#endif

// Version information for an assembly consists of the following four values:
//
//      Major Version
//      Minor Version 
//      Build Number
//      Revision
//
// You can specify all the values or you can default the Build and Revision Numbers 
// by using the '*' as shown below:
// [assembly: AssemblyVersion("1.0.*")]
[assembly: AssemblyVersion("0.9.0.0")]
[assembly: CLSCompliant(true)]
[assembly: log4net.Config.XmlConfigurator(ConfigFile = "log4net.config", Watch = true)]
[assembly: NeutralResourcesLanguageAttribute("")]
