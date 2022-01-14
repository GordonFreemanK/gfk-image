# Script for digiKam with fake cmd
# Run the following command in an admin console before running this script
# Install-Package -Name GeoTimeZone -ProviderName NuGet -SkipDependencies -Confirm

[CmdletBinding(PositionalBinding = $true)]
param (
    [Parameter(Mandatory, Position=0)]
    [string]
    $FilePath
)

$ErrorActionPreference = 'Stop'
$PSStyle.OutputRendering = [System.Management.Automation.OutputRendering]::PlainText

$geoTimeZonePackage = Get-Package -Name GeoTimeZone
$geoTimeZonePackagePath = Split-Path $geoTimeZonePackage.Source -Parent
$geoTimeZoneDllPath = Join-Path -Path $geoTimeZonePackagePath lib netstandard2.0 GeoTimeZone.dll
Add-Type -Path $geoTimeZoneDllPath

function Get-LocalDate([string]$Date, [string]$Latitude, [string]$Longitude) {
    $dateTime = [datetime]::Parse($Date)

    $timeZoneResult = [GeoTimeZone.TimeZoneLookup]::GetTimeZone($Latitude, $Longitude)
    $timeZoneInfo = [System.TimeZoneInfo]::FindSystemTimeZoneById($timeZoneResult.Result)
    $offset = $timeZoneInfo.GetUtcOffset($dateTime)
    
    $dateTimeOffset = New-Object DateTimeOffset $dateTime, $offset
    return $dateTimeOffset.ToString('yyyy-MM-ddTHH:mm:sszzz')
}

$latitude, $longitude, $taken, $digitized = exiftool $FilePath -XMP:GPSLatitude -XMP:GPSLongitude -EXIF:DateTimeOriginal -EXIF:CreateDate -s -s -s -c '%+.6f' -d '%Y-%m-%dT%H:%M:%S'

$takenLocal = Get-LocalDate -Date $taken -Latitude $latitude -Longitude $longitude
$digitizedLocal = Get-LocalDate -Date $digitized -Latitude $latitude -Longitude $longitude

exiftool -overwrite_original $FilePath -Modified<now '-Taken<$TakenParam' '-Digitized<$DigitizedParam' -userParam TakenParam="$takenLocal" -userParam DigitizedParam="$digitizedLocal"