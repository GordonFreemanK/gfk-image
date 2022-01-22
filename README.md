# 1. What is this?

This PowerShell module provides:
- A simplified wrapper around [ExifTool](https://exiftool.org/) (tool not included) to read and write image metadata
- A Windows-only command to make [digiKam](https://www.digikam.org/download/) Batch Queue Manager's User Shell Script plugin PowerShell-aware
- Useful commands to:
  - Calculate [UTC offsets](https://en.wikipedia.org/wiki/UTC_offset) automatically based on a date/time and GPS location
  - Load digiKam tags for an image as if they were a PowerShell drive
- An example [.ExifTool_config](GFK.Image/ExifTool/.ExifTool_config) file to configure shortcut tags on ExifTool  

Written in C# / PowerShell, and tested on Windows.

# 2. Pre-requisites

These tools must be installed and are free, open-source and available on Windows, Linux and macOS.

- [PowerShell Core](https://docs.microsoft.com/en-us/powershell/scripting/install/installing-powershell) (version 6.0 and above)  is the default modern shell on Windows systems, written in C# ([sources](https://github.com/powershell/powershell)).


- [digiKam](https://www.digikam.org/download/) is an image manager written in C++ ([sources](https://invent.kde.org/graphics/digikam), [mirror](https://github.com/KDE/digikam)) for visually organizing, viewing and editing image collections using file operations, image transformations and metadata editing. It features [Batch Queue Manager](https://userbase.kde.org/Digikam/Batch_Process) which allows defining and batch-processing groups of images using configurable plugins, including the **User Shell Script** plugin.

- [ExifTool](https://exiftool.org/) is a command-line utility written in Perl ([sources](https://github.com/exiftool/exiftool)) to losslessly (without modifying the image itself) edit image metadata (aka **EXIF/IPTC/XMP tags**), and is in fact used by digiKam as well as many other image management applications to manage metadata.

ExifTool can be installed by following [the official instructions](https://exiftool.org/install.html) or:
  - On Linux, by using the package manager
  - On Windows, by using [this third party installer](https://oliverbetz.de/pages/Artikel/ExifTool-for-Windows)

# 3. Included third-party package

- [GeoTimeZone](https://www.nuget.org/packages/GeoTimeZone) is a [NuGet](https://www.nuget.org/) package written in C# ([sources](https://github.com/mattjohnsonpint/GeoTimeZone)) to get the [IANA timezone name](https://en.wikipedia.org/wiki/List_of_tz_database_time_zones) for a GPS location. It is based on the offline data constructed from [OpenStreetMap](https://www.openstreetmap.org) data by the [Timezone Boundary Builder](https://github.com/evansiroky/timezone-boundary-builder) project.

# 4. Quick Start

**Important! This tool *modifies your files*! You should back them up before using it.**

## a. Install the PowerShell module
```powershell
 Install-Module GFK.Image
```

## b. Install PowerShell in digiKam
```powershell
Install-PSDigiKam
```
*Note: Run this again after upgrading digiKam or GFK.Image.*

## c. Choose and configure an image in your digiKam collection
  - Set a tag value, for instance `Author/Gordon Freeman` (`right click > Assign Tag`)
  - Set an `EXIF: Original` date (`Item > Adjust Time & Date...`)
  - Set GPS coordinates (`Item > Edit Geolocation...`)

## d. Add PowerShell code in the User Shell Script window in digiKam Batch Queue Manager
  - Load the image in Batch Queue Manager
  - Select `Behaviour > If Target File Exists > Overwrite automatically` (do this only if you have backups!)
  - Choose the User Shell Script tool
  - Paste the following code
  - Run the Batch Queue Manager

```powershell
# Important! Do not use isolated ampersand characters anywhere in this script as they will break the digiKam PowerShell integration

# This will ensure that the script stops on error
# Errors are reported in digiKam as 'User Script: Script process crashed'
$ErrorActionPreference = 'Stop'

# Double double quotes here are intentional (one of the set gets replaced by digiKam)
# File paths containing '$' or '`' characters will fail
$sourcePath = ""$INPUT""
$destinationPath = ""$OUTPUT""

# Create a Tags: drive to navigate digiKam tags
New-PSDigiKamDrive -Tags $Env:TAGSPATH
$author = (ls Tags:/Author).Value

# Temporarily set the ExifTool configuration file path to the one included in the module
$Env:EXIFTOOL_HOME = Join-Path (Get-Module GFK.Image).ModuleBase ExifTool

# Example metadata read
$latitude, $longitude, $taken = Get-ImageMetadata `
    -FilePath $sourcePath `
    -TagNames XMP:GPSLatitude,XMP:GPSLongitude,EXIF:DateTimeOriginal
$takenDateTime = ConvertFrom-ImageDateTime -DateTime $taken
$takenDateTimeOffset = Get-DateTimeOffset `
    -DateTime $takenDateTime `
    -Latitude $latitude `
    -Longitude $longitude

# $sourcePath is the original file and $destinationPath is the file to be modified
# This command must be invoked before starting modifications on $destinationPath
# After this line whatever happens digiKam considers the script a success and logs 'Item processed successfully (overwritten)'
# This means any failure after this line will interrupt the process and be logged, but not be considered a failure by digiKam
cp $sourcePath $destinationPath

# Example metadata write using shortcut tags defined in the temporary ExifTool configuration
Set-ImageMetadata `
    -FilePath $destinationPath `
    -Tags @{
        Author = $author;
        Modified = Get-Date;
        Taken = $takenDateTimeOffset
    }
```

## e. Expected results

- The value `Gordon Freeman` should be set in:
  - `EXIF > Image Information > Artist`
  - `XMP > Dublin Core > Creator`
- The date you set previously should now also be set in:
  - `EXIF > Image Information > Date and Time`
  - `EXIF > Photograph Information > Date and Time (original)`
  - `XMP > Adobe Photoshop > Date Created`
  - `XMP > Basic Schema > Modify Date`
  - `XMP > Exif-specify Properties > Date and Time Original`
  - `XMP > TIFF Properties > Date and Time`
- The offset corresponding to the date and coordinates you chose should be set in the four XMP fields above as well as in:
  - `EXIF > Offset Time`
  - `EXIF > Offset Time Original`
  
  ***Note:** EXIF offsets are only visible on digiKam versions 7.3 and above*

# 5. Implementation

## a. PowerShell commands included in the module

### i. Notes
- use `Get-Help <CommandName>` to get more information about these commands
- `-WhatIf` and `-Confirm` switches are available for all commands that modify files

### ii. Helper commands
  - `Get-DateTimeOffset`

This cmdlet takes a [DateTime](https://docs.microsoft.com/en-us/dotnet/api/system.datetime) (local or not), latitude and longitude (in signed [decimal degree](https://en.wikipedia.org/wiki/Decimal_degrees) format) and returns a [DateTimeOffset](https://docs.microsoft.com/en-us/dotnet/api/system.datetimeoffset) including the offset for the local time zone *at that time*.

*Usage (this example uses automatic DateTime conversion from a string for the input):*
```powershell
PS C:\> (Get-DateTimeOffset -DateTime '2022-01-19 15:16:17' -Latitude -3.075833 -Longitude 37.353333).ToString()
19/01/2022 15:16:17 +03:00
```

### iii. ExifTool commands

- `Get-ImageMetadata`: gets metadata tags or shortcut tags
- `Set-ImageMetadata`: sets metadata tags or shortcut tags
- `ConvertFrom-ImageDateTime`: a utility to convert dates from ExifTool format to [DateTime](https://docs.microsoft.com/en-us/dotnet/api/system.datetime) objects

*Usage:*
```powershell
PS C:\> $filePath = 'C:\Users\Gordon Freeman\Pictures\Black Mesa Research Center.jpg'
PS C:\> Set-ImageMetadata -FilePath $filePath -Tags @{'XMP:CreatedDate'='2022-01-19 15:16:17+03:00';Artist='Gordon Freeman','Adrian Shephard'}
    1 image files updated
PS C:\> $artist,$createdDate = Get-ImageMetadata -FilePath $filePath -TagNames Artist,XMP:CreatedDate
    1 image files read
PS C:\> $artist,$createdDate
Gordon Freeman;Adrian Shephard
2022:01:19 15:16:17+03:00
PS C:\> $allMetadata = Get-ImageMetadata -FilePath $filePath -TagNames All -Full -Grouped
    1 image files read
PS C:\> $allMetadata[0].Tags.EXIF.Artist,$allMetadata[0].Tags.XMP.CreatedDate
Gordon Freeman;Adrian Shephard
2022:01:19 15:16:17+03:00
PS C:\> '{0:r}' -f (ConvertFrom-ImageDateTime -DateTime $allMetadata[0].Tags.XMP.CreatedDate)
Wed, 19 Jan 2022 15:16:17 GMT+3
```

*Notes:*
- `Get-ImageMetadata` can either output tag values without names as a list of strings, or a more complex object containing file names, tag names and optionally tag groups (EXIF, IPTC, XMP). More details are available with `Get-Help Get-ImageMetadata`
- `Get-ImageMetadata` and `Set-ImageMetadata` will echo the exiftool command if using the `-Verbose` switch
- There are multiple types of date/time tags in EXIF/IPTC/XMP. All these types can be set from a [DateTime](https://docs.microsoft.com/en-us/dotnet/api/system.datetime) or [DateTimeOffset](https://docs.microsoft.com/en-us/dotnet/api/system.datetimeoffset) value. `Set-ImageMetadata` serializes it to the [ISO 8601 format](https://en.wikipedia.org/wiki/ISO_8601) (e.g. `2022-01-15T15:28:36+01:00`) and ExifTool automatically stores the relevant part in any of these fields:
  - EXIF uses one tag for date+time and one for offset (e.g. `EXIF:DateTimeOriginal` and `EXIF:OffsetTimeOriginal`)
  - IPTC uses one tag for date and one for time+offset (e.g. `IPTC:DateCreated` and `IPTC:TimeCreated`)
  - XMP stores the date+time+offset in a single tag (e.g. `XMP-exif:DateTimeOriginal`)

### iv. digiKam commands

- `Install-PSDigiKam` (Windows 64 bit only, requires elevation): changes the shell for the User Shell Script plugin in digiKam to `pwsh.exe` by copying a PowerShell bootstrapper named `cmd.exe` in the digiKam install location (more details below).
- `Uninstall-PSDigiKam` (Windows 64 bit only, requires elevation): resets the shell for the User Shell Script plugin in digiKam to the default `cmd.exe`.

*Usage:*
```powershell
PS C:\> Install-PSDigiKam
PS C:\> Uninstall-PSDigiKam
```

- `New-PSDigiKamDrive`: reads a string containing an image tags as formatted by digiKam (using a `;` as a separator between tags and a `/` as a path separator within each tag) and creates a drive (e.g. `Tags:`) drive in the PowerShell session.

*Usage:*
```powershell
PS C:\> New-PSDigiKamDrive -Name Tags -Tags 'Author/GFK;People/Adrian Shephard;People/The G-Man'
PS C:\> cd Tags:\
PS Tags:\> (ls People).Value
Adrian Shephard
The G-Man
```

*Notes:*
- PowerShell uses `\ ` as a path separator. `\ ` in tag values will be replaced by `-`.
- This PSProvider does not support renaming, moving or deleting items

## b. Injecting `pwsh.exe` into digiKam

The [digiKam User Shell Script plugin code](https://github.com/KDE/digikam/blob/master/core/dplugins/bqm/custom/userscript/userscript.cpp) runs the user-defined script by splitting the script into its component lines, serializing them using the [& operator](https://bashitout.com/2013/05/18/Ampersands-on-the-command-line.html) and passing the result to `sh` on Linux and [cmd](https://en.wikipedia.org/wiki/Cmd.exe) on Windows.

In order to avoid `cmd.exe` altogether, we inject a standalone executable, also called `cmd.exe`, but which is really a PowerShell bootstrapper,  in the digiKam application path where it will take precedence over the actual Windows command prompt executable.
This executable rebuilds the original user-defined script then passes it for execution to `pwsh.exe` (PowerShell Core).

We are now able to use PowerShell code in the User Shell Script plugin, and save it as part of a Batch Queue Manager workflow.

**Notes:**
- The digiKam plugin does not log but the fake cmd.exe logs any failure to `${Env:LocalAppData}\GFK\GFK.Image.cmd\cmd.log`
- Loading PowerShell is a relatively CPU-intensive task. **Consider using multi-threading** by selecting `Queue Settings > Behaviour > Work on all processor cores` in Batch Queue Manager, unless your scripts use a non-thread-safe resource, or if I/O is the bottleneck (e.g. operations relying on a slow network connection)
- **Avoid any isolated `&` as they will be lost in translation and replaced by a new line**
- **Disable digiKam metadata writing for the fields modified by these scripts** (unselect the relevant sections in `Settings > Configure digiKam... > Metadata > Behaviour > Write This Information to the Metadata`), or they could be overwritten after the batch script runs

## c. ExifTool configuration

Included in this module is an [example ExifTool configuration file](GFK.Image/ExifTool/.ExifTool_config) (read how these files can be used in the documentation in the [official example file](https://www.exiftool.org/config.html)) to create custom [shortcuts tags](https://www.exiftool.org/TagNames/Shortcuts.html), which are ways to read or write multiple metadata tags at once.

This example configures the following shortcut tags:
- `Author`: author of the photo
- `Taken`: date the photo was taken
- `Digitized`: date the photo was digitized (expected to be same as `Taken` for digital photography, but different for film photography)
- `Modified`: date the photo was modified

If the PowerShell module is installed, the file can be found at `$exifToolConfigurationPath = Join-Path (Get-Module GFK.Image).ModuleBase ExifTool`

It can be used by exiftool directly using the [-config option](https://exiftool.org/exiftool_pod.html#Advanced-options) or by the ExifTool wrapper commands in this module using the `-ConfigurationPath $exifToolConfigurationPath` switch.

Alternatively, you can enter the statement `$Env:EXIFTOOL_HOME = $exifToolConfigurationPath` in your PowerShell session or scripts before running commands related to ExifTool.

Finally, you can `cp $exifToolConfigurationPath ~` to place it in your home folder, after which **any instance of ExifTool including versions embedded in other applications will be using it!**

# 4. Unicode with ExifTool on Windows

The only way I found to make the combination of PowerShell and exiftool fully compatible (read and write) with UTF-8 characters is by setting the system locale to UTF-8. This comes with a lot of fine print. How-to and caveats can be found [here](https://stackoverflow.com/questions/49476326/displaying-unicode-in-powershell/49481797#49481797).

# 5. Modifying and running the code locally (Windows)

Get the repository:
```powershell
git clone https://github.com/GordonFreemanK/gfk-image.git
cd gfk-image
```

Uninstall any version of the module you may have installed:
```powershell
Get-Module GFK.Image | Uninstall-Module
```

At this point you may change the code as you see fit locally. When you are ready to test it, deploy it to your user folder:
```powershell
$publishPath = Join-Path ([System.Environment]::GetFolderPath('MyDocuments')) 'PowerShell' 'modules' 'GFK.Image'
dotnet publish GFK.Image -c Release -o $publishPath
dotnet publish GFK.Image.cmd -c Release -o (Join-Path $publishPath cmd)
```

The module should now be available in any terminal. You may remove it with:
```powershell
$publishPath = Join-Path ([System.Environment]::GetFolderPath('MyDocuments')) 'PowerShell' 'modules' 'GFK.Image'
rm -R $publishPath
```

Note that both publishing and removing will fail if the module is loaded in any powershell environment (the file is locked and cannot be removed). If you are not sure which one it is, you can kill all pwsh.exe processes.

# 6. Changelog

## 1.0.0
Initial draft (unlisted)