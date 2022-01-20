#Requires -PSEdition Core
#Requires -Module GFK.Image

<#
.SYNOPSIS
    Sets tags on an image file
.DESCRIPTION
    Uses ExifTool for setting tags or shortcut tags. Tags and their values are dynamically parsed.
    Arrays will be concatenated with ';'.
.EXAMPLE
    Set-ImageMetadata -FilePath 'C:\Users\Gordon Freeman\Pictures\Black Mesa Research Center.jpg' -Artist 'Gordon Freeman','Adrian Shephard' '-XMP-xmp:MetadataDate' (Get-Date)
.NOTES
    Use the -WhatIf switch to display the exiftool command instead of running it
#>
[CmdletBinding]
function Set-ImageMetadata([string]$FilePath, [switch]$WhatIf)
{
    if (-not (Get-Command exiftool)) {
        throw 'ExifTool not found in the Path'
    }
    
    $arguments = @('-overwrite_original')
    $tagNameArg = $null
    $tagName
    foreach ($arg in $Args)
    {
        if ($tagNameArg) {
            $arguments += "$tagNameArg<`$${tagName}Param",'-userParam',"$($tagName)Param=`"$(Get-TagValue $arg)`""
            $tagNameArg = $null
            $tagName = $null
        } else {
            $tagNameArg = $arg
            $tagName = Get-TagName $arg
        }
    }
    if ($tagNameArg) {
        throw "Missing value for tag $tagNameArg"
    }
    $arguments += '--',$FilePath
    
    if ($WhatIf) {
        Write-Host "What if: exiftool $($arguments -join ' ')" 
    } else {
        &exiftool $arguments
    }
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

# region Private functions

function Get-TagName
{
    param (
        [Parameter(Mandatory)][string]$Name
    )

    if (-not ($Name -match '^-(?:[\w-]+:)?(?<TagName>\w+)$')) {
        throw "Expected '-TagName' or '-Namespace:TagName' but found '$Name'"
    }
    return $Matches.TagName
}

function Get-TagValue
{
    param (
        [Parameter(Mandatory)][object]$Value
    )

    if ($Value -is [datetime] -or $Value -is [System.DateTimeOffset]) {
        return '{0:yyyy-MM-ddTHH:mm:sszzz}' -f $Value
    }

    if ($Value -is [array]) {
        return $Value -join ';'
    }

    return [string] $Value
}

#endregion