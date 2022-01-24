namespace GFK.Image.Cmdlets
{
    public class GoogleApiTimeZoneTimeZone
    {
        public GoogleApiTimeZoneTimeZone(double dstOffset, double rawOffset, string status, string timeZoneId, string timeZoneName, string errorMessage)
        {
            DstOffset = dstOffset;
            RawOffset = rawOffset;
            Status = status;
            TimeZoneId = timeZoneId;
            TimeZoneName = timeZoneName;
            ErrorMessage = errorMessage;
        }

        public double DstOffset { get; }
        public double RawOffset { get; }
        public string Status { get; }
        public string TimeZoneId { get; }
        public string TimeZoneName { get; }
        public string ErrorMessage { get; }
    }
}