using eShopLabs.Services.Location.API.Models;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace eShopLabs.Services.Location.API.Infrastructure
{
    public class LocationsContext
    {
        private readonly IMongoDatabase _database = null;
        public LocationsContext(IOptions<LocationSettings> settings)
        {
            var client = new MongoClient(settings.Value.ConnectionString);

            if (client != null)
            {
                _database = client.GetDatabase(settings.Value.Database);
            }
        }

        public IMongoCollection<UserLocation> UserLocation { get => _database.GetCollection<UserLocation>("UserLocation"); }
        public IMongoCollection<Locations> Locations { get => _database.GetCollection<Locations>("Locations"); }
    }
}