# digikam-scripts

This repo contains utilities to allow running PowerShell scripts as User Shell Scripts in digiKam Batch Queue Manager. Provided scripts:

- `Adjust-date-offsets.ps1`: automatically set offsets (i.e. timezone plus DST if applies) based on GPS coordinates.

# Quick start

Clone the repository
```
git clone https://github.com/GordonFreemanK/digikam-scripts.git
cd digikam-scripts
```

Set ExifTool configuration
```
cp exiftool\.Exiftool_config ~
```

Build and copy a fake cmd.exe to digiKam's folder
```
dotnet publish -p:PublishSingleFile=true -p:DebugType=none -r win-x64 -c Release --sc -o 'C:\Program Files\digiKam\'
```

Install [GeoTimeZone](https://github.com/mattjohnsonpint/GeoTimeZone)
```
Install-Package -Name GeoTimeZone -ProviderName NuGet -SkipDependencies -Confirm
```

Add the following in the User Shell Script in digiKam
```
$sourcePath = '$INPUT'
$destinationPath = '$OUTPUT'
Copy-Item $sourcePath $destinationPath
C:\repos\digikam-scripts\scripts\Adjust-date-offsets.ps1 $destinationPath
C:\repos\digikam-scripts\scripts\Adjust-author.ps1 $destinationPath -Author $Env:TAGSPATH
```