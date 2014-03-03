Uhuru Software .Net Extensions to Cloud Foundry
===============================================

This is the repository for the Uhuru vcap-dotnet code. This is includes the Droplet Execution Agent (DEA) and Service Nodes written in C# designed to allow Windows Server to run in a Cloud Foundry 2.0 environment.

What are the .NET Extensions to Cloud Foundry?
----------------------------------------------

This project is an effort to extend Cloud Foundry so it runs .Net web applications on a Windows environment.

Cloud Foundry was developed on Linux and Ruby and lacks support for Microsoft Windows Server environments. The .NET Extensions from Uhuru software are built entirely on Windows and .NET. We have ported the Cloud Foundry NATS client message bus, DEA (Droplet Execution Engine) and the Service Node base components to .NET and Windows Server. The Uhuru Software .NET Extensions allow Windows Servers to be full-fledged Cloud Foundry 2.0 citizens. Windows developers can now benefit from the same Cloud Foundry application deployment advances that Ruby developers already enjoy.

The .NET Extensions also make it possible for the open source developer community to add new Cloud Foundry enabled frameworks and services to Windows Servers.

Deploying the extensions 
------------------------

The Windows extensions can be deployed using BOSH just like the rest of Cloud Foundry. See the [Cloud Foundry Release](http://github.com/UhuruSoftware/cf-release) and [Uhuru Cloud Commander](http://github.com/UhuruSoftware/uhuru-commander) repositories for more details. 

### Notice of Export Control Law

This software distribution includes cryptographic software that is subject to the U.S. Export Administration Regulations (the "EAR") and other U.S. and foreign laws and may not be exported, re-exported or transferred (a) to any country listed in Country Group E:1 in Supplement No. 1 to part 740 of the EAR (currently, Cuba, Iran, North Korea, Sudan & Syria); (b) to any prohibited destination or to any end user who has been prohibited from participating in U.S. export transactions by any federal agency of the U.S. government; or (c) for use in connection with the design, development or production of nuclear, chemical or biological weapons, or rocket systems, space launch vehicles, or sounding rockets, or unmanned air vehicle systems.You may not download this software or technical information if you are located in one of these countries or otherwise subject to these restrictions. You may not provide this software or technical information to individuals or entities located in one of these countries or otherwise subject to these restrictions. You are also responsible for compliance with foreign law requirements applicable to the import, export and use of this software and technical information.