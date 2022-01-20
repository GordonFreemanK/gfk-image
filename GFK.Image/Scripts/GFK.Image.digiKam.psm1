#Requires -PSEdition Core
#Requires -Module GFK.Image

<#
.SYNOPSIS
    Add the PowerShell injection to digiKam
.NOTES
    digiKam is expected to be installed for this command to work
#>
[CmdletBinding]
function Install-PSDigiKam {
    Test-IsAdministrator

    $sourcePath = Join-Path (Split-Path $PSScriptRoot) cmd cmd.exe
    Copy-Item $sourcePath (Get-DigiKamPath)
}

<#
.SYNOPSIS
    Removes the PowerShell injection from digiKam
.NOTES
    digiKam is expected to be installed for this command to work
#>
[CmdletBinding]
function Uninstall-PSDigiKam {
    Test-IsAdministrator

    $path = Join-Path (Get-DigiKamPath) cmd.exe
    if (Test-Path $path) {
        Remove-Item $path
    }
}

<#
.SYNOPSIS
    Creates a Tags:/ drive from digiKam tags
.DESCRIPTION
    Hierarchized tags passed by digiKam are in a single string where ';' is the tag separator and '/' is the path separator
    This will load these tags in a new PowerShell drive called Tag:/ to enable exploring them
#>
[CmdletBinding]
function New-TagsDrive {
    param ([Parameter(Mandatory)][string]$Tags)

    New-PSDrive -Name Tags -PSProvider Tags -Root 'Tags:' -Scope Global | Out-Null

    $Tags -replace '\\', '-' -replace '/', '\' -split ';' | Foreach-Object { New-Item "Tags:\$_" | Out-Null }
}

#region Private functions

function Test-IsAdministrator {
    $identity = [Security.Principal.WindowsIdentity]::GetCurrent()
    $principal = New-Object Security.Principal.WindowsPrincipal $identity
    if (-not $principal.IsInRole([Security.Principal.WindowsBuiltinRole]::Administrator)) {
        throw 'This command must run in an elevated shell'
    }
}

function Get-DigiKamPath {
    if ($env:OS -ne 'Windows_NT' -or $env:PROCESSOR_ARCHITECTURE -ne 'AMD64') {
        throw 'This command is only compatible with win64 systems'
    }

    $digiKamRegistryKey = Get-Item HKLM:\SOFTWARE\Wow6432Node\Microsoft\Windows\CurrentVersion\Uninstall\digiKam -ErrorAction SilentlyContinue
    if (-not$digiKamRegistryKey) {
        throw 'digiKam does not appear to be installed'
    }

    return $digiKamRegistryKey.GetValue('InstallLocation')
}

#endregion