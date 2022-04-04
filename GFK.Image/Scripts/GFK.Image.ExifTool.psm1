#Requires -PSEdition Core
#Requires -Module GFK.Image

Set-StrictMode -Version Latest

class ImageMetadata {
    [string] $FilePath
    [PSObject] $Tags
}

function Get-ImageMetadata {
    <#
    .SYNOPSIS
        Uses ExifTool for getting tags or shortcut tags by tag names
    .DESCRIPTION
        This command outputs metadata tags for one or multiple files
        The input path can be a folder and can contain wildcards    
    .EXAMPLE
        There are two modes (parameter sets) in which this command can run:

        PS C:\> Get-ImageMetadata -FilePath 'C:\Users\Gordon Freeman\Pictures\Black Mesa Research Center.jpg' -TagName Artist,CreateDate
        The command returns one string per tag (including those not found in the file, as empty strings) as an array, except if:
        - the path resolves to more than one file
        - one of the tags used is a shortcut tag
        Additionnally, tag values that are just a single caret '-' will be returned as empty strings
        Choose this if you want the fastest mode to use with fully defined single files and without shortcut tags
        (You could actually work with more than one file is you splice the resulting array. This would theoretically be faster than running the command multiple times)

        PS C:\> Get-ImageMetadata -FilePath 'C:\Users\Gordon Freeman\Pictures\Black Mesa Research Center.jpg' -TagName Artist,CreateDate -Full
        In full mode, the command returns one ImageMetadata object per file found (each contains the file path and a Tags property)
        Tags not found on the file will not be  in the output at all (even as empty strings)
        The Tags property of each ImageMetadata object contains the top level groups, each of which contains the properties for the tags found
        There will be no file or tag collisions in this mode
        Choose this if you want to handle multiple files and want an exhaustive report on the tags available on the file
    .NOTES
        - You can show the actual ExifTool command with the -Verbose switch
        - Recursion is available as a switch. The option will have no effect if the path is not a directory
        - An optional ExifTool configuration can be specified as a parameter

    #>
    [CmdletBinding(PositionalBinding = $false, DefaultParameterSetName = 'Basic')]
    [OutputType([string[]], ParameterSetName = 'Basic')]
    [OutputType([ImageMetadata[]], ParameterSetName = 'Full')]
    Param(
        [SupportsWildcards()]
        [Parameter(Mandatory, ValueFromPipeline)]
        [string] $FilePath,

        [string[]] $SourceFiles = @(),

        [Parameter(Mandatory)]
        [string[]] $TagNames,

        [Parameter(ParameterSetName = 'Full')]
        [switch] $Full,

        [switch] $Recurse,

        [string] $ConfigurationPath
    )

    Begin {
        Test-ExifTool
    }

    Process {
        if ($ConfigurationPath) {
            $arguments = @('-config', $ConfigurationPath)
        }
        else {
            $arguments = @()
        }
        
        $arguments += $SourceFiles | ForEach-Object { '-srcfile', $_ }

        $extension = Split-Path -Extension $FilePath
        if ($extension) {
            $arguments += '-ext', $extension
        }

        if ($Full) {
            $arguments += '-j', '-G', '-a'
        }
        else {
            $arguments += '-s3', '-f'
        }

        if ($Recurse) {
            $arguments += '-r'
        }
        
        $arguments = @($arguments; $TagNames | Get-TagNameArgument; '-c', '%+.6f', '--', $FilePath)
    
        Write-Verbose "exiftool $(($arguments | Foreach-Object { "'$($_ -replace "'","''")'" }) -join ' ')"
        $toolResults = &exiftool $arguments
        if ($Full) {
            return ConvertFrom-Json ($toolResults -join "`n") | New-ImageMetadata
        }
        else {
            return $toolResults | ConvertFrom-ImageValue
        }
    }
}

function Set-ImageMetadata() {
    <#
    .SYNOPSIS
        Uses ExifTool for setting tags or shortcut tags
    .DESCRIPTION
        This command writes metadata tags to one or multiple files
        The input path can be a folder and can contain wildcards    
        Arrays will be concatenated with ';'
    .EXAMPLE
        Set-ImageMetadata `
            -FilePath 'C:\Users\Gordon Freeman\Pictures\Black Mesa Research Center.jpg' `
            -Tags @{
                Artist = 'Gordon Freeman','Adrian Shephard';
                '-XMP-xmp:MetadataDate' = Get-Date
            }
    .NOTES
        - You can show the actual ExifTool command with the -Verbose switch, and show only the command without running
        it with -Verbose -WhatIf 
        - An optional ExifTool configuration can be specified as a parameter.
        - Shortcut tag values cannot be set with the `=` operator and need to be set with the `<` operator.
        In turn, the `<` operator does not play well with constant values when they contain special characters such as `$`.
        Therefore to support both constant and non-constant values on tags and on shortcut tags, we use the '-userParam' option.
    #>
    [CmdletBinding(SupportsShouldProcess, PositionalBinding = $false)]
    Param(
        [SupportsWildcards()]
        [Parameter(Mandatory, ValueFromPipeline)]
        [string]$FilePath,

        [string[]] $SourceFiles = @(),

        [Parameter(Mandatory)]
        [hashtable] $Tags,

        [switch] $Recurse,

        [string] $ConfigurationPath
    )

    Begin {
        Test-ExifTool
    }

    Process {
        if ($ConfigurationPath) {
            $arguments = @('-config', $ConfigurationPath)
        }
        else {
            $arguments = @()
        }

        $arguments += '-overwrite_original'
        $arguments += $SourceFiles | ForEach-Object { '-srcfile', $_ }

        $extension = Split-Path -Extension $FilePath
        if ($extension) {
            $arguments += '-ext', $extension
        }

        $index = 0
        foreach ($tagName in $Tags.Keys) {
            $tagNameArgument = Get-TagNameArgument -TagName $tagName
            $paramName = "Param$(($index++))"
            $tagValue = ConvertTo-ImageValue -TagValue $Tags[$tagName]
            $arguments += "$tagNameArgument<`$$paramName", '-userParam', "$paramName=`"$tagValue`""
        }
        $arguments += '--', $FilePath
    
        Write-Verbose "exiftool $(($arguments | Foreach-Object { "'$($_ -replace "'","''")'" }) -join ' ')"
        if ($PSCmdlet.ShouldProcess($FilePath)) {
            &exiftool $arguments
        }    
    }
}

function ConvertFrom-ImageDateTime {
    <#
    .SYNOPSIS
        Converts a metadata date/time or date+time into a [datetime] object
    .DESCRIPTION
        Relies on ExifTool's default formats for such fields. Supports EXIF (naive full date), IPTC (date + naive or local time), XMP (naive or local full date)
    .EXAMPLE
        ConvertFrom-ImageDateTime -Date '2022:01:19' -Time '15:16:17'
        ConvertFrom-ImageDateTime -DateTime '2022:01:19 15:16:17+03:00'
        ConvertFrom-ImageDateTime -DateTime (Get-ImageMetadata $filePath XMP:CreateDate)
    #>
    [CmdletBinding(PositionalBinding = $false)]
    [OutputType([datetime])]
    Param (
        [Parameter(Mandatory, ParameterSetName = 'OneField', ValueFromPipelineByPropertyName)]
        [string]$DateTime,

        [Parameter(Mandatory, ParameterSetName = 'TwoFields', ValueFromPipelineByPropertyName)]
        [string]$Date,

        [Parameter(Mandatory, ParameterSetName = 'TwoFields', ValueFromPipelineByPropertyName)]
        [string]$Time
    )

    Process {
        if ($PsCmdlet.ParameterSetName -EQ 'TwoFields') {
            $DateTime = "$Date $Time"
        }
        $formats = 'yyyy:MM:dd HH:mm:ss', 'yyyy:MM:dd HH:mm:sszzz'
        return [datetime]::ParseExact($DateTime, [string[]] $formats, [System.Globalization.CultureInfo]::InvariantCulture)    
    }
}

#region Private functions

function Test-ExifTool {
    if (-not (Get-Command exiftool)) {
        throw 'ExifTool not found in the environment Path'
    }
}

function Get-TagNameArgument {
    [CmdletBinding(PositionalBinding = $false)]
    [OutputType([string])]
    Param (
        [Parameter(Mandatory, ValueFromPipeline)]
        [string]$TagName
    )

    Process {
        if (-not ($TagName -match '^(?:[\w-]+:)?\w+$')) {
            throw "Expected 'TagName' or 'Namespace:TagName' but found '$TagName'"
        }
        return "-$TagName"    
    }
}

function ConvertFrom-ImageValue {
    [CmdletBinding(PositionalBinding = $false)]
    Param (
        [Parameter(Mandatory, ValueFromPipeline)]
        [string]$TagValue
    )

    Process {
        if ($TagValue -eq '-') {
            return ''
        }
        return $TagValue
    }
}

function ConvertTo-ImageValue {
    [CmdletBinding(PositionalBinding = $false)]
    Param (
        [Parameter(Mandatory, ValueFromPipeline)]
        [object]$TagValue
    )

    Process {
        if ($TagValue -is [datetime] -or $TagValue -is [System.DateTimeOffset]) {
            return '{0:yyyy-MM-ddTHH:mm:sszzz}' -f $TagValue
        }
    
        if ($TagValue -is [array]) {
            return $TagValue -join ';'
        }
    
        return [string] $TagValue    
    }
}

function New-ImageMetadata {
    [CmdletBinding(PositionalBinding = $false)]
    Param (
        [Parameter(Mandatory, ValueFromPipeline)]
        [PSObject] $ExifToolResult
    )

    Process {
        $tagGroups = @{}
        foreach ($member in Get-Member -InputObject $ExifToolResult -MemberType NoteProperty) {
            if ($member.Name -EQ 'SourceFile') {
                continue
            }

            $groupName, $tagName = $member.Name -split ':'
            $tagValue = Select-Object -InputObject $ExifToolResult -ExpandProperty $member.Name

            if (-not $tagGroups[$groupName]) {
                $tagGroups[$groupName] = @{}
            }
            $tagGroups[$groupName][$tagName] = $tagValue
        }

        $tags = @{}
        foreach ($groupName in $tagGroups.Keys) {
            $tags[$groupName] = [PSCustomObject] ($tagGroups[$groupName])
        }

        return [ImageMetadata] @{
            FilePath = Convert-Path $ExifToolResult.SourceFile;
            Tags     = [PSCustomObject] $tags
        }
    }
}

#endregion