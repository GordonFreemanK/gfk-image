using System;
using System.Management.Automation;
using GFK.Image.DateTimeOffsetBuilder;

namespace GFK.Image.Cmdlets
{
    [Cmdlet(VerbsCommon.Get, "DateTimeOffset")]
    [CmdletBinding(PositionalBinding = false, DefaultParameterSetName = TimeApi)]
    [OutputType(typeof(DateTimeOffset))]
    public class GetDateTimeOffsetCmdlet : PSCmdlet
    {
        private const string TimeApi = nameof(TimeApi);
        private const string GoogleApi = nameof(GoogleApi);
        private const string GeoTimeZone = nameof(GeoTimeZone);

        [Parameter(Mandatory = true, ValueFromPipelineByPropertyName = true)]
        public DateTime DateTime { get; set; }

        [Parameter(Mandatory = true, ValueFromPipelineByPropertyName = true)]
        public float Latitude { get; set; }

        [Parameter(Mandatory = true, ValueFromPipelineByPropertyName = true)]
        public float Longitude { get; set; }

        [Parameter(ParameterSetName = TimeApi)]
        [Parameter(ParameterSetName = GoogleApi)]   
        public SwitchParameter Online { get; set; }

        [Parameter(Mandatory = true, ParameterSetName = GeoTimeZone)]
        public SwitchParameter Offline { get; set; }

        [Parameter(ParameterSetName = TimeApi)]
        public Uri TimeApiUri { get; set; } = new Uri("https://www.timeapi.io");

        [Parameter(ParameterSetName = GoogleApi)]   
        public Uri GoogleApiUri { get; set; } = new Uri("https://maps.googleapis.com/maps/api/timezone/json");
        
        [Parameter(Mandatory = true, ParameterSetName = GoogleApi)]   
        public string? GoogleApiKey { get; set; }
        
        protected override void ProcessRecord()
        {
            var dateTimeOffset = GetDateTimeOffsetFactory()
                .Build(DateTime, Latitude, Longitude)
                .GetAwaiter()
                .GetResult();
            WriteObject(dateTimeOffset);
        }

        private IDateTimeOffsetFactory GetDateTimeOffsetFactory()
        {
            switch (ParameterSetName)
            {
                case TimeApi:
                    return new TimeApiDateTimeOffsetFactory(TimeApiUri);
                case GoogleApi:
                    if (GoogleApiKey == null)
                        throw new Exception("GoogleApiKey is required");
                    return new GoogleApiDateTimeOffsetFactory(GoogleApiUri, GoogleApiKey);
                case GeoTimeZone:
                    return new GeoTimeZoneDateTimeOffsetFactory();
                default:
                    throw new Exception("Cannot bind parameter set name");
            }
        }
    }
}