# What is this?

**This repository is aimed at integrating PowerShell scripts in a purely visual workflow for batch-processing photos, initially with the goal of automatically setting [UTC offsets](https://en.wikipedia.org/wiki/UTC_offset) based on the picture location and time.**

Written mostly in C# / PowerShell, and tested on Windows. All the tools and technologies used are open-source and available on both Linux and Windows.

# Pre-requisites

- [git](https://git-scm.com/)
- [PowerShell](https://docs.microsoft.com/en-us/powershell/scripting/install/installing-powershell) version 6.0 and above
- [.NET SDK](https://dotnet.microsoft.com/en-us/download) version 6.0 and above
- [digiKam](https://www.digikam.org/download/)
- [ExifTool](https://exiftool.org/) should come installed with digiKam but the latest version can be downloaded directly and [added to the path](https://docs.microsoft.com/en-us/powershell/module/microsoft.powershell.core/about/about_environment_variables?view=powershell-7.2#saving-changes-to-environment-variables) or installed using a [third-party installer](https://oliverbetz.de/pages/Artikel/ExifTool-for-Windows)

# tl;dr / Quick start

Execute the following commands **just once**. Most need to be executed in an **elevated** PowerShell prompt.

## Clone the repository
```
git clone https://github.com/GordonFreemanK/digikam-scripts.git
Set-Location digikam-scripts
```

## Set ExifTool configuration
```
Move-Item ~\.Exiftool_config ~\.Exiftool_config.bak -SilentlyContinue
Copy-Item exiftool\.Exiftool_config ~
```

## Create a fake cmd.exe and copy it to digiKam's folder
```
dotnet publish -p:PublishSingleFile=true -p:DebugType=none -r win-x64 -c Release --sc -o 'C:\Program Files\digiKam\'
```

## Install GeoTimeZone in PowerShell
```
Install-Package -Name GeoTimeZone -ProviderName NuGet -SkipDependencies -Confirm
```

## Add the following in the User Shell Script window in digiKam
Replace paths to the git repository accordingly.
```
$ErrorActionPreference = 'Stop'
$sourcePath = ""$INPUT""
$destinationPath = ""$OUTPUT""
Copy-Item $sourcePath $destinationPath
C:\repos\digikam-scripts\scripts\Adjust-date-offsets.ps1 $destinationPath
C:\repos\digikam-scripts\scripts\Adjust-author.ps1 $destinationPath -Author $Env:TAGSPATH
```

# External tools

- [digiKam](https://www.digikam.org/) is a photo manager (C++, [source](https://invent.kde.org/graphics/digikam), [mirror](https://github.com/KDE/digikam)) for visually organizing, viewing and editing collections of pictures using file operations, image transformations and metadata editing. One of its features is called [Batch Queue Manager](https://userbase.kde.org/Digikam/Batch_Process) and allows defining and batch-processing groups of pictures using a number of configurable plugins.

One of these plugins is called **User Shell Script**: it enables the user to use a free text field to define a shell script to be run against the picture files. The script is transformed to an extent by digiKam and passed to the shell (`sh` on Linux and `cmd` on Windows).    

- [ExifTool](https://exiftool.org/) is a command-line utility (Perl, [source](https://github.com/exiftool/exiftool)) to losslessly (without modifying the image itself) edit photo metadata (aka **EXIF/IPTC/XMP tags**), and is actually used by digiKam as well as many other photo management applications for all their metadata management needs.

- [GeoTimeZone](https://www.nuget.org/packages/GeoTimeZone) is a [NuGet](https://www.nuget.org/) package (C#, [source](https://github.com/mattjohnsonpint/GeoTimeZone)) to get the timezone for a location. This is based on the offline data constructed from [OpenStreetMap](https://www.openstreetmap.org) data by the [Timezone Boundary Builder](https://github.com/evansiroky/timezone-boundary-builder) project.

# Implementation

## Injecting `pwsh.exe`

On Windows, the [digiKam User Shell Script plugin code](https://github.com/KDE/digikam/blob/master/core/dplugins/bqm/custom/userscript/userscript.cpp) runs the user-defined script by splitting the script into its component lines, serialising it using the [& operator](https://bashitout.com/2013/05/18/Ampersands-on-the-command-line.html) and passing the result to the [Windows command prompt](https://en.wikipedia.org/wiki/Cmd.exe).

The `cmd.exe` rules about [argument passing](http://www.windowsinspired.com/understanding-the-command-line-string-and-arguments-received-by-a-windows-program/) and [character escaping](https://fabianlee.org/2018/10/10/saltstack-escaping-dollar-signs-in-cmd-run-parameters-to-avoid-interpolation/) make it difficult to work with, in particular with the [high usage of special characters by the exiftool CLI](https://www.exiftool.org/exiftool_pod.html#WRITING-EXAMPLES), including double quotes.

To circumvent this issue we create a standalone executable called `cmd.exe` which is really a .net application that takes all the arguments passed by the digiKam plugin and reconstructs the original user-defined script by replacing `&` with new lines, then passes it for execution to `pwsh.exe` (PowerShell Core). This executable is then copied to the digiKam application folder where it will take precedence over the actual Windows command prompt executable.

We are now able to use PowerShell in the User Shell Script plugin. To avoid the user-defined code in the plugin being too complex, we create a skeleton PowerShell boostrapper which calls external PowerShell scripts.

Provided in this repository are two such external scripts:

- `Adjust-date-offsets.ps1`: automatically set offsets (i.e. timezone offset plus DST if applies) based on GPS coordinates and date/time saved in the photo metadata.
- `Adjust-author.ps1`: automatically sets the author fields based on a digiKam tag.

### Notes

- The digiKam plugin lacks any kind of logging but the fake cmd.exe logs any failure to a **cmd.log file on the user desktop**.

- Loading PowerShell is a relatively CPU-intensive task. **Consider using multi-threading** by selecting `Queue Settings > Behaviour > Work on all processor cores` in Batch Queue Manager, unless your scripts use a non-thread-safe resource, or if I/O is the bottleneck (e.g. operations relying on a slow network connection).

- Due to the way the User Shell Script plugin works, **any isolated `&` will be lost in translation and replaced by a new line**. Avoid isolated `&` commands.

- **digiKam metadata writing needs to be disabled for the fields you modify with these scripts** (unselect the relevant sections in `Settings > Configure digiKam... > Metadata > Behaviour > Write This Information to the Metadata`), or they will be overwritten after the batch script runs.

- before sending it to the shell, the plugin respectively replaces the terms `$INPUT` and `$OUTPUT` with the path of the file currently in the library, and the path of a temporary file created in the same directory, and if either term is surrounded by double quotes, those get replaced as well. This is why there are *double double* quotes in the bootstrapper script. Double quoted strings in PowerShell *do* perform interpolation so **in the event that the original file path includes `$` or a backtick (`) the process will fail** for that particular file.

- the output file is created empty by digiKam before the User Shell script is called. This is why having a simple command such as `exiftool '$INPUT' -o '$OUTPUT' -All=` fails with *file already exists*. In our user shell script we overwrite the empty output file by calling `Copy-Item` before running other scripts.

- `$ErrorActionPreference = 'Stop'` will ensure that the script stops on error. Errors are reported in digiKam as `User Script: Script process crashed` but if the file was actually copied it considers the step a success and logs `Item processed successfully (overwritten)`. This cannot be changed and means that any failure after the line `Copy-Item $sourcePath $destinationPath` will stop the process and be reported but not be considered as a failure by digiKam.

## Getting the UTC offset for a location

### 1. Get the location and date/time from the file

We use ExifTool to get the coordinates and date the picture was taken and digitized from the appropriate tags (default is `XMP-exif:GPSLatitude`, `XMP-exif:GPSLongitude`, `EXIF:DateTimeOriginal` and `EXIF:CreateDate`, change as you see fit).

### 2. Determine the time zone associated with the location

In the absence of an equivalent PowerShell module on the official repository, GeoTimeZone is a .net (C#) library conveniently released publicly as a NuGet package with no dependencies and containing a single standalone executable. This is what enables it to be installed in PowerShell despite not being a PowerShell module or Cmdlet. It provides a method to get the [IANA timezone name](https://en.wikipedia.org/wiki/List_of_tz_database_time_zones) from GPS locations.

### 3. Calculate the offset for the timezone at a specific date/time

We use the .net class [TimeZoneInfo](https://docs.microsoft.com/en-us/dotnet/api/system.timezoneinfo) to get the offset for a specific timezone and date.
The date in the EXIF tag is assumed to be local.

### 4. Store the date/time and offset in the relevant fields.

From a local date/time and an offset, a full date is constructed in [ISO 8601 format](https://en.wikipedia.org/wiki/ISO_8601) (e.g. `2022-01-15T15:28:36+01:00`).

There are multiple types of date/time tags in the metadata:
- XMP stores the date+time+offset in a single tag (e.g. `XMP-exif:DateTimeOriginal`)
- IPTC uses one tag for date and one for time+offset (e.g. `IPTC:DateCreated` and `IPTC:TimeCreated`)
- EXIF uses one tag for date+time and one for offset (e.g. `EXIF:DateTimeOriginal` and `EXIF:OffsetTimeOriginal`)

Luckily, ExifTool automatically stores the relevant part in any of these fields when the input is a fully qualified ISO 8601 date/time.

## Configuring ExifTool

Considering the number of equivalent tags in photo metadata, you will probably need a way to store the same value in multiple tags in one command. In order to do that, we can use an ExifTool configuration file (see documentation in the [example file](https://www.exiftool.org/config.html)).

In this configuration file, we create custom [shortcuts tags](https://www.exiftool.org/TagNames/Shortcuts.html), which are ways to read or write multiple tags at once. This configuration file is then copied to the root of the user home folder, where it will be picked up by ExifTool.

**Important: any instance of ExifTool running under your user session will be using this configuration file once it is copied!** If you want to safely modify it, test it by *not* copying it and using the [-config option](https://exiftool.org/exiftool_pod.html#Advanced-options).

The following shortcut tags are created by this repository, change them as needed:
- `Author`: author of the picture
- `Taken`: date the picture was taken
- `Digitized`: date the picture was digitized (expected to be same as `Taken` for digital photography, but different for film photography)
- `Modified`: date the picture was modified

**Note:** shortcut tags cannot be set with the `=` operator and need to be set with the `<` operator. In turn, the `<` operator does not play well with constant values when they contain special characters such as `$`. It is easier to use the [`-userParam` option](https://exiftool.org/exiftool_pod.html#Advanced-options) for assigning constant values to shortcut tags.

## Unicode with ExifTool on Windows

The only way I found to make the combination of PowerShell and exiftool fully compatible (read and write) with UTF-8 characters is by setting the system locale to UTF-8. This comes with a lot of fine print. How-to and caveats can be found [here](https://stackoverflow.com/questions/49476326/displaying-unicode-in-powershell/49481797#49481797).