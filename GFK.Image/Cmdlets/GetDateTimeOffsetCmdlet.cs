using System;
using System.Management.Automation;
using GeoTimeZone;

namespace GFK.Image.Cmdlets
{
    [Cmdlet(VerbsCommon.Get, "DateTimeOffset")]
    [CmdletBinding(PositionalBinding = false)]
    [OutputType(typeof(DateTimeOffset))]
    public class GetDateTimeOffsetCmdlet : PSCmdlet
    {
        [Parameter(Mandatory = true, ValueFromPipelineByPropertyName = true)]
        public DateTime DateTime { get; set; }

        [Parameter(Mandatory = true, ValueFromPipelineByPropertyName = true)]
        public double Latitude { get; set; }

        [Parameter(Mandatory = true, ValueFromPipelineByPropertyName = true)]
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
