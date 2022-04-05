[![Test](https://github.com/GordonFreemanK/gfk-image/actions/workflows/test.yml/badge.svg)](https://github.com/GordonFreemanK/gfk-image/actions/workflows/test.yml)

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

# 3. Other third party tools

- the [Time API](https://www.timeapi.io/) is a free **online** service we can use to get information regarding UTC and DST offsets.
- the Google Maps Platform [Time Zone API](https://developers.google.com/maps/documentation/timezone/overview) is an **online** service we can optionally use for precise information regarding UTC and DST offsets.
- [GeoTimeZone](https://www.nuget.org/packages/GeoTimeZone) (included) is a [NuGet](https://www.nuget.org/) package written in C# ([sources](https://github.com/mattjohnsonpint/GeoTimeZone)) to get the [IANA timezone name](https://en.wikipedia.org/wiki/List_of_tz_database_time_zones) for a GPS location. It is based on the **offline** data constructed from [OpenStreetMap](https://www.openstreetmap.org) data by the [Timezone Boundary Builder](https://github.com/evansiroky/timezone-boundary-builder) project.

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

# Metadata read using composite tag defined in the ExifTool configuration
$taken = Get-ImageMetadata `
    -FilePaths $sourcePath `
    -TagNames Composite:CreateDate
$takenDateTime = ConvertFrom-TagDateTime -DateTime $taken

# Metadata read using source files. This will get the values from an xmp sidecar if it exists
$latitude, $longitude = Get-ImageMetadata `
    -FilePaths $sourcePath `
    -SourceFiles '%d%f.xmp','@' `
    -TagNames XMP:GPSLatitude,XMP:GPSLongitude

# Calculate offset for location and time
$takenDateTimeOffset = Get-DateTimeOffset `
    -DateTime $takenDateTime `
    -Latitude $latitude `
    -Longitude $longitude

# $sourcePath is the original file and $destinationPath is the file to be modified
# This command must be invoked before starting modifications on $destinationPath
# After this line whatever happens digiKam considers the script a success and logs 'Item processed successfully (overwritten)'
# This means any failure after this line will interrupt the process and be logged, but not be considered a failure by digiKam
cp $sourcePath $destinationPath

# Metadata write using shortcut tags defined in the temporary ExifTool configuration
# We should only write to destination, but the implementation of the Batch Queue Manager does not take the xmp sidecar in account
# By also writing to source we ensure that the sidecar gets updated
Set-ImageMetadata `
    -FilePaths $sourcePath,$destinationPath `
    -SourceFiles '%d%f.xmp','@' `
    -Tags @{
  Author = $author;
  Modified = Get-Date;
  Taken = $takenDateTimeOffset;
  Digitized = $takenDateTimeOffset;
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

*Note: this command can use multiple methods for calculation. See below for more details* 

### iii. ExifTool commands

- `Get-ImageMetadata`: gets metadata tags or shortcut tags
- `Set-ImageMetadata`: sets metadata tags or shortcut tags
- `ConvertFrom-TagDateTime`: a utility to convert dates from ExifTool format to [DateTime](https://docs.microsoft.com/en-us/dotnet/api/system.datetime) objects

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
PS C:\> $allMetadata = Get-ImageMetadata -FilePath $filePath -TagNames All -Full
    1 image files read
PS C:\> $allMetadata[0].Tags.EXIF.Artist,$allMetadata[0].Tags.XMP.CreatedDate
Gordon Freeman;Adrian Shephard
2022:01:19 15:16:17+03:00
PS C:\> '{0:r}' -f (ConvertFrom-TagDateTime -DateTime $allMetadata[0].Tags.XMP.CreatedDate)
Wed, 19 Jan 2022 15:16:17 GMT+3
```

*Notes:*
- `Get-ImageMetadata` can either output tag values without names as a list of strings, or a more complex object containing file names and tags organized by groups (EXIF, IPTC, XMP, File, etc). More details are available with `Get-Help Get-ImageMetadata`
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
PS C:\> cd Tags:
PS Tags:/> (ls People).Value
Adrian Shephard
The G-Man
```

*Note: this PSProvider does not support renaming, moving or deleting items*

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

## d. ExifTool configuration

Included in this module is an [example ExifTool configuration file](GFK.Image/ExifTool/.ExifTool_config) (read how these files can be used in the documentation in the [official example file](https://www.exiftool.org/config.html)) to create custom [shortcuts tags](https://www.exiftool.org/TagNames/Shortcuts.html), which are ways to read or write multiple metadata tags at once.

This example configures the following shortcut tags:
- `Author`: author of the photo
- `Taken`: date the photo was taken
- `Digitized`: date the photo was digitized (expected to be same as `Taken` for digital photography, but different for film photography)
- `Modified`: date the photo was modified

After this PowerShell module is installed, the file can be found at `$exifToolConfigurationPath = Join-Path (Get-Module GFK.Image).ModuleBase ExifTool`. You can:
- use it with ExifTool with the [-config option](https://exiftool.org/exiftool_pod.html#Advanced-options)
- use it with `Get-ImageMetadata` and `Set-ImageMetadata` with the `-ConfigurationPath` switch
- add `$Env:EXIFTOOL_HOME = $exifToolConfigurationPath` in your PowerShell session or scripts before running ExifTool-related commands
- run `cp (Join-Path $exifToolConfigurationPath *) ~` to copy it to your home folder, after which **any instance of ExifTool including versions embedded in other applications will be using it**

# 3. Offset calculation methods

## a. Implementation

These are three modes in which `Get-DateTimeOffset` can work:
```powershell
PS C:\> (Get-DateTimeOffset -DateTime '1993-01-25T12:00:00' -Latitude 38.71667 -Longitude -9.13333 -Online).ToString()
25/01/1993 12:00:00 +01:00
PS C:\> (Get-DateTimeOffset -DateTime '1993-01-25T12:00:00' -Latitude 38.71667 -Longitude -9.13333 -Online -GoogleApiKey <googleApiKey>).ToString()
25/01/1993 12:00:00 +01:00
PS C:\> (Get-DateTimeOffset -DateTime '1993-01-25T12:00:00' -Latitude 38.71667 -Longitude -9.13333 -Offline).ToString()
25/01/1993 12:00:00 +00:00
```

## b. Explanation
- The first two calls uses information provided by online services. The returned value is the correct UTC offset for that location and date.
  - The first call is made with the Time API endpoint (this is the default, i.e. the method used if the `-Online` parameter is not specified)
  - The second call is made wit the Google Time Zone API endpoint
- The second call uses the offline information as available in `GeoTimeZone` (for the time zone information) and in .NET (for the offset for a date/time and timezone).

**These three calls will most of the time return the same value**, but sometimes historical changes in base UTC offset and DST application for a specific location can throw off the offline resolution. In this example, the coordinates correspond to Lisbon in Portugal, where between 1992 and 1996 the time zone was CET (UTC+01:00), as opposed to WET (UTC+00:00) before and since.

## c. Pros and cons

|                                                                                            | Pros                                                                                  | Cons                                                                                                                                 |
|--------------------------------------------------------------------------------------------|---------------------------------------------------------------------------------------|--------------------------------------------------------------------------------------------------------------------------------------|
| [Time API](https://www.timeapi.io/)                                                        | <ul><li>Free</li><li>Exact</li></ul>                                                  | <ul><li>Slower &ast;</li><li>Might break or stop working in the future</li></ul>                                                     |
| [Google Time Zone API](https://developers.google.com/maps/documentation/timezone/overview) | <ul><li>Exact</li><li>Can be expected to not break in the future</li></ul>            | <ul><li>Slower &ast;</li><li>Not free &ast;&ast;</li><li>Requires setup</li><li>Registration requires personal information</li></ul> |
| [GeoTimeZone](https://github.com/mattjohnsonpint/GeoTimeZone)                              | <ul><li>Free</li><li>Fastest</li><li>Works offline</li><li>Will always work</li></ul> | <ul><li>Can be inexact in certain conditions</li></ul>                                                                               |


&ast; The speed difference between online and offline should be minimal and might be irrelevant depending on your workflow and internet speed.

&ast;&ast; At the time of writing, the [pricing structure](https://developers.google.com/maps/documentation/timezone/usage-and-billing) of the Google Time Zone API for individuals allows for 100,000 free requests the first month after registration and 40,000 free requests monthly thereafter. This might be more than enough for most people.

## d. Google API setup

- Create a Google account
- Create a Google API account: [Get Started](https://developers.google.com/maps)
- Add a billing account to your API account (this will not be charged)
- Create a new project e.g. gfk-image
- Go to APIs on the left menu, enable the Time Zone API
- Go to Credentials on the left menu, create a new API key
  - (optional) restrict it to the Time Zone API
  - You will need to copy the key when you create it as it cannot be retrieved afterwards. If you lose it you can delete and create as many API keys as you want

# 4. Unicode with ExifTool on Windows

The only way I found to make the combination of PowerShell and exiftool fully compatible (read and write) with UTF-8 characters is by setting the system locale to UTF-8. This comes with a lot of fine print. How-to and caveats can be found [here](https://stackoverflow.com/questions/49476326/displaying-unicode-in-powershell/49481797#49481797).

# 5. Modifying and running the code locally (Windows)

## a. Get the repository
```powershell
git clone https://github.com/GordonFreemanK/gfk-image.git
cd gfk-image
```

## b. Uninstall any version of the module you may have installed:
```powershell
Get-Module GFK.Image | Uninstall-Module
```

## c. Modify the code

## d. Deploy the module to the user environment for testing

```powershell
yarn deploy:user
```

The module should now be available in any `pwsh` session. You may remove it with:
```powershell
$publishPath = Join-Path ([System.Environment]::GetFolderPath('MyDocuments')) 'PowerShell' 'modules' 'GFK.Image'
rm -R $publishPath
```

Note that both publishing and removing will fail if the module is loaded in any PowerShell session (the files are locked and cannot be removed). If you are not sure which one it is, you can kill all pwsh.exe processes.

## e. (Optional) Update generated C# help files

Inline XML documentation is not supported for C# cmdlets such as `Get-DateTimeOffset`. To update this:

### i. Publish the module

As described above.

### ii. Update the intermediate markdown files

```powershell
Import-Module GFK.Image
Import-Module platyPS -Scope Local
Update-MarkdownHelp docs
```

### iii. Modify the markdown files

They are located in the `/docs` directory.

### iv. Update the generated XML documentation

```powershell
New-ExternalHelp docs -OutputPath .\GFK.Image\en-US\ -Force
```

### v. Delete and publish the module again

As described above.

### vi. Test the result

```powershell
Get-Help Get-DateTimeOffset -ShowWindow
```
