using System.Collections.Generic;
using System.Linq;
using MongoDB.Driver.GeoJsonObjectModel;

namespace eShopLabs.Services.Location.API.Models.Core
{
    public class LocationPolygon
    {
        public LocationPolygon()
        {

        }

        public LocationPolygon(List<GeoJson2DGeographicCoordinates> coordinatesList)
        {
            var coordinatesMapped = coordinatesList.Select(x => new List<double> { x.Longitude, x.Latitude }).ToList();
            
            Coordinates.Add(coordinatesMapped);
        }

        public string Type { get; private set; } = "Polygon";
        public List<List<List<double>>> Coordinates { get; private set; } = new List<List<List<double>>>();
    }
}