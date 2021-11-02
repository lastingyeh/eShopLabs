using System;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace eShopLabs.Services.Location.API.Models
{
    public class UserLocation
    {
        [BsonIgnoreIfDefault]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }
        public string UserId { get; set; }
        public int LocationId { get; set; }
        public DateTime UpdateDate { get; set; }
    }
}