using Newtonsoft.Json;

namespace HOK.MissionControl.Core.Schemas
{
    /// <summary>
    /// 
    /// </summary>
    public class ProjectAddress
    {
        [JsonProperty("formattedAddress")]
        public string FormattedAddress { get; set; }

        [JsonProperty("street1")]
        public string Street1 { get; set; }

        [JsonProperty("street2")]
        public string Street2 { get; set; }

        [JsonProperty("city")]
        public string City { get; set; }

        [JsonProperty("state")]
        public string State { get; set; }

        [JsonProperty("country")]
        public string Country { get; set; }

        [JsonProperty("zipCode")]
        public string ZipCode { get; set; }

        [JsonProperty("placeId")]
        public string PlaceId { get; set; }

        [JsonProperty("geoLocation")]
        public GeometryLocation GeoLocation { get; set; } = new GeometryLocation();
    }

    /// <summary>
    /// 
    /// </summary>
    public class GeometryLocation
    {
        [JsonProperty("latitude")]
        public double Latitude { get; set; }

        [JsonProperty("longitude")]
        public double Longitude { get; set; }
    }
}
