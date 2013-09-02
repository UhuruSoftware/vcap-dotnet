TODO:
 - Network traffic upload limit 
   WMI interface: http://msdn.microsoft.com/en-us/library/windows/desktop/hh872446(v=vs.85).aspx
   PowerShell interface: http://technet.microsoft.com/en-US/library/hh967471.aspx

 - Network firewall rules per User (Support only from Windows Server 2012)
   PowerShell interface: http://technet.microsoft.com/en-US/library/jj554908.aspx
   WMI interface: http://msdn.microsoft.com/en-us/library/jj676843(v=vs.85).aspx
   COM interface: http://msdn.microsoft.com/en-us/library/windows/desktop/hh447468(v=vs.85).aspx

   LocalUser SDDL Specified here:
   "O:LSD:(A;;CC;;;{0})" is the format saved by the Windows MMC Firewall GUI Tool
   or much simple without the owner "D:(A;;CC;;;{0})"
   http://technet.microsoft.com/en-us/library/cc753463(v=ws.10).aspx
   PowerShell POC:
   New-NetFirewallRule -DisplayName prison-d946bfcf-4f61 -Action Block -Direction Outbound -LocalUser 'D:(A;;CC;;;{0})' # Replace {0} with the users SID
   New-NetFirewallRule -DisplayName prison-d946bfcf-4f61 -Action Block -Direction Outbound -LocalUser "D:(A;;CC;;;S-1-5-21-1781455180-1624663491-1037019526-2354)"

 - Create dedicated Windows Station and Windows Destion for non-interactive prisons

ProcessPrisson will run Windows processes with some security and resource restrictions.
 - Dedicated local Windows user
 - Processes will start taged with a Job Object
 - Disk Quota enforcement per local Windows user
