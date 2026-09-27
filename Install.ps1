param([string]$InstallRoot = "$env:LOCALAPPDATA\OpenBackup")
$ErrorActionPreference = 'Stop'
$source = Join-Path $PSScriptRoot 'OpenBackup.exe'
if (!(Test-Path -LiteralPath $source)) { throw 'OpenBackup.exe must be beside Install.ps1.' }
New-Item -ItemType Directory -Path $InstallRoot -Force | Out-Null
Copy-Item -LiteralPath $source -Destination (Join-Path $InstallRoot 'OpenBackup.exe') -Force
$shortcut = Join-Path ([Environment]::GetFolderPath('Desktop')) 'OpenBackup.lnk'
$shell = New-Object -ComObject WScript.Shell
$link = $shell.CreateShortcut($shortcut); $link.TargetPath = Join-Path $InstallRoot 'OpenBackup.exe'; $link.WorkingDirectory = $InstallRoot; $link.Description = 'OpenBackup local backup manager'; $link.Save()
Write-Output "Installed OpenBackup to $InstallRoot"
