for /F "skip=2 tokens=3" %%G in ('reg query HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\InetStp /v MajorVersion') do echo %%G