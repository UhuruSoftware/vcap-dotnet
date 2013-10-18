$path = $args[0]

$files = @(Get-ChildItem $path -Name)
IF ($files -contains "web.config")
{
  Write-Output "IIS8/.NET"
  exit 0
}
ELSE
{
  exit 1
}