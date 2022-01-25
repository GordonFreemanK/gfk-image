using System;
using System.Management.Automation;
using System.Net.Http;
using System.Threading.Tasks;
using GeoTimeZone;
using GFK.Time.API;

namespace GFK.Image.Cmdlets
{
    [Cmdlet(VerbsCommon.Get, "DateTimeOffset")]
    [CmdletBinding(PositionalBinding = false, DefaultParameterSetName = "Online")]
    [OutputType(typeof(DateTimeOffset))]
    public class GetDateTimeOffsetCmdlet : PSCmdlet
    {
        [Parameter(Mandatory = true, ValueFromPipelineByPropertyName = true)]
        public DateTime DateTime { get; set; }

        [Parameter(Mandatory = true, ValueFromPipelineByPropertyName = true)]
        public float Latitude { get; set; }

        [Parameter(Mandatory = true, ValueFromPipelineByPropertyName = true)]
        public float Longitude { get; set; }
        
        [Parameter(ParameterSetName = "Online")]
        public Uri TimeApiUri { get; set; } = new Uri("https://www.timeapi.io");

        [Parameter(Mandatory = true, ParameterSetName = "Offline")]
        public SwitchParameter Offline { get; set; }

        protected override void ProcessRecord()
        {
            var dateTimeOffset = this.Offline
                ? GetDateTimeOffsetOffline()
                : GetDateTimeOffsetOnline().GetAwaiter().GetResult();

            WriteObject(dateTimeOffset);
        }

        private async Task<DateTimeOffset> GetDateTimeOffsetOnline()
        {
            using var httpClient = new HttpClient {BaseAddress = TimeApiUri};

            var timeApiClient = new TimeApiClient(httpClient);

            var currentTime = await timeApiClient.CoordinateAsync(Latitude, Longitude);
            if (currentTime.TimeZone == null)
                throw new Exception($"Could not get timezone for latitude {Latitude} and longitude {Longitude}");

            var convertRequest = new ConvertRequest(
                DateTime.ToString("yyyy-MM-dd HH:mm:ss"),
                null,
                currentTime.TimeZone,
                "UTC");
            var conversion = await timeApiClient.ConvertTimeZoneAsync(convertRequest);
            
            var offset = DateTime - conversion.ConversionResult.DateTime;

            return new DateTimeOffset(DateTime, offset);
        }

        private DateTimeOffset GetDateTimeOffsetOffline()
        {
            var timeZoneResult = TimeZoneLookup.GetTimeZone(Latitude, Longitude);

            var timeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById(timeZoneResult.Result);

            var offset = timeZoneInfo.GetUtcOffset(DateTime);

            return new DateTimeOffset(DateTime, offset);
        }
    }
}
