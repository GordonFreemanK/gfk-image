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
    .PARAMETER FilePaths
        One or multiple strings representing paths to image files. Can be full file names, directories, and can contain wildcards
        If one or more paths contain explicit extensions, then only files with those extensions will be considered
        e.g. '.' will read from all files in the current directory, but '*.mpg','.' will only read from .mpg files
        It is possible to specify multiple extensions that way e.g. '*.jpg','*.mpg'
    .PARAMETER SourceFiles
        This parameter enables including sidecars, which are useful to manage metadata files that cannot be written (e.g. RAW, movies)
        The syntax is defined by ExifTool (https://www.exiftool.org/metafiles.html)
        If you include at least a value, it is recommended to include '@' or only the sidecar will be read
        If a sidecar does not exist it is ignored
        Order is important as for each image file, tags will be read in the first source file that exist
    .PARAMETER TagNames
        List of tags as defined in ExifTool (https://www.exiftool.org/TagNames/index.html) or custom tags as defined in the configuration
        Do not prefix with '-'!
    .PARAMETER Simple
        Optional, sets the mode to simple (see examples for more details)
    .PARAMETER Full
        Optional, sets the mode to full (see examples for more details)
    .PARAMETER Groups
        This is an advanced parameter for controlling how metadata groups should be named in the full mode
        These values will be concatenated and passed to the ExifTool -g parameter
        See https://www.exiftool.org/exiftool_pod.html#Input-output-text-formatting for the -g parameter
        See https://www.exiftool.org/ExifTool.html#GetGroup for groups definitions
        Example: if the requested tag is XMP:CreateDate
        - 0 (default) will result in the group name being 'XMP'
        - 1 will result in the group name being 'XMP-xmp'
        - 0,1 will result in the group name being 'XMP:XMP-xmp'
        This will influence how deduplication work for tag results
        For instance if a file contains a value for EXIF:CreateDate, XMP-xmp:CreateDate and XMP-exif:CreateDate
        there would be two results for @(0) and 3 results for @(1) or @(0,1)
        Note: the default is empty, which is equivalent to @(0)
    .PARAMETER Recurse
    Recurse in subdirectories. The option will have no effect if the path is not a directory
    .PARAMETER ConfigurationPath
    Optional configuration path. See example here: https://www.exiftool.org/config.html
    .EXAMPLE
    There are two modes (parameter sets) in which this command can run:

    PS C:\> Get-ImageMetadata -FilePath 'C:\Users\Gordon Freeman\Pictures\Black Mesa Research Center.jpg' -TagName Artist, CreateDate
    The command returns one string per tag per file as an array except if one of the tags used resolves to multiple tags
    Choose this if you want the fastest mode to use with fully defined single files and without shortcut tags
    Note: when using this for multiple files or multiple tags, since the tag names are not in the output, it is recommended to fully
    qualify tag names and not use shortcut tags as if multiple matches occur it will not be obvious which tags they belong to
    For instance CreateDate will return two rows if both EXIF:CreateDate and XMP:CreateDate are set, therefore it is preferable to specify
    EXIF:CreateDate

    PS C:\> Get-ImageMetadata -FilePath 'C:\Users\Gordon Freeman\Pictures\Black Mesa Research Center.jpg' -TagName Artist, CreateDate -Full
    In full mode, the command returns one ImageMetadata object per file found (each contains the file path and a Tags property)
    The Tags property of each ImageMetadata object contains the top level groups, each of which contains the properties for the tags
    There will be no file or tag collisions in this mode
    Choose this if you want to handle multiple files and want an exhaustive report on the tags available on the file
    .NOTES
    - You can show the actual ExifTool command with the -Verbose switch
    #>
    [CmdletBinding(PositionalBinding = $false, DefaultParameterSetName = 'Simple')]
    [OutputType([string[]], ParameterSetName = 'Simple')]
    [OutputType([ImageMetadata[]], ParameterSetName = 'Full')]
    Param(
        [SupportsWildcards()]
        [Parameter(Mandatory, ValueFromPipeline)]
        [string[]] $FilePaths,

        [string[]] $SourceFiles = @(),

        [Parameter(Mandatory)]
        [string[]] $TagNames,

        [Parameter(ParameterSetName = 'Simple')]
        [switch] $Simple,

        [Parameter(ParameterSetName = 'Full')]
        [switch] $Full,

        [Parameter(ParameterSetName = 'Full')]
        [uint[]] $Groups,

        [switch] $Recurse,

        [string] $ConfigurationPath
    )

    Begin {
        Test-ExifTool
    }

    Process {
        if ($ConfigurationPath) {
            # Add configuration file
            $arguments = @('-config', $ConfigurationPath)
        }
        else {
            $arguments = @()
        }
        
        # Add source files
        $arguments += $SourceFiles | ForEach-Object { '-srcfile', $_ }

        # If one or multiple paths contain extensions, explicitly set extensions
        $arguments += $FilePaths |
        Split-Path -Extension |
        Where-Object { $_ } |
        Select-Object -Unique |
        ForEach-Object { '-ext', $_ }

        if ($Recurse) {
            # Add recursion
            $arguments += '-r'
        }
        
        # Add tags
        $arguments += $TagNames | Get-TagNameArgument

        if ($PsCmdlet.ParameterSetName -eq 'Full') {
            $arguments += "-g$($Groups -join ':')"
        }
        else {
            $arguments += '-G'
        }

        # Extract all tags including composite and unknown, format results by file and group as json with signed GPS coordinates
        $arguments += '-a', '-f', '-u', '-j', '-c', '%+.6f'

        # Ignore minor errors, terminate argument list and add file paths
        $arguments += @('-m', '--'; $FilePaths)
    
        # -Verbose support
        Write-Verbose "exiftool $(($arguments | Foreach-Object { "'$($_ -replace "'","''")'" }) -join ' ')"

        # Read JSON
        $toolResults = (&exiftool $arguments) -join "`n" | ConvertFrom-Json
        if ($PsCmdlet.ParameterSetName -eq 'Full') {
            # Build structured results
            return $toolResults | ConvertTo-ImageMetadata
        }
        else {
            # Show values only
            return $toolResults | ConvertTo-ValuesOnly
        }
    }
}

function Set-ImageMetadata() {
    <#
    .SYNOPSIS
        Uses ExifTool for setting tags or shortcut tags
    .DESCRIPTION
        This command writes metadata tags to one or multiple files
    .PARAMETER FilePaths
        One or multiple strings representing paths to image files. Can be full file names, directories, and can contain wildcards.
        If one or more paths contain explicit extensions, then only files with those extensions will be considered
        e.g. '.' will write to all files in the current directory, but '*.mpg','.' will only write to .mpg files
        It is possible to specify multiple extensions that way e.g. '*.jpg','*.mpg'
    .PARAMETER SourceFiles
        This parameter enables including sidecars, which are useful to manage metadata files that cannot be written (e.g. RAW, movies)
        The syntax is defined by ExifTool (https://www.exiftool.org/metafiles.html)
        If you include at least a value, it is recommended to include '@' or only the sidecar will be read
        If a sidecar does not exist it is ignored
        Order is important as for each image file, tags will be read in the first source file that exist
    .PARAMETER Tags
        Hashtable where the keys are expected to be tag names as defined in ExifTool (https://www.exiftool.org/TagNames/index.html),
        or custom tags as defined in the configuration, and the value associated to each key is the value to be set on the tag
        Do not prefix tag names with '-'!
        Tag values that are arrays will be concatenated with ';'
        Note that the values can be the name of other tags (for copying tags) and contain advanced formatting as defined by ExifTool
        This can even include Perl commands (https://www.exiftool.org/exiftool_pod.html#Advanced-formatting-feature)
    .PARAMETER Recurse
        Recurse in subdirectories. The option will have no effect if the path is not a directory
    .PARAMETER ConfigurationPath
        Optional configuration path. See example here: https://www.exiftool.org/config.html
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
    #>
    [CmdletBinding(SupportsShouldProcess, PositionalBinding = $false)]
    Param(
        [SupportsWildcards()]
        [Parameter(Mandatory, ValueFromPipeline)]
        [string[]]$FilePaths,

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
            # Add configuration file
            $arguments = @('-config', $ConfigurationPath)
        }
        else {
            $arguments = @()
        }

        # Default to making changes in place
        $arguments += '-overwrite_original'

        # Add source files
        $arguments += $SourceFiles | ForEach-Object { '-srcfile', $_ }

        # If one or multiple paths contain extensions, explicitly set extensions
        $arguments += $FilePaths |
        Split-Path -Extension |
        Where-Object { $_ } |
        Select-Object -Unique |
        ForEach-Object { '-ext', $_ }

        # Define tag assignements
        $index = 0
        foreach ($tagName in $Tags.Keys) {
            # Shortcut tag values cannot be set with the `=` operator and need to be set with the `<` operator.
            # In turn, the `<` operator does not play well with constant values when they contain special characters such as `$`.
            # Therefore to support both constant and non-constant values on tags and on shortcut tags, we use the '-userParam' option.
            $tagNameArgument = Get-TagNameArgument -TagName $tagName
            $paramName = "Param$(($index++))"
            $tagValue = ConvertTo-TagValue -Value $Tags[$tagName]
            $arguments += "$tagNameArgument<`$$paramName", '-userParam', "$paramName=`"$tagValue`""
        }

        # Ignore minor errors, terminate argument list and add file paths
        $arguments += @('-m', '--'; $FilePaths)
    
        # -Verbose support
        Write-Verbose "exiftool $(($arguments | Foreach-Object { "'$($_ -replace "'","''")'" }) -join ' ')"

        # -WhatIf support
        if ($PSCmdlet.ShouldProcess($FilePaths)) {
            &exiftool $arguments
        }    
    }
}

function ConvertFrom-TagDateTime {
    <#
    .SYNOPSIS
        Converts a metadata date/time or date+time into a [datetime] object
    .DESCRIPTION
        Relies on ExifTool's default formats for such fields. Supports EXIF (naive full date), IPTC (date + naive or local time), XMP (naive or local full date)
    .EXAMPLE
        ConvertFrom-TagDateTime -Date '2022:01:19' -Time '15:16:17'
        ConvertFrom-TagDateTime -DateTime '2022:01:19 15:16:17+03:00'
        ConvertFrom-TagDateTime -DateTime (Get-ImageMetadata $filePath XMP:CreateDate)
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

function ConvertFrom-TagValue {
    [CmdletBinding(PositionalBinding = $false)]
    Param (
        [Parameter(ValueFromPipeline)]
        [string]$TagValue
    )

    Process {
        if ($TagValue -eq '-') {
            return ''
        }
        return $TagValue
    }
}

function ConvertTo-TagValue {
    [CmdletBinding(PositionalBinding = $false)]
    Param (
        [Parameter(Mandatory, ValueFromPipeline)]
        [object]$Value
    )

    Process {
        if ($Value -is [datetime] -or $Value -is [System.DateTimeOffset]) {
            return '{0:yyyy-MM-ddTHH:mm:sszzz}' -f $Value
        }
    
        if ($Value -is [array]) {
            return $Value -join ', '
        }
    
        return [string] $Value    
    }
}

function ConvertTo-ImageMetadata {
    [CmdletBinding(PositionalBinding = $false)]
    Param (
        [Parameter(Mandatory, ValueFromPipeline)]
        [PSObject] $Tags
    )

    Process {
        $sourceFile = $Tags.SourceFile
        $Tags.PSObject.Properties.Remove('SourceFile')
        if ($Tags | Get-Member Unknown) {
            # ExifTool replaces missing values with '-', reverting this
            $Tags.Unknown |
            Get-Member -MemberType NoteProperty |
            Select-Object -ExpandProperty Name |
            Foreach-Object { $Tags.Unknown.$($_) = $Tags.Unknown.$($_) | ConvertFrom-TagValue }
        }

        return [ImageMetadata] @{
            FilePath = $sourceFile;
            Tags     = $Tags
        }
    }
}

function ConvertTo-ValuesOnly {
    [CmdletBinding(PositionalBinding = $false)]
    Param (
        [Parameter(Mandatory, ValueFromPipeline)]
        [PSObject] $Tags
    )

    Process {
        # Using PSObject.Properties to list the properties instead of Get-Member incidentally preserves the order
        # in which the original json is written which itself preserves the order in which the tags were requested
        return $Tags.PSObject.Properties |
        Where-Object MemberType -EQ NoteProperty |
        Select-Object -ExpandProperty Name |
        Where-Object {$_ -ne 'SourceFile' } |
        ForEach-Object {
            $value = $Tags.$($_)
            # ExifTool replaces missing values with '-', reverting this
            if (($_ -split ':')[0] = 'Unknown') {
                return ($value | ConvertFrom-TagValue)
            }
            else {
                return $value
            }
        }
    }
}

#endregion