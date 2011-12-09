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

To run .Net on Cloud Foundry, the first thing you need is a Cloud Foundry installation. Refer to the [http://www.github.com/CloudFoundry/vcap](vcap repo) for details on how to setup that up.

Once you have a Cloud Foundry setup running, you can either:

* get the code and compile it to get the installers (use the vcap-dotnet-installers solution)
* grab the latest release from [http://www.uhurusoftware.com/vcap-dotnet/latest](here)

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
All the tests have been implemented using MSTest framework.

### Unit Tests
Unit tests take the smallest piece of testable software in the application, isolate it from the remainder of the code, and determine whether it behaves correctly.
#### Configure
The Unit Tests run out of the box, no additional configuration is needed.
#### Run
1. Open Visual studio command prompt
2. Build vcap-dotnet solution
msbuild {cloneDirectory}vcap-dotnet.sln
3. Run tests in the "Unit" category using MSTest
MSTest.exe /testcontainer:{cloneDirectory}\..\bin-vcap-dotnet\Uhuru.CloudFoundry.Test.dll /category:"Unit"

### Integration Tests
This type of tests ensure that all the functional requirements are met at the component level.
#### Configure
This tests require a working NATS Serve deployment.
To edit the NATS Server used for the tests follow the steps:
1. Go to Uhuru.CloudFoundtry.Test project
cd {clonePath}\Uhuru.CloudFoundry.Test\
2. Edit the App.config file
notepad App.config 
3. Set a valid NATS Server for the "nats" key:
 <add key="nats" value="nats://nats:nats@192.168.1.120:4222"/>
#### Run
1. Open Visual studio command prompt
2. Build vcap-dotnet solution
msbuild {cloneDirectory}vcap-dotnet.sln
3. Build CloudTestApp solution
msbuild {cloneDirectory}\TestApps\CloudTestApp\CloudTestApp.sln
4. Run tests in the "Integration" category using MSTest
MSTest.exe /testcontainer:{cloneDirectory}\..\bin-vcap-dotnet\Uhuru.CloudFoundry.Test.dll /category:"Integration"

### System Tests
System testing is conducted on the complete, integrated system to evaluate the system’s compliance with the specified requirements.
#### Configure
To run the System Tests you must have a full deployment as described above, in the deployment section. Additional configuration steps are described bellow:
 1. Go to Uhuru.CloudFoundtry.Test project
cd {clonePath}\Uhuru.CloudFoundry.Test\
2. Edit the App.config file
notepad App.config 
3. Set a valid NATS Server for the nats key:
 <add key="nats" value="nats://nats:nats@192.168.1.120:4222"/>
4. Set the target CloudFoundry deployment
<add key="target" value="api.uhurucloud.net"/>
5. Set the user name for the deployment
<add key="username" value="continuousintegration@uhurusoftware.com"/>
6. Set the password for the deployment
<add key="password" value="myPassword"/>
7. Set the Umbraco root directory
<add key="umbracoRootDir" value="C:\PathToUmbraco"/>
#### Run
1. Open Visual studio command prompt
2. Build vcap-dotnet solution
msbuild {cloneDirectory}vcap-dotnet.sln
3. Build CloudTestApp solution
msbuild {cloneDirectory}\TestApps\CloudTestApp\CloudTestApp.sln
4. Run tests in the "Sytem" category using MSTest
MSTest.exe /testcontainer:{cloneDirectory}\..\bin-vcap-dotnet\Uhuru.CloudFoundry.Test.dll /category:"System"
