Quota and accounting implementation:

Windows provides to mechanisms for disk space quotas:
 - FSRM - File Server Resource Manager
    has a lot of extra functionality and can work per directory
    it is very good when applying quotas for a shred folder
    best suited for uhurufs - beacuse it can be enforces on a directory basis without taking into account 
      the user the user that is accessing the share
 - NTFS Quotas
    can be enforced only per volume for a particular user
    only files for the respective user is takan into account for a rule
    more appropiate for the DEA - it will account every file used on the disk (i.e. temp files, public folders)   
More info here:
 http://waynes-world-it.blogspot.ro/2009/02/2003-fsrm-and-ntfs-quotas-compared.html
 http://technet.microsoft.com/en-us/library/cc754810(v=ws.10).aspx
 
 
Another method to enforce quota and account is to create and mount a VHD per service instance.
The VHD method consumes more resources (mounting or created more then 250 VHD will take a long create and consume
a lot of Kernel memory) but the isolation is more strict. Another issue is managaing expandable (Thin provisioning
in VMware's terms). After some disk space has been freed in the VHD's file system, the system has to manualy 
compat the VHD to reduce the size of the VHD file, which will requrie the disk to be offline (maybe some tricks
can be made with Mirror Volumes to compat a VHD without taking it offline).

This feature has to be installed on the windows server box:
 FS-Resource-Manager: `powershell.exe -command "& {Import-Module Servermanager; add-windowsfeature FS-Resource-Manager}"`