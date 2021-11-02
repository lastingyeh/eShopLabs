using System.Collections.Generic;

namespace eShopLabs.Services.Location.API.Models.Core
{
    public class LocationPoint
    {
        public LocationPoint()
        {

        }

        public LocationPoint(double longitude, double latitude)
        {
            Coordinates.Add(longitude);
            Coordinates.Add(latitude);
        }

        public string Type { get; private set; } = "Point";
        public List<double> Coordinates { get; private set; } = new List<double>();
    }
}