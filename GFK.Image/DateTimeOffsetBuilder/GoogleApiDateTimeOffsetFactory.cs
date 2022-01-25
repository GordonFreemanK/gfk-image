using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Web;
using Newtonsoft.Json;

namespace GFK.Image.DateTimeOffsetBuilder
{
    internal class GoogleApiDateTimeOffsetFactory : IDateTimeOffsetFactory
    {
        private readonly Uri _uri;
        private readonly string _key;

        public GoogleApiDateTimeOffsetFactory(Uri uri, string key)
        {
            _uri = uri;
            _key = key;
        }
        
        public async Task<DateTimeOffset> Build(DateTime dateTime, float latitude, float longitude)
        {
            var queryString = HttpUtility.ParseQueryString(string.Empty);
            queryString.Add("location",$"{latitude},{longitude}");
            queryString.Add("timestamp", ((DateTimeOffset)dateTime).ToUnixTimeSeconds().ToString());
            queryString.Add("sensor", false.ToString());
            queryString.Add("key", _key);

            string response;
            using (var httpClient = new HttpClient())
            {
                httpClient.DefaultRequestHeaders.Accept.Clear();
                httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                response = await httpClient.GetStringAsync($"{_uri}?{queryString}");
            }

            var timeZone = new
            {
                Status = default(string),
                DstOffset = default(double),
                RawOffset = default(double),
                ErrorMessage = default(string)
            };
            var googleApiTimeZoneResponse =
                JsonConvert.DeserializeAnonymousType(response, timeZone)
                ?? throw new Exception("Error deserializing Google API response");
            if (googleApiTimeZoneResponse.Status != "OK")
                throw new Exception(
                    $"Google API Time Zone call unsuccessful: {googleApiTimeZoneResponse.ErrorMessage}");

            var offset =
                TimeSpan.FromSeconds(googleApiTimeZoneResponse.DstOffset + googleApiTimeZoneResponse.RawOffset);
            
            return new DateTimeOffset(dateTime, offset);
        }
    }
}