Uhuru Software .Net Extensions to Cloud Foundry
===============================================

Copyright (c) 2011 Uhuru Software, Inc., All Rights Reserved

What are the .Net Extensions to Cloud Foundry?
----------------------------------------------

This project is an effort to extend Cloud Foundry so it runs .Net web applications on a Windows environment.

Although Cloud Foundry is developed on Linux/Ruby, our stack is 100% .Net. We have ported the NATS client (NATS is the message bus used for communication by all Cloud Foundry components), the DEA (Droplet Execution Engine) and the Service Node base. This means that now we can bring in frameworks and services on Cloud Foundry that run on Windows.

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

Install "Windows DEA.msi" and "MS Sql Node.msi" on a box that meets the following prerequisites:

* Windows 7 or Windows 2008 Server R2
* IIS 7
* MS SQL Server 2008 R2

Detailed Install/Run Instructions:
----------------------------------




Trying your setup
-----------------

