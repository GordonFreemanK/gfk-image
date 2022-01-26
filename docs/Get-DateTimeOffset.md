---
external help file: GFK.Image.dll-Help.xml
Module Name: GFK.Image
online version:
schema: 2.0.0
---

# Get-DateTimeOffset

## SYNOPSIS
Calculates the UTC offset associated with a date/time and a location

## SYNTAX

### TimeApi
```
Get-DateTimeOffset -DateTime <DateTime> -Latitude <Single> -Longitude <Single> [-Online] [-TimeApiUri <Uri>]
 [<CommonParameters>]
```

### GoogleApi
```
Get-DateTimeOffset -DateTime <DateTime> -Latitude <Single> -Longitude <Single> [-Online] [-GoogleApiUri <Uri>]
 -GoogleApiKey <String> [<CommonParameters>]
```

### GeoTimeZone
```
Get-DateTimeOffset -DateTime <DateTime> -Latitude <Single> -Longitude <Single> [-Offline] [<CommonParameters>]
```

## DESCRIPTION
This command takes a DateTime object and a latitude and longitude in decimal formats, and outputs a DateTimeOffset for a local date including the UTC offset for that particular location and date/time.

The command currently implements three ways of calculating the offset:

- The TimeApi method uses the free online service Time API (https://www.timeapi.io/swagger/index.html)

- The GoogleApi method uses the pay-as-you-go online service Google Time Zone API (https://developers.google.com/maps/documentation/timezone/overview)

- The GeoTimeZone method uses the free offline NuGet library GeoTimeZone (https://github.com/mattjohnsonpint/GeoTimeZone)

## EXAMPLES

### Example 1
```powershell
PS C:\> (Get-DateTimeOffset -DateTime '2020-08-25T12:00:00' -Latitude 38.71667 -Longitude -9.13333 -Online -TimeApiUri 'https://www.timeapi.io').ToString('o')
25/08/2020 12:00:00 +01:00
```

This example uses the Time API. The -Online and -TimeApiUri parameters are optional, this example shows the default URI value.

### Example 2
```powershell
PS C:\> Get-DateTimeOffset -DateTime '2020-08-25T12:00:00' -Latitude 38.71667 -Longitude -9.13333 -Online -GoogleApiUri 'https://maps.googleapis.com/maps/api/timezone/json' -GoogleApiKey <Your API key here>
25/08/2020 12:00:00 +01:00
```

This example uses the Google API. The -Online and -GoogleApiUri parameters are optional, this example shows the default URI value.

### Example 3
```powershell
PS C:\> Get-DateTimeOffset -DateTime '2020-08-25T12:00:00' -Latitude 38.71667 -Longitude -9.13333 -Offline
25/08/2020 12:00:00 +01:00
```

This example uses the GeoTimeZone NuGet package.

## PARAMETERS

### -DateTime
A DateTime expected to have no offset specified (DateTimeKind.Unspecified)

```yaml
Type: DateTime
Parameter Sets: (All)
Aliases:

Required: True
Position: Named
Default value: None
Accept pipeline input: True (ByPropertyName)
Accept wildcard characters: False
```

### -Latitude
The latitude, in signed digital format

```yaml
Type: Single
Parameter Sets: (All)
Aliases:

Required: True
Position: Named
Default value: None
Accept pipeline input: True (ByPropertyName)
Accept wildcard characters: False
```

### -Longitude
The longitude, in signed digital format

```yaml
Type: Single
Parameter Sets: (All)
Aliases:

Required: True
Position: Named
Default value: None
Accept pipeline input: True (ByPropertyName)
Accept wildcard characters: False
```

### -GoogleApiKey
A valid API key for the Google Maps Platform with access to the Time Zone API

```yaml
Type: String
Parameter Sets: GoogleApi
Aliases:

Required: True
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -GoogleApiUri
The base URI for the Google Time Zone API

```yaml
Type: Uri
Parameter Sets: GoogleApi
Aliases:

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -Offline
Use GeoTimeZone for the offset calculation

```yaml
Type: SwitchParameter
Parameter Sets: GeoTimeZone
Aliases:

Required: True
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -Online
Use Time API or Google API for the offset calculation

```yaml
Type: SwitchParameter
Parameter Sets: TimeApi, GoogleApi
Aliases:

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -TimeApiUri
The base URI for the Time API

```yaml
Type: Uri
Parameter Sets: TimeApi
Aliases:

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### CommonParameters
This cmdlet supports the common parameters: -Debug, -ErrorAction, -ErrorVariable, -InformationAction, -InformationVariable, -OutVariable, -OutBuffer, -PipelineVariable, -Verbose, -WarningAction, and -WarningVariable. For more information, see [about_CommonParameters](http://go.microsoft.com/fwlink/?LinkID=113216).

## INPUTS

### System.DateTime

### System.Single

## OUTPUTS

### System.DateTimeOffset

## NOTES

## RELATED LINKS
