<#
.SYNOPSIS
    Short description
.DESCRIPTION
    Long description
.EXAMPLE
    PS C:\> <example usage>
    Explanation of what the example does
.INPUTS
    Inputs (if any)
.OUTPUTS
    Output (if any)
.NOTES
    General notes
#>
[CmdletBinding()]
param (
    [Parameter(Position = 0)]
    [string]$Tags
)

New-PSDrive -Name Tag -PSProvider Tag -Root 'Tag:' -Scope Global

$Tags -replace '\\', '-' -replace '/', '\' -split ';' | Foreach-Object { New-Item "Tag:\$_" }