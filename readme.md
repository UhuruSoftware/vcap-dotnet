Uhuru Software .Net Extensions to Cloud Foundry
===============================================

Copyright (c) 2011 Uhuru Software, Inc., All Rights Reserved

What are the .Net Extensions to Cloud Foundry?
----------------------------------------------

This project is an effort to extend Cloud Foundry so it runs .Net web applications on a Windows environment.

Although Cloud Foundry is developed on Linux/Ruby, our stack is 100% .Net. We have ported the NATS client (NATS is the message bus used for communication by all Cloud Foundry components), the DEA (Droplet Execution Engine) and the Service Node base. This means that now we can bring in frameworks and services on Cloud Foundry that run on Windows. We hope that since it's .Net, adding frameworks/services for Windows will be easier and faster for Windows devs, as well as more stable than Ruby on Windows.

So far we've added:

* A dotNet framework that runs .Net 2.0, 3.5 and 4.0 web applications on IIS 7
* An mssql system service that allows you to provision MS SQL Server 2008 R2 databases using Cloud Foundry

License
-------

This project uses the Apache 2 license.  See LICENSE for details.

Installation Notes
------------------

To run .Net on Cloud Foundry, the first thing you need is a Cloud Foundry installation. Refer to [http://www.github.com/CloudFoundry/vcap](http://www.github.com/CloudFoundry/vcap) for details on how to setup that up.

Once you have a Cloud Foundry setup running, you can either:

* get the code and compile it to get the installers (use the vcap-dotnet-installers solution)
* grab the latest release from here [http://www.uhurusoftware.com/vcap-dotnet/latest](http://www.uhurusoftware.com/vcap-dotnet/latest)

Install "WindowsDEA.msi" and "MSSqlNode.msi" on a box that meets the following prerequisites:

* Windows 7 or Windows 2008 Server R2
* IIS 7
* MS SQL Server 2008 R2

Detailed Install/Run Instructions:
----------------------------------

### Installing the WindowsDEA

You have to run the WindowsDEA.msi installer in order to install the service. There are two ways to do that.

#### Install from from command line using following command and parameters: msiexec /i WindowsDEA.msi [parameterName=parameterValue]

Valid parameters that can be used are: 
	
	baseDir (string)
	localRoute (string)
	filerPort (int) - port for the fileserver
	messageBus (string) - message bus address (nats://user:password@someip:port/)
	multiTenant (boolean)
	maxMemory (integer)
	secure (boolean)
	enforceUlimit (integer)
	heartBeatInterval (integer)
	forceHttpSharing (boolean)

#### Install using the UI. In this case, you will have to edit all the configuration manually after the installation in {installationDirectory}/uhuru.config.

After the installation start the DeaWindowsService in services.msc

### Installing the MSSqlNode

You have to run the MSSqlNode.msi installer in order to install the service. There are two ways to do that.

#### Install from from command line using following command and parameters: msiexec /i MSSqlNode.msi [parameterName=parameterValue]

Valid parameters that can be used are: 

	nodeId (string)
	migrationNfs (string)
	mbus (string) - message bus address (nats://user:password@someip:port/)
	index (integer)
	zInterval (integer)
	maxDbSize (integer)
	maxLongQuery (integer)
	maxLongTx (integer)
	localDb (string)
	baseDir (string)
	localRoute (string)
	availableStorage (integer)
	host (string) - MSSql Server address
	user (string) - MSSql Server user
	password (string) - MSSql Server password
	port (integer) - MSSql Server port

#### Install using the UI. In this case, you will have to edit all the configuration manually after the installation in {installationDirectory}/uhuru.config.

After the installation start the MssqlNodeService in services.msc

Trying your setup
-----------------


Running the tests
-----------------

### Unit Tests

#### Configure
#### Run

### Integration Tests

#### Configure
#### Run

### System Tests

#### Configure
#### Run
