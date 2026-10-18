if ([string]::IsNullOrWhiteSpace($env:PSGALLERY_PUBLISH_TOKEN)) {
    throw 'PSGallery publish token is missing'
}

Write-Host 'PSGallery publish token is present'
Publish-Module -Path 'pub/GFK.Image' -NuGetApiKey $env:PSGALLERY_PUBLISH_TOKEN -ErrorAction Stop
