using System;
using System.Management.Automation;
using GFK.Image.DateTimeOffsetBuilder;

namespace GFK.Image.Cmdlets
{
    [Cmdlet(VerbsCommon.Get, "DateTimeOffset")]
    [CmdletBinding(PositionalBinding = false, DefaultParameterSetName = "TimeApi")]
    [OutputType(typeof(DateTimeOffset))]
    public class GetDateTimeOffsetCmdlet : PSCmdlet, IDynamicParameters
    {
        [Parameter(Mandatory = true, ValueFromPipelineByPropertyName = true)]
        public DateTime DateTime { get; set; }

        [Parameter(Mandatory = true, ValueFromPipelineByPropertyName = true)]
        public float Latitude { get; set; }

        [Parameter(Mandatory = true, ValueFromPipelineByPropertyName = true)]
        public float Longitude { get; set; }

        [Parameter(ParameterSetName = "TimeApi")]
        [Parameter(ParameterSetName = "GoogleApi")]
        [Parameter(ParameterSetName = "GeoTimeZone")]
        public GetDateTimeOffsetMethod Method { get; set; } = GetDateTimeOffsetMethod.TimeApi;

        private TimeApiParameters? _timeApiParameters;
        private GoogleApiParameters? _googleApiParameters;

        public object? GetDynamicParameters()
        {
            return Method switch
            {
                GetDateTimeOffsetMethod.TimeApi => _timeApiParameters = new TimeApiParameters(),
                GetDateTimeOffsetMethod.GoogleApi => _googleApiParameters = new GoogleApiParameters(),
                _ => null
            };
        }

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
            switch (Method)
            {
                case GetDateTimeOffsetMethod.GeoTimeZone:
                    return new GeoTimeZoneDateTimeOffsetFactory();
                case GetDateTimeOffsetMethod.TimeApi:
                    if (_timeApiParameters == null)
                        throw new Exception("Could not bind Time API parameters");
                    return new TimeApiDateTimeOffsetFactory(_timeApiParameters.Uri);
                case GetDateTimeOffsetMethod.GoogleApi:
                    if (_googleApiParameters == null)
                        throw new Exception("Could not bind Google API parameters");
                    if (_googleApiParameters.Key == null)
                        throw new Exception("Could not bind Google API key");
                    return new GoogleApiDateTimeOffsetFactory(_googleApiParameters.Uri, _googleApiParameters.Key);
                default:
                    throw new ArgumentOutOfRangeException(nameof(Method));
            }
        }

        private class ApiParameters
        {
            [Parameter(ParameterSetName = "TimeApi")]
            [Parameter(ParameterSetName = "GoogleApi")]
            public Uri Uri { get; set; } = new Uri("https://www.timeapi.io");
        }

        private class TimeApiParameters : ApiParameters
        {
            public TimeApiParameters()
            {
                Uri = new Uri("https://www.timeapi.io");
            }
        }

        private class GoogleApiParameters : ApiParameters
        {
            public GoogleApiParameters()
            {
                Uri = new Uri("https://maps.googleapis.com/maps/api/timezone/json");
            }
            
            [Parameter(Mandatory = true, ParameterSetName = "GoogleApi")]
            public string? Key { get; set; }
        }
    }
}
