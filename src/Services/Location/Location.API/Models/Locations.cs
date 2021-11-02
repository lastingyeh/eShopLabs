using System;
using System.Collections.Generic;
using eShopLabs.Services.Location.API.Models.Core;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver.GeoJsonObjectModel;

namespace eShopLabs.Services.Location.API.Models
{
    public class Locations
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }
        public int LocationId { get; set; }
        public string Code { get; set; }
        [BsonRepresentation(BsonType.ObjectId)]
        public string Parent_Id { get; set; }
        public string Description { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public LocationPoint Location { get; private set; }
        public LocationPolygon Polygon { get; private set; }

        public void SetLocation(double lon, double lat) => SetPosition(lon, lat);
        public void SetArea(List<GeoJson2DGeographicCoordinates> coordinatesList) => SetPolygon(coordinatesList);
        
        private void SetPosition(double lon, double lat)
        {
            Latitude = lat;
            Longitude = lon;

            Location = new LocationPoint(lon, lat);
        }
        private void SetPolygon(List<GeoJson2DGeographicCoordinates> coordinatesList)
        {
            Polygon = new LocationPolygon(coordinatesList);
        }
    }
}