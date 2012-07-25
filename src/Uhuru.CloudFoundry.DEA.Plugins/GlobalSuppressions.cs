// -----------------------------------------------------------------------
// <copyright file="GlobalSuppressions.cs" company="Uhuru Software, Inc.">
// Copyright (c) 2011 Uhuru Software, Inc., All Rights Reserved
// </copyright>
// -----------------------------------------------------------------------

// This file is used by Code Analysis to maintain SuppressMessage 
// attributes that are applied to this project.
// Project-level suppressions either have no target or are given 
// a specific target and scoped to a namespace, type, member, etc.
//
// To add a suppression to this file, right-click the message in the 
// Error List, point to "Suppress Message(s)", and click 
// "In Project Suppression File".
// You do not need to add suppressions to this file manually.

[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1020:AvoidNamespacesWithFewTypes", Scope = "namespace", Target = "Uhuru.CloudFoundry.DEA.Plugins", Justification = "This is a plugin, isolated to keep things simple.")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Plugins", Justification = "The word is in the dictionary, but the warning is still generated.")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Scope = "member", Target = "Uhuru.CloudFoundry.DEA.Plugins.IISPlugin.#AutowireUhurufs(Uhuru.CloudFoundry.DEA.PluginBase.ApplicationInfo,Uhuru.CloudFoundry.DEA.PluginBase.ApplicationVariable[],Uhuru.CloudFoundry.DEA.PluginBase.ApplicationService[],System.String)")]
