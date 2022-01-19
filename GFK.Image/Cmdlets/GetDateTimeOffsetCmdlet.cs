using System;
using System.Management.Automation;
using GeoTimeZone;

namespace GFK.Image.Cmdlets
{
    [Cmdlet(VerbsCommon.Get, "DateTimeOffset")]
    [OutputType(typeof(DateTimeOffset))]
    public class GetDateTimeOffsetCmdlet : PSCmdlet
    {
        [Parameter(Mandatory = true, Position = 0)]
        public DateTime DateTime { get; set; }

        [Parameter(Mandatory = true, Position = 1)]
        public double Latitude { get; set; }

        [Parameter(Mandatory = true, Position = 2)]
        public double Longitude { get; set; }

        protected override void ProcessRecord()
        {
            var timeZoneResult = TimeZoneLookup.GetTimeZone(Latitude, Longitude);

            var timeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById(timeZoneResult.Result);

            var offset = timeZoneInfo.GetUtcOffset(DateTime);

            var dateTimeOffset = new DateTimeOffset(DateTime, offset);

            WriteObject(dateTimeOffset);
        }
    }
}
