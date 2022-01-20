#Requires -Module GFK.Image

<#
.SYNOPSIS
    Injects PowerShell into digiKam
.DESCRIPTION
.NOTE
    digiKam is expected to be installed for this function to work
#>

[CmdletBinding]
function Install-PSDigiKam {
    Write-Output $PSScriptRoot
    Write-Output (Get-Location).Path
    
    if ($env:OS -ne 'Windows_NT' -or $env:PROCESSOR_ARCHITECTURE -ne 'AMD64') {
        throw 'This function is only compatible with win64 systems'
    }

    $identity = [Security.Principal.WindowsIdentity]::GetCurrent()
    $principal = New-Object Security.Principal.WindowsPrincipal $identity
    if (-not $principal.IsInRole([Security.Principal.WindowsBuiltinRole]::Administrator)) {
        throw 'This function must run in an elevated shell'
    }

    $digiKamRegistryKey = Get-Item HKLM:\SOFTWARE\Wow6432Node\Microsoft\Windows\CurrentVersion\Uninstall\digiKam -ErrorAction SilentlyContinue
    if (-not $digiKamRegistryKey) {
        throw 'digiKam does not appear to be installed'
    }

    $digiKamInstallLocation = $digiKamRegistryKey.GetValue('InstallLocation')
    Copy-Item 'cmd/cmd.exe' $digiKamInstallLocation
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
