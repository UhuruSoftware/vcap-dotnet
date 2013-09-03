$build_path = $args[0]
$cache_path = $args[1]

$scriptPath = split-path -parent $MyInvocation.MyCommand.Definition
$iisPath = Join-Path (get-item $scriptPath ).parent.FullName 'iishwc\*'

$iishwcPath = Join-Path $build_path "iishwc"
mkdir $iishwcPath
Copy-Item $iisPath $iishwcPath -Recurse -Force

exit 0