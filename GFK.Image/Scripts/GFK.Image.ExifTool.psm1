#Requires -Module GFK.Image

<#
.SYNOPSIS
    Sets Authors tags on a photo file
.DESCRIPTION
    Concatenates the values and stores it in the Authors ExifTool shortcut tag for the file
.EXAMPLE
    PS C:\> Set-Author.p1 C:\photos\photo1.jpg -Authors 'Alan Turing','Charlie Chaplin'
#>
[CmdletBinding]
function Set-Authors
{
    param (
        [Parameter(Mandatory)][string]$FilePath,
        [Parameter(Mandatory)][string[]]$Authors
    )

    exiftool -overwrite_original $FilePath -Modified<now '-Authors<$AuthorsParam' -userParam AuthorsParam="`"$($Authors -join ';')`""
}

<#
.SYNOPSIS
    Sets UTC offset tags on a photo file
.DESCRIPTION
    Uses GeoTimeZone and TimeZoneInfo to find the UTC offset at the location and time tags of the file
    Stores the full date in the Taken and Digitized ExifTool shortcut tags
#>
[CmdletBinding]
function Set-DateTimeOffsets
{
    param ([Parameter(Mandatory)][string]$FilePath)

    # Note: Only the XMP GPS coordinates are signed, therefore using the 4-field system as used in the EXIF metadata would require further work.
    # Note: In film photography DateTimeOriginal is meant to be the date the picture was taken, and CreateDate is meant to be the date the picture was digitized.
    # In digital photography, they are expected to be the same. If you only do digital, this script will still work but could be simplified to only require DateTimeOriginal.
    $latitude, $longitude, $taken, $digitized = exiftool $FilePath -XMP-exif:GPSLatitude -XMP-exif:GPSLongitude -EXIF:DateTimeOriginal -EXIF:CreateDate -s -s -s -c '%+.6f' -d '%Y-%m-%dT%H:%M:%S'

    $takenLocal = '{0:yyyy-MM-ddTHH:mm:sszzz}' -f  (Get-DateTimeOffset $taken $latitude $longitude)
    # Note: If you do film photography the location where the pictures are digitized might be different from the location they are taken.
    # If you digitize all your pictures in the same time zone you might want to replace these parameters by constant coordinates situated in that time zone.
    $digitizedLocal = '{0:yyyy-MM-ddTHH:mm:sszzz}' -f  (Get-DateTimeOffset $digitized $latitude $longitude)

    exiftool -overwrite_original $FilePath -Modified<now '-Taken<$TakenParam' '-Digitized<$DigitizedParam' -userParam TakenParam="$takenLocal" -userParam DigitizedParam="$digitizedLocal"
}