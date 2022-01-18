#Requires -Module GFK.Image.PowerShell

<#
.SYNOPSIS
    Creates a Tags:/ drive from digiKam tags
.DESCRIPTION
    Hierarchized tags passed by digiKam are in a single string where ';' is the tag separator and '/' is the path separator
    This will load these tags in a new PowerShell drive called Tag:/ to enable exploring them
#>
[CmdletBinding]
param ([Parameter(Mandatory)][string]$Tags)

New-PSDrive -Name Tags -PSProvider Tags -Root 'Tags:' -Scope Global

$Tags -replace '\\', '-' -replace '/', '\' -split ';' | Foreach-Object { New-Item "Tags:\$_" }