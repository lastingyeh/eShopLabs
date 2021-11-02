using System.Collections.Generic;
using System.Threading.Tasks;
using eShopLabs.Services.Location.API.Models;
using eShopLabs.Services.Location.API.ViewModel;

namespace eShopLabs.Services.Location.API.Infrastructure.Services
{
    public interface ILocationsService
    {
        Task<Locations> GetLocationAsync(int locationId);
        Task<UserLocation> GetUserLocationAsync(string id);
        Task<List<Locations>> GetAllLocationAsync();
        Task<bool> AddOrUpdateUserLocationAsync(string userId, LocationRequest locRequest);
    }
}