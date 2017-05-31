
namespace HOK.MissionControl.Core.Classes
{
    public class ProjectAddress
    {
        public string formattedAddress { get; set; } = "";
        public string street1 { get; set; } = "";
        public string street2 { get; set; } = "";
        public string city { get; set; } = "";
        public string state { get; set; } = "";
        public string country { get; set; } = "";
        public string zipCode { get; set; } = "";
        public string placeId { get; set; } = "";
        public GeometryLocation geoLocation { get; set; } = new GeometryLocation();
    }

    public class GeometryLocation
    {
        public double latitude { get; set; }
        public double longitude { get; set; }
    }
}
