using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HOK.MissionControl.Classes
{
    public class ProjectAddress
    {
        private string formattedAddressVal = "";
        private string street1Val = "";
        private string street2Val = "";
        private string cityVal = "";
        private string stateVal = "";
        private string countryVal = "";
        private string zipCodeVal = "";
        private string placeIdVal = "";
        private GeometryLocation geoLocationVal = new GeometryLocation();

        public string formattedAddress { get { return formattedAddressVal; } set { formattedAddressVal = value;  } }
        public string street1 { get { return street1Val; } set { street1Val = value; } }
        public string street2 { get { return street2Val; } set { street2Val = value; } }
        public string city { get { return cityVal; } set { cityVal = value; } }
        public string state { get { return stateVal; } set { stateVal = value; } }
        public string country { get { return countryVal; } set { countryVal = value; } }
        public string zipCode { get { return zipCodeVal; } set { zipCodeVal = value; } }
        public string placeId { get { return placeIdVal; } set { placeIdVal = value; } }
        public GeometryLocation geoLocation { get { return geoLocationVal; } set { geoLocationVal = value; } }

    }

    public class GeometryLocation 
    {
        private double latitudeVal = 0;
        private double longitudeVal = 0;

        public double latitude { get { return latitudeVal; } set { latitudeVal = value; } }
        public double longitude { get { return longitudeVal; } set { longitudeVal = value; } }

        public GeometryLocation()
        {
        }

       
    }
}
