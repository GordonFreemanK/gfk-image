<#
.SYNOPSIS
    Sets Authors tags on a photo file
.DESCRIPTION
    Concatenates the values and stores it in the Authors ExifTool shortcut tag for the file
.EXAMPLE
    PS C:\> Set-Author.p1 C:\photos\photo1.jpg -Authors 'Alan Turing','Charlie Chaplin'
#>
[CmdletBinding]
param (
    [Parameter(Mandatory)][string]$FilePath,
    [Parameter(Mandatory)][string[]]$Authors
)

exiftool -overwrite_original $FilePath -Modified<now '-Authors<$AuthorsParam' -userParam AuthorsParam="${$Authors -join ';'}"