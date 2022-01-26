using System;
using System.Threading.Tasks;

namespace GFK.Image.DateTimeOffsetBuilder;

internal interface IDateTimeOffsetFactory
{
    Task<DateTimeOffset> Build(DateTime dateTime, float latitude, float longitude);
}