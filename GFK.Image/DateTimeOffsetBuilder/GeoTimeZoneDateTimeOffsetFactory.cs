using System;
using System.Threading.Tasks;
using GeoTimeZone;

namespace GFK.Image.DateTimeOffsetBuilder;

internal class GeoTimeZoneDateTimeOffsetFactory : IDateTimeOffsetFactory
{
    public Task<DateTimeOffset> Build(DateTime dateTime, float latitude, float longitude)
    {
        var timeZoneResult = TimeZoneLookup.GetTimeZone(latitude, longitude);

        var timeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById(timeZoneResult.Result);

        var offset = timeZoneInfo.GetUtcOffset(dateTime);

        var result = new DateTimeOffset(dateTime, offset);

        return Task.FromResult(result);
    }
}