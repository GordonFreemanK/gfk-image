# What is this?

This PowerShell module is aimed at integrating scripts in a visual workflow for batch-processing photos. At the moment, this consists of:
- setting metadata [UTC offsets](https://en.wikipedia.org/wiki/UTC_offset) automatically based on the photo location and time
- setting metadata tags based on digiKam tags (requires digiKam)

Written mostly in C# / PowerShell, and tested on Windows. All the tools and technologies used are open-source and available on both Linux and Windows.

# Pre-requisites

- [PowerShell](https://docs.microsoft.com/en-us/powershell/scripting/install/installing-powershell) version 5.1 and above
- [digiKam](https://www.digikam.org/download/)
- [ExifTool](https://exiftool.org/) should come installed with digiKam but the latest version can be downloaded directly and [added to the path](https://docs.microsoft.com/en-us/powershell/module/microsoft.powershell.core/about/about_environment_variables?view=powershell-7.2#saving-changes-to-environment-variables) or installed using a [third-party installer](https://oliverbetz.de/pages/Artikel/ExifTool-for-Windows)

# Quick start

**Important! This tool *modifies your files*! You should back them up before using it.**

Execute the following commands in an **elevated** PowerShell prompt.

## Install the PowerShell module
```powershell
 Install-Module GFK.Image
```

## Install a fake cmd.exe into digiKam's folder
```powershell
Install-PSDigiKam
```
**Note:** Run this again after upgrading digiKam or GFK.Image

## Add this code in the User Shell Script window in digiKam
```powershell
$ErrorActionPreference = 'Stop'

$sourcePath = ""$INPUT""
$destinationPath = ""$OUTPUT""
New-PSDigiKamDrive $Env:TAGSPATH

cp $sourcePath $destinationPath

# Example metadata read/write operations

$latitude, $longitude, $taken, $digitized = Get-ImageMetadata $FilePath `
    '-XMP-exif:GPSLatitude' `
    '-XMP-exif:GPSLongitude' `
    '-EXIF:DateTimeOriginal' `
    '-EXIF:CreateDate'

Set-ImageMetadata $destinationPath `
    -Author (ls Tags:/Author).PSChildName `
    -Modified (Get-Date) `
    -Taken (Get-DateTimeOffset $taken $latitude $longitude) `
    -Digitized (Get-DateTimeOffset $digitized $latitude $longitude)
```

### Notes

- `$INPUT` is the original file and `$OUTPUT` is the file to be modified, therefore `cp` (`Copy-Item`) must be invoked before starting modifications on `$OUTPUT`
- *double double* quotes around `$INPUT` and `$OUTPUT` are intentional (one of the set gets replaced by digiKam)
- **file paths including `$` or a backtick (`) will fail**
- `$ErrorActionPreference = 'Stop'` will ensure that the script stops on error. Errors are reported in digiKam as `User Script: Script process crashed` but if the file was actually copied it considers the step a success and logs `Item processed successfully (overwritten)`. This means any failure after the line `Copy-Item $sourcePath $destinationPath` will interrupt the process and be logged, but not be considered as a failure by digiKam.

# External tools

- [GeoTimeZone](https://www.nuget.org/packages/GeoTimeZone) is a [NuGet](https://www.nuget.org/) package (C#, [source](https://github.com/mattjohnsonpint/GeoTimeZone)) to get the [IANA timezone name](https://en.wikipedia.org/wiki/List_of_tz_database_time_zones) for a GPS location. This is based on the offline data constructed from [OpenStreetMap](https://www.openstreetmap.org) data by the [Timezone Boundary Builder](https://github.com/evansiroky/timezone-boundary-builder) project.

- [ExifTool](https://exiftool.org/) is a command-line utility (Perl, [source](https://github.com/exiftool/exiftool)) to losslessly (without modifying the image itself) edit photo metadata (aka **EXIF/IPTC/XMP tags**), and is actually used by digiKam as well as many other photo management applications to manage metadata.

- [digiKam](https://www.digikam.org/) is a photo manager (C++, [source](https://invent.kde.org/graphics/digikam), [mirror](https://github.com/KDE/digikam)) for visually organizing, viewing and editing collections of photos using file operations, image transformations and metadata editing. It features [Batch Queue Manager](https://userbase.kde.org/Digikam/Batch_Process) which allows defining and batch-processing groups of photos using configurable plugins.
- The digiKam **User Shell Script** plugin allows defining a shell script to be run against each photo in the batch. The script is transformed by digiKam and passed to the shell (`sh` on Linux and `cmd` on Windows).    

# Implementation

## 1. ExifTool configuration

To help with the manual and automated workflows, we use a custom ExifTool configuration file (see documentation in the official [example file](https://www.exiftool.org/config.html)) to create custom [shortcuts tags](https://www.exiftool.org/TagNames/Shortcuts.html), which are ways to read or write multiple tags at once. This configuration file is then copied to the root of the user home folder, where it will be picked up by ExifTool.

**Important: any instance of ExifTool running under your user session will be using this configuration file once it is copied!** If you want to safely modify it, test it by *not* copying it and using the [-config option](https://exiftool.org/exiftool_pod.html#Advanced-options) instead.

The following shortcut tags are created by this repository, change them as needed:
- `Author`: author of the picture
- `Taken`: date the picture was taken
- `Digitized`: date the picture was digitized (expected to be same as `Taken` for digital photography, but different for film photography)
- `Modified`: date the picture was modified

**Note:** shortcut tag values cannot be set with the `=` operator and need to be set with the `<` operator. In turn, the `<` operator does not play well with constant values when they contain special characters such as `$`. It is easier to use the [`-userParam` option](https://exiftool.org/exiftool_pod.html#Advanced-options) for assigning constant values to shortcut tags e.g. `exiftool myfile -MyTag<$MyTagParam -userParam MyTagParam='some string'`.

## 2. PowerShell module

### Get-DateTimeOffset

This helper cmdlet takes a [DateTime](https://docs.microsoft.com/en-us/dotnet/api/system.datetime) (local or not), latitude and longitude (in signed [decimal degree](https://en.wikipedia.org/wiki/Decimal_degrees) format) and returns a [DateTimeOffset](https://docs.microsoft.com/en-us/dotnet/api/system.datetimeoffset).

*Usage (this example uses automatic DateTime conversion from a string for the input):*
```powershell
PS C:\> (Get-DateTimeOffset '2022-01-19 15:16:17' -3.075833 37.353333).ToString()
19/01/2022 15:16:17 +03:00
```

### New-PSDigiKamDrive

This function reads a string containing the tags for a picture (using a `;` as a separator between tags and a `/` as a path separator within each tag) and creates a `Tags:` drive in the PowerShell session.

*Usage:*
```powershell
PS C:\> New-PSDigiKamDrive 'Author/GFK;People/Adrian Shephard;People/The G-Man'
PS C:\> ls Tags:\People
Adrian Shephard
The G-Man
```

**Notes:**
- PowerShell uses `\ ` as a path separator. `\ ` in tag values will be replaced by `-`.
- This PSProvider does not support renaming, moving or deleting items

### ExifTool wrapper functions

**Set-ImageTag**

This function writes tags or shortcut tags to the given file (or folder) using ExifTool. Array values are concatenated with ';'.

**Set-DateTimeOffsets**

This function reads the dates (`EXIF:DateTimeOriginal` for the date taken and `EXIF:CreateDate` for the date digitized) and GPS locations (`XMP-exif:GPSLatitude` and `XMP-exif:GPSLongitude`) from the given file (or folder) using ExifTool, calculates the offsets then writes back the dates and offsets to the the `Taken` and `Digitized` shortcut tags on the given file.

*Usage:*
```powershell
C:\> Set-ImageTag 'C:\Users\Gordon Freeman\Pictures\Black Mesa Research Center.jpg' -Author 'Gordon Freeman','Adrian Shephard'
    1 image files updated
C:\> Set-DateTimeOffsets 'C:\Users\Gordon Freeman\Pictures\Black Mesa Research Center.jpg'
    1 image files updated
```

Note on setting dates with offsets with ExifTool. There are multiple types of date/time tags in the metadata:
- EXIF uses one tag for date+time and one for offset (e.g. `EXIF:DateTimeOriginal` and `EXIF:OffsetTimeOriginal`)
- IPTC uses one tag for date and one for time+offset (e.g. `IPTC:DateCreated` and `IPTC:TimeCreated`)
- XMP stores the date+time+offset in a single tag (e.g. `XMP-exif:DateTimeOriginal`)

Luckily, we can construct a fully qualified date/time in [ISO 8601 format](https://en.wikipedia.org/wiki/ISO_8601) (e.g. `2022-01-15T15:28:36+01:00`) and ExifTool automatically stores the relevant part in any of these fields.

## 3. Injecting `pwsh.exe` into digiKam

On Windows, the [digiKam User Shell Script plugin code](https://github.com/KDE/digikam/blob/master/core/dplugins/bqm/custom/userscript/userscript.cpp) runs the user-defined script by splitting the script into its component lines, serializing them using the [& operator](https://bashitout.com/2013/05/18/Ampersands-on-the-command-line.html) and passing the result to the [Windows command prompt](https://en.wikipedia.org/wiki/Cmd.exe).

`cmd.exe` rules about [argument passing](http://www.windowsinspired.com/understanding-the-command-line-string-and-arguments-received-by-a-windows-program/) and [character escaping](https://fabianlee.org/2018/10/10/saltstack-escaping-dollar-signs-in-cmd-run-parameters-to-avoid-interpolation/) make it difficult to work with spaces and special characters. To circumvent this issue we create a standalone executable called `cmd.exe` which is really a .net application that we copy to the digiKam application folder where it will take precedence over the actual Windows command prompt executable. This application takes the arguments passed by the digiKam plugin and reconstructs the original user-defined script by replacing `&` with new lines, then passes it for execution to `pwsh.exe` (PowerShell Core).

We are now able to use PowerShell code in the User Shell Script plugin, which can be saved as part of a Batch Queue Manager workflow.

### Notes

- The digiKam plugin does not log but the fake cmd.exe logs any failure to a **cmd.log file on the user desktop**.

- Loading PowerShell is a relatively CPU-intensive task. **Consider using multi-threading** by selecting `Queue Settings > Behaviour > Work on all processor cores` in Batch Queue Manager, unless your scripts use a non-thread-safe resource, or if I/O is the bottleneck (e.g. operations relying on a slow network connection).
- **Avoid any isolated `&` as they will be lost in translation and replaced by a new line**.
- **Disable digiKam metadata writing for the fields modified by these scripts** (unselect the relevant sections in `Settings > Configure digiKam... > Metadata > Behaviour > Write This Information to the Metadata`), or they will be overwritten after the batch script runs.

## Unicode with ExifTool on Windows

The only way I found to make the combination of PowerShell and exiftool fully compatible (read and write) with UTF-8 characters is by setting the system locale to UTF-8. This comes with a lot of fine print. How-to and caveats can be found [here](https://stackoverflow.com/questions/49476326/displaying-unicode-in-powershell/49481797#49481797).
