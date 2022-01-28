#Requires -PSEdition Core

param(
    [Parameter(Mandatory, Position=0)][string] $Path,
    [Parameter()][string] $Configuration = 'Debug'
)

$ModulePath = Join-Path $Path GFK.Image;
dotnet publish GFK.Image -c $Configuration -o $ModulePath --nologo;
dotnet publish GFK.Image.cmd -c $Configuration -o (Join-Path $ModulePath cmd) --nologo;
