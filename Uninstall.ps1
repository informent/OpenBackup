param([string]$InstallRoot = "$env:LOCALAPPDATA\OpenBackup")
$ErrorActionPreference = 'Stop'
$shortcut = Join-Path ([Environment]::GetFolderPath('Desktop')) 'OpenBackup.lnk'
if (Test-Path -LiteralPath $shortcut) { Remove-Item -LiteralPath $shortcut -Force }
if (Test-Path -LiteralPath $InstallRoot) { Remove-Item -LiteralPath $InstallRoot -Recurse -Force }
Write-Output 'OpenBackup was removed. Existing backup snapshots were not touched.'
