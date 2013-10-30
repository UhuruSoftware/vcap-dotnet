write-output "Started Compilation Script";
$build_path = $args[0]
$cache_path = $args[1]

$scriptPath = split-path -parent $MyInvocation.MyCommand.Definition
$iisPath = Join-Path (get-item $scriptPath ).parent.FullName 'iishwc\*'

$iishwcPath = Join-Path $build_path "iishwc"
$null = mkdir $iishwcPath
write-output "Copying IIS Executable to App directory"

$bpAppPath = Join-Path (get-item $scriptPath ).parent.FullName 'app\*'
$null = Copy-Item $bpAppPath $build_path -Recurse -Force
$null = Copy-Item $iisPath $iishwcPath -Recurse -Force
write-output "Done"

exit 0
