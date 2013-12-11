$script:scriptPath = Join-Path $env:HOME iishwc
$script:appPoolName = [Guid]::NewGuid().ToString()
$script:appPort = $Env:PORT
$script:appPath = (get-item $script:scriptPath).parent.FullName
$script:logsDir = Join-Path $env:HOMEPATH logs
$script:exitCode = 0

function DetectBitness()
{
    Write-Host("Detecting application bitness...");
	$assemblies = @(Get-ChildItem -Path $script:appPath -Filter "*.dll" -Recurse)
	foreach ($assembly in $assemblies)
	{
		$kind = new-object Reflection.PortableExecutableKinds
        $machine = new-object Reflection.ImageFileMachine
        try
        {
            $a = [Reflection.Assembly]::ReflectionOnlyLoadFrom($assembly.Fullname)
            $a.ManifestModule.GetPEKind([ref]$kind, [ref]$machine)
        }
        catch
        {
            Write-Error("Could not detect bitness for assembly $($assembly.Name)");
            $kind = [System.Reflection.PortableExecutableKinds]"NotAPortableExecutableImage"
        }
		
		switch ($kind)
		{
			[System.Reflection.PortableExecutableKinds]::Required32Bit
			{
                Write-Host("Application requires a 32bit enabled application pool");
				return ,$true;                
			}			
			([System.Reflection.PortableExecutableKinds]([System.Reflection.PortableExecutableKinds]::Required32Bit -bor [System.Reflection.PortableExecutableKinds]::ILOnly))
			{
                Write-Host("Application requires a 32bit enabled application pool");
				return ,$true;
			}
			default { }
		}			  		
	}
	
    Write-Host("Application does not require a 32bit enabled application pool");
	return $false
}

function GetFrameworkFromConfig()
{
    Write-Host("Detecting required asp.net version...");
	$webConfigPath = Join-Path $script:appPath 'web.config'
	$webConfig = New-Object System.Xml.XmlDocument
	$webConfig.Load($webConfigPath)

	$node = $webConfig.SelectSingleNode("/configuration/system.web/compilation/@targetFramework")
	if ($node)
	{
        Write-Host("Application requires asp.net v4.0");
		return "v4.0"
	}
	else
	{
        Write-Host("Application requires asp.net v2.0");
		return "v2.0"
	}
}

function AddApplicationPool([ref]$applicationHost)
{
    Write-Output("Creating application pool in applicationHost.config")
	$enable32bit = DetectBitness
	if ($enable32bit -eq $true)
	{
		$script:exitCode = 1
	}

    $defaults = $applicationHost.Value.SelectSingleNode("/configuration/system.applicationHost/applicationPools/applicationPoolDefaults/processModel")
    $defaults.SetAttribute("identityType", "SpecificUser")
    $defaults.SetAttribute("userName", $env:VCAP_WINDOWS_USER)
    $defaults.SetAttribute("password", $env:VCAP_WINDOWS_USER_PASSWORD)

	$element = $applicationHost.Value.CreateElement("add")
	$element.SetAttribute('name', $script:appPoolName)
	$element.SetAttribute('enable32BitAppOnWin64', $enable32bit)
	$framework = GetFrameworkFromConfig
	$element.SetAttribute('managedRuntimeVersion', $framework)
	$null = $applicationHost.Value.configuration."system.applicationHost".applicationPools.AppendChild($element)
}

function AddSite([ref]$applicationHost, $appName)
{
    Write-Output("Creating site in applicationHost.config")
	$appName = [Guid]::NewGuid().ToString()
	$element = $applicationHost.Value.CreateElement("site")
	$element.SetAttribute('name', $appName)
	$element.SetAttribute('id', 1)
	$null = $applicationHost.Value.configuration."system.applicationHost".sites.AppendChild($element)
}

function AddBinding([ref]$applicationHost)
{
    Write-Output("Adding http bindings for application")
	$bindings = $applicationHost.Value.CreateElement("bindings")	
	$element = $applicationHost.Value.CreateElement("binding")
	$element.SetAttribute('protocol', "http")
	$element.SetAttribute("bindingInformation", [String]::Format("*:{0}:", $script:appPort ))	
	$null = $bindings.AppendChild($element)
	$null = $applicationHost.Value.configuration."system.applicationHost".sites.site.AppendChild($bindings)	
}

function AddApplication([ref]$applicationHost)
{
    Write-Output("Adding application and virtual directory to site")
	$application = $applicationHost.Value.CreateElement("application")
	$application.SetAttribute('path', '/')
	$application.SetAttribute('applicationPool', $script:appPoolName)
	$virtualDirectory = $applicationHost.Value.CreateElement("virtualDirectory")
	$virtualDirectory.SetAttribute('path', '/')
	$virtualDirectory.SetAttribute('physicalPath', $script:appPath)
	$null = $application.AppendChild($virtualDirectory)
	$null = $applicationHost.Value.configuration."system.applicationHost".sites.site.AppendChild($application)	
}

$applicationHostTemplatePath = Join-Path $script:scriptPath 'applicationHostTemplate.config'
$applicationHostPath = Join-Path $script:scriptPath 'applicationHost.config'
Copy-Item -Force $applicationHostTemplatePath $applicationHostPath

$applicationHost = New-Object System.Xml.XmlDocument
$applicationHost.Load($applicationHostPath)

AddApplicationPool ([ref]$applicationHost)
AddSite ([ref]$applicationHost)
AddBinding ([ref]$applicationHost)
AddApplication ([ref]$applicationHost)

$applicationHost.Save($applicationHostPath)

Write-Output("Autowiring service connection strings if necessary")
$vcap_services = $Env:VCAP_SERVICES | ConvertFrom-Json
$configFiles= @(get-childitem $script:appPath *.config -rec)
foreach ($file in $configFiles)
{
    foreach ($serviceName in $vcap_services | Get-Member -MemberType NoteProperty) {
        foreach($service in $vcap_services."$($serviceName.Name)") {
            foreach($property in $service.credentials | Get-Member -MemberType NoteProperty) {
                $content = Get-Content($file.PSPath)                
                $content | Foreach-Object {$_ -replace "{$($service.name)#$($property.Name)}", $service.credentials."$($property.Name)"} | 
                Set-Content $file.PSPath
            }
        }
    }        
}

$webConfigPath = Join-Path $script:appPath "web.config"
$webConfig = New-Object System.Xml.XmlDocument
$webConfig.Load($webConfigPath)

$appSettings = $webConfig.SelectSingleNode("/configuration/appSettings")
if($appSettings -eq $null)
{
    $appSettings = $webConfig.CreateElement("appSettings")
    $configuration = $webConfig.SelectSingleNode("/configuration")
    $null = $configuration.AppendChild($appSettings)
}
$element = $webConfig.CreateElement("add")
$element.SetAttribute('key', "UHURU_LOG_FILE")
$element.SetAttribute('value', (Join-Path $script:logsDir iis.stdout.log))
$null = $appSettings.AppendChild($element)
$element = $webConfig.CreateElement("add")
$element.SetAttribute('key', "UHURU_ERROR_LOG_FILE")
$element.SetAttribute('value', (Join-Path $script:logsDir iis.stderr.log))
$null = $appSettings.AppendChild($element)

$healthMonitoring = $webConfig.SelectSingleNode("/configuration/system.web/healthMonitoring")
if($healthMonitoring -eq $null)
{
    $healthMonitoring = $webConfig.CreateElement("healthMonitoring")
    $systemWeb = $webConfig.SelectSingleNode("/configuration/system.web")
    $healthMonitoring.SetAttribute("configSource", "UhuruAspNetEventProvider.config")
    $null = $systemWeb.AppendChild($healthMonitoring)
}

$webConfig.Save($webConfigPath)

$null = mkdir (Join-Path $env:TEMP "IIS Temporary Compressed Files")

Write-Output("Starting IIS Process")

$host.SetShouldExit($script:exitCode)
exit $script:exitCode
