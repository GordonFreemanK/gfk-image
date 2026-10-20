using System;
using System.Net.Http;
using System.Threading.Tasks;
using GFK.Time.API;

namespace GFK.Image.DateTimeOffsetBuilder;

internal class TimeApiDateTimeOffsetFactory(Uri uri) : IDateTimeOffsetFactory
{
    public async Task<DateTimeOffset> Build(DateTime dateTime, float latitude, float longitude)
    {
        using var httpClient = new HttpClient { BaseAddress = uri };

        var timeApiClient = new TimeApiClient(httpClient);

        var currentTime = await timeApiClient.CoordinateAsync(latitude, longitude);
        if (currentTime.TimeZone == null)
            throw new Exception($"Could not get timezone for latitude {latitude} and longitude {longitude}");

        var convertRequest = new ConvertRequest(
            dateTime.ToString("yyyy-MM-dd HH:mm:ss"),
            "earlier",
            currentTime.TimeZone,
            "UTC");
        var conversion = await timeApiClient.ConvertTimeZoneAsync(convertRequest);

        var offset = dateTime - conversion.ConversionResult.DateTime;

        return new DateTimeOffset(dateTime, offset);
    }
}