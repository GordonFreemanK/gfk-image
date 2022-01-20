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
    
    $Args | Get-TagName # test tag name regex
    $arguments = $Args + '-s', '-s', '-s', '-c', '%+.6f', '-d', '%Y-%m-%dT%H:%M:%S', '--', $FilePath
    
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

#region Private functions

function Get-TagName {
    param (
        [Parameter(Mandatory)][string]$Name
    )

    if (-not ($Name -match '^-(?:[\w-]+:)?(?<TagName>\w+)$')) {
        throw "Expected '-TagName' or '-Namespace:TagName' but found '$Name'"
    }
    return $Matches.TagName
}

function Get-TagValue {
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