# Script for digiKam with fake cmd
# Run the following command in an admin console before running this script
# Install-Package -Name GeoTimeZone -ProviderName NuGet -SkipDependencies -Confirm

[CmdletBinding(PositionalBinding = $true)]
param (
    [Parameter(Mandatory, Position=0)]
    [string]
    $FilePath,

    [Parameter(Mandatory)]
    [string]
    $Author
)

$ErrorActionPreference = 'Stop'
$PSStyle.OutputRendering = [System.Management.Automation.OutputRendering]::PlainText

exiftool -overwrite_original $FilePath -Modified<now '-Author<$AuthorParam' -userParam AuthorParam="$Author"