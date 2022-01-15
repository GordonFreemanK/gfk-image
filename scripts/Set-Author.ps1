<#
.SYNOPSIS
    Sets author tags on a photo file
.DESCRIPTION
    Sets the value in the Author ExifTool shortcut tag
.EXAMPLE
    PS C:\> Set-Author.p1 C:\photos\photo1.jpg -Author 'Alan Turing'
#>
[CmdletBinding()]
param (
    [Parameter(Mandatory, Position=0)]
    [string]$FilePath,

    [Parameter(Mandatory)]
    [string]$Author
)

exiftool -overwrite_original $FilePath -Modified<now '-Author<$AuthorParam' -userParam AuthorParam="$Author"