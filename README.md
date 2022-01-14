# digikam-scripts

Clone the repository
```
git clone https://github.com/GordonFreemanK/digikam-scripts.git
cd digikam-scripts
```

Build and copy a fake cmd.exe to digiKam's folder
```
dotnet publish -p:PublishSingleFile=true -p:DebugType=none -r win-x64 -c Release --sc -o 'C:\Program Files\digiKam\'
```

Set ExifTool configuration
```
cp exiftool\.Exiftool_config ~
```