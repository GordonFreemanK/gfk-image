using System;
using System.Management.Automation;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Web;
using GeoTimeZone;
using Newtonsoft.Json;

namespace GFK.Image.Cmdlets
{
    [Cmdlet(VerbsCommon.Get, "DateTimeOffset")]
    [CmdletBinding(PositionalBinding = false, DefaultParameterSetName = ParameterSetNameOffline)]
    [OutputType(typeof(DateTimeOffset))]
    public class GetDateTimeOffsetCmdlet : PSCmdlet
    {
        private const string ParameterSetNameOffline = "Offline";
        private const string ParameterSetNameOnline = "Online";
        private const string GoogleApiTimeZoneUrl = "https://maps.googleapis.com/maps/api/timezone/json";

        [Parameter(Mandatory = true, ValueFromPipelineByPropertyName = true)]
        public DateTime DateTime { get; set; }

        [Parameter(Mandatory = true, ValueFromPipelineByPropertyName = true)]
        public double Latitude { get; set; }

        [Parameter(Mandatory = true, ValueFromPipelineByPropertyName = true)]
        public double Longitude { get; set; }
        
        [Parameter(Mandatory = true, ParameterSetName = ParameterSetNameOnline)]
        public string? GoogleApiKey { get; set; }

        protected override void ProcessRecord()
        {
            var dateTimeOffset = ParameterSetName == ParameterSetNameOffline
                ? GetDateTimeOffsetOffline()
                : GetDateTimeOffsetOnline().GetAwaiter().GetResult();

            WriteObject(dateTimeOffset);
        }

        private DateTimeOffset GetDateTimeOffsetOffline()
        {
            var timeZoneResult = TimeZoneLookup.GetTimeZone(Latitude, Longitude);

            var timeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById(timeZoneResult.Result);

            var offset = timeZoneInfo.GetUtcOffset(DateTime);

            return new DateTimeOffset(DateTime, offset);
        }

        private async Task<DateTimeOffset> GetDateTimeOffsetOnline()
        {
            var queryString = HttpUtility.ParseQueryString(string.Empty);
            queryString.Add("location",$"{Latitude},{Longitude}");
            queryString.Add("timestamp", ((DateTimeOffset)DateTime).ToUnixTimeSeconds().ToString());
            queryString.Add("sensor", false.ToString());
            queryString.Add("key", GoogleApiKey);

            string response;
            using (var httpClient = new HttpClient())
            {
                httpClient.DefaultRequestHeaders.Accept.Clear();
                httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                response = await httpClient.GetStringAsync($"{GoogleApiTimeZoneUrl}?{queryString}");
            }

            var googleApiTimeZoneResponse =
                JsonConvert.DeserializeObject<GoogleApiTimeZoneTimeZone>(response)
                ?? throw new Exception("Error deserializing Google API response");
            if (googleApiTimeZoneResponse.Status != "OK")
                throw new Exception(
                    $"Google API Time Zone call unsuccessful: {googleApiTimeZoneResponse.ErrorMessage}");

            var offset =
                TimeSpan.FromSeconds(googleApiTimeZoneResponse.DstOffset + googleApiTimeZoneResponse.RawOffset);
            
            return new DateTimeOffset(DateTime, offset);
        }
    }
}
