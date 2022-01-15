# Script for digiKam with fake cmd
# Run the following command in an admin console before running this script
# Install-Package -Name GeoTimeZone -ProviderName NuGet -SkipDependencies -Confirm

[CmdletBinding(PositionalBinding = $true)]
param (
    [Parameter(Mandatory, Position=0)]
    [string]
    $FilePath
)

$geoTimeZonePackage = Get-Package -Name GeoTimeZone
$geoTimeZonePackagePath = Split-Path $geoTimeZonePackage.Source -Parent
$geoTimeZoneDllPath = Join-Path -Path $geoTimeZonePackagePath lib netstandard2.0 GeoTimeZone.dll
Add-Type -Path $geoTimeZoneDllPath

function Get-LocalDate([string]$Date, [string]$Latitude, [string]$Longitude) {
    # Assume the date is local
    $dateTime = [datetime]::Parse($Date)

    $timeZoneResult = [GeoTimeZone.TimeZoneLookup]::GetTimeZone($Latitude, $Longitude)
    $timeZoneInfo = [System.TimeZoneInfo]::FindSystemTimeZoneById($timeZoneResult.Result)
    $offset = $timeZoneInfo.GetUtcOffset($dateTime)
    
    $dateTimeOffset = New-Object DateTimeOffset $dateTime, $offset
    return $dateTimeOffset.ToString('yyyy-MM-ddTHH:mm:sszzz')
}

# Note: Only the XMP GPS coordinates are signed, therefore using the 4-field system as used in the EXIF metadata would require further work.
# Note: In film photography DateTimeOriginal is meant to be the date the picture was taken, and CreateDate is meant to be the date the picture was digitized.
# In digital photography, they are expected to be the same. If you only do digital, this script will still work but could be simplified to only require DateTimeOriginal.
$latitude, $longitude, $taken, $digitized = exiftool $FilePath -XMP-exif:GPSLatitude -XMP-exif:GPSLongitude -EXIF:DateTimeOriginal -EXIF:CreateDate -s -s -s -c '%+.6f' -d '%Y-%m-%dT%H:%M:%S'

$takenLocal = Get-LocalDate -Date $taken -Latitude $latitude -Longitude $longitude
# Note: If you do film photography the location where the pictures are digitized might be different from the location they are taken.
# If you digitize all your pictures in the same time zone you might want to replace these variables by constant coordinates situated in that time zone.
$digitizedLocal = Get-LocalDate -Date $digitized -Latitude $latitude -Longitude $longitude

exiftool -overwrite_original $FilePath -Modified<now '-Taken<$TakenParam' '-Digitized<$DigitizedParam' -userParam TakenParam="$takenLocal" -userParam DigitizedParam="$digitizedLocal"