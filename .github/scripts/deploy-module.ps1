[CmdletBinding()]
param(
    [switch] $InstallForUser
)

$ErrorActionPreference = 'Stop'
$repositoryRoot = (Resolve-Path (Join-Path $PSScriptRoot '..\..')).Path
$manifestPath = Join-Path $repositoryRoot 'GFK.Image\GFK.Image.psd1'

if ($InstallForUser) {
    $moduleVersion = (Import-PowerShellDataFile $manifestPath).ModuleVersion
    $outputPath = Join-Path (Join-Path ([Environment]::GetFolderPath('MyDocuments')) 'PowerShell\modules\GFK.Image') $moduleVersion
}
else {
    $outputPath = Join-Path $repositoryRoot 'pub\GFK.Image'
}

$projects = @(
    'GFK.Image\GFK.Image.csproj'
    'GFK.Image.cmd\GFK.Image.cmd.csproj'
)

foreach ($project in $projects) {
    $projectPath = Join-Path $repositoryRoot $project
    dotnet publish $projectPath --configuration Release --output $outputPath --no-logo
    if ($LASTEXITCODE -ne 0) {
        throw "Publishing '$project' failed with exit code $LASTEXITCODE."
    }
}
