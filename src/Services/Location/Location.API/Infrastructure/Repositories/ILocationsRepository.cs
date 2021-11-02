using System.Collections.Generic;
using System.Threading.Tasks;
using eShopLabs.Services.Location.API.Models;
using eShopLabs.Services.Location.API.ViewModel;

namespace eShopLabs.Services.Location.API.Infrastructure.Repositories
{
    public interface ILocationsRepository
    {
        Task<Locations> GetAsync(int locationId);
        Task<List<Locations>> GetLocationListAsync();
        Task<UserLocation> GetUserLocationAsync(string userId);
        Task<List<Locations>> GetCurrentUserRegionsListAsync(LocationRequest currentPosition);
        Task AddUserLocationAsync(UserLocation location);
        Task UpdateUserLocationAsync(UserLocation userLocation);
    }
}