#Requires -PSEdition Core
#Requires -Module GFK.Image

<#
.SYNOPSIS
    Gets tags from an image file
.DESCRIPTION
    Uses ExifTool for getting tags or shortcut tags.
.EXAMPLE
    Get-ImageMetadata -FilePath 'C:\Users\Gordon Freeman\Pictures\Black Mesa Research Center.jpg' -Artist '-XMP-xmp:MetadataDate'
#>
[CmdletBinding]
function Get-ImageMetadata {
    [OutputType([string[]])]
    param(
        [string]$FilePath
    )
    
    if (-not (Get-Command exiftool)) {
        throw 'ExifTool not found in the Path'
    }
    
    $Args | Get-TagName | Out-Null # test tag name regex
    $arguments = $Args + '-s', '-s', '-s', '-c', '%+.6f', '--', $FilePath
    
    return &exiftool $arguments
}

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
function Set-ImageMetadata([string]$FilePath, [switch]$WhatIf) {
    if (-not (Get-Command exiftool)) {
        throw 'ExifTool not found in the Path'
    }

    $arguments = @('-overwrite_original')
    $tagNameArg = $null
    $tagName
    foreach ($arg in $Args) {
        if ($tagNameArg) {
            $arguments += "$tagNameArg<`$${tagName}Param", '-userParam', "$($tagName)Param=`"$(Get-TagValue $arg)`""
            $tagNameArg = $null
            $tagName = $null
        }
        else {
            $tagNameArg = $arg
            $tagName = Get-TagName $arg
        }
    }
    if ($tagNameArg) {
        throw "Missing value for tag $tagNameArg"
    }
    $arguments += '--', $FilePath

    if ($WhatIf) {
        Write-Host "What if: exiftool $($arguments -join ' ')"
    }
    else {
        &exiftool $arguments
    }
}

<#
.SYNOPSIS
    Converts a metadata date/time or date+time into a [datetime] object
.DESCRIPTION
    Relies on ExifTool's default formats for such fields. Supports EXIF (naive full date), IPTC (date + naive or local time), XMP (naive or local full date)
.EXAMPLE
    Convert-ImageDateTime -Date '2022:01:19' -Time '15:16:17'
    Convert-ImageDateTime -DateTime '2022:01:19 15:16:17+03:00'
    Convert-ImageDateTime -DateTime (exiftool $filePath -XMP:CreateDate -s -s -s)
#>
[CmdletBinding]
function Convert-ImageDateTime {
    [OutputType([datetime])]
    param (
        [Parameter(Mandatory, ParameterSetName = 'OneField')][string]$DateTime,
        [Parameter(Mandatory, ParameterSetName = 'TwoFields')][string]$Date,
        [Parameter(Mandatory, ParameterSetName = 'TwoFields')][string]$Time
    )

    if ($PsCmdlet.ParameterSetName -EQ 'TwoFields') {
        $DateTime = "$Date $Time"
    }
    $formats = 'yyyy:MM:dd HH:mm:ss','yyyy:MM:dd HH:mm:sszzz'
    return [datetime]::ParseExact($DateTime, [string[]] $formats, [System.Globalization.CultureInfo]::InvariantCulture)
}

#region Private functions

[CmdletBinding]
function Get-TagName {
    param (
        [Parameter(Mandatory, ValueFromPipeline)][string]$Name
    )

    if (-not ($Name -match '^-(?:[\w-]+:)?(?<TagName>\w+)$')) {
        throw "Expected '-TagName' or '-Namespace:TagName' but found '$Name'"
    }
    return $Matches.TagName
}

function Get-TagValue {
    param (
        [Parameter(Mandatory, ValueFromPipeline)][object]$Value
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