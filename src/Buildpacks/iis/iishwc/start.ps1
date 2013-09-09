$script:scriptPath = split-path -parent $MyInvocation.MyCommand.Definition
$script:appPoolName = [Guid]::NewGuid().ToString()
$script:appPort = $Env:PORT
$script:appPath = (get-item $script:scriptPath).parent.FullName
$script:exitCode = 0

function DetectBitness()
{
	$assemblies = Get-ChildItem -Path $script:appPath -Filter "*.dll" -Recurse	
	foreach ($assembly in $assemblies)
	{
		$kind = new-object Reflection.PortableExecutableKinds
        $machine = new-object Reflection.ImageFileMachine
        try
        {
            $a = [Reflection.Assembly]::ReflectionOnlyLoadFrom($assembly.Fullname)
            $a.ManifestModule.GetPEKind([ref]$kind, [ref]$machine)
        }
        catch [System.BadImageFormatException]
        {
            $kind = [System.Reflection.PortableExecutableKinds]"NotAPortableExecutableImage"
        }
		
		switch ($kind)
		{
			[System.Reflection.PortableExecutableKinds]::Required32Bit
			{
				return $true;
			}			
			([System.Reflection.PortableExecutableKinds]([System.Reflection.PortableExecutableKinds]::Required32Bit -bor [System.Reflection.PortableExecutableKinds]::ILOnly))
			{
				return $true;
			}
			default { }
		}			  		
	}
	
	return $false
}

function GetFrameworkFromConfig()
{
	$webConfigPath = Join-Path $script:appPath 'web.config'
	$webConfig = New-Object System.Xml.XmlDocument
	$webConfig.Load($webConfigPath)

	$node = $webConfig.SelectSingleNode("/configuration/system.web/compilation/@targetFramework")
	if ($node)
	{
		return "v4.0"
	}
	else
	{
		return "v2.0"
	}
}

function AddApplicationPool([ref]$applicationHost)
{
	$enable32bit = DetectBitness
	if ($enable32bit -eq $true)
	{
		$script:exitCode = 1
	}
	$element = $applicationHost.Value.CreateElement("add")
	$element.SetAttribute('name', $script:appPoolName)
	$element.SetAttribute('enable32BitAppOnWin64', $enable32bit)
	$framework = GetFrameworkFromConfig
	$element.SetAttribute('managedRuntimeVersion', $framework)
	$applicationHost.Value.configuration."system.applicationHost".applicationPools.AppendChild($element)
}

function AddSite([ref]$applicationHost, $appName)
{
	$appName = [Guid]::NewGuid().ToString()
	$element = $applicationHost.Value.CreateElement("site")
	$element.SetAttribute('name', $appName)
	$element.SetAttribute('id', 1)
	$applicationHost.Value.configuration."system.applicationHost".sites.AppendChild($element)
}

function AddBinding([ref]$applicationHost)
{
	$bindings = $applicationHost.Value.CreateElement("bindings")	
	$element = $applicationHost.Value.CreateElement("binding")
	$element.SetAttribute('protocol', "http")
	$element.SetAttribute("bindingInformation", [String]::Format("*:{0}:", $script:appPort ))	
	$bindings.AppendChild($element)
	$applicationHost.Value.configuration."system.applicationHost".sites.site.AppendChild($bindings)	
}

function AddApplication([ref]$applicationHost)
{
	$application = $applicationHost.Value.CreateElement("application")
	$application.SetAttribute('path', '/')
	$application.SetAttribute('applicationPool', $script:appPoolName)
	$virtualDirectory = $applicationHost.Value.CreateElement("virtualDirectory")
	$virtualDirectory.SetAttribute('path', '/')
	$virtualDirectory.SetAttribute('physicalPath', $script:appPath)
	$application.AppendChild($virtualDirectory)
	$applicationHost.Value.configuration."system.applicationHost".sites.site.AppendChild($application)	
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

$vcap_services = $Env:VCAP_SERVICES | ConvertFrom-Json
$configFiles=get-childitem $script:appPath *.config -rec
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

exit $script:exitCode