using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using eShopLabs.BuildingBlocks.EventBus.Abstractions;
using eShopLabs.Services.Location.API.Infrastructure.Exceptions;
using eShopLabs.Services.Location.API.Infrastructure.Repositories;
using eShopLabs.Services.Location.API.IntegrationEvents.Events;
using eShopLabs.Services.Location.API.Models;
using eShopLabs.Services.Location.API.ViewModel;
using Microsoft.Extensions.Logging;

namespace eShopLabs.Services.Location.API.Infrastructure.Services
{
    public class LocationsService : ILocationsService
    {
        private readonly ILocationsRepository _locationsRepository;
        private readonly IEventBus _eventBus;
        private readonly ILogger<LocationsService> _logger;

        public LocationsService(ILocationsRepository locationsRepository, IEventBus eventBus, ILogger<LocationsService> logger)
        {
            _eventBus = eventBus;
            _logger = logger;
            _locationsRepository = locationsRepository;
        }
        public async Task<bool> AddOrUpdateUserLocationAsync(string userId, LocationRequest currentPosition)
        {
            var currentUserAreaLocationList = await _locationsRepository.GetCurrentUserRegionsListAsync(currentPosition);

            if (currentUserAreaLocationList is null)
            {
                throw new LocationDomainException("User current area not found");
            }

            var locationAncestors = new List<string>();
            var userLocation = await _locationsRepository.GetUserLocationAsync(userId);

            userLocation = userLocation ?? new UserLocation();

            userLocation.UserId = userId;
            userLocation.LocationId = currentUserAreaLocationList[0].LocationId;
            userLocation.UpdateDate = DateTime.UtcNow;

            await _locationsRepository.UpdateUserLocationAsync(userLocation);

            // Publish integration event to update marketing read data model
            // with the new locations updated
            PublishNewUserLocationPositionIntegrationEvent(userId, currentUserAreaLocationList);

            return true;
        }

        public Task<List<Locations>> GetAllLocationAsync()
        {
            return _locationsRepository.GetLocationListAsync();
        }

        public Task<Locations> GetLocationAsync(int locationId)
        {
            return _locationsRepository.GetAsync(locationId);
        }

        public Task<UserLocation> GetUserLocationAsync(string userId)
        {
            return _locationsRepository.GetUserLocationAsync(userId);
        }

        private void PublishNewUserLocationPositionIntegrationEvent(string userId, List<Locations> newLocations)
        {
            var newUserLocations = MapUserLocationDetails(newLocations);
            var @event = new UserLocationUpdatedIntegrationEvent(userId, newUserLocations);

            _logger.LogInformation("----- Publishing integration event: {IntegrationEventId} from {AppName} - ({@IntegrationEvent})", @event.Id, Program.AppName, @event);

            _eventBus.Publish(@event);
        }

        private List<UserLocationDetails> MapUserLocationDetails(List<Locations> newLocations)
        {
            var result = new List<UserLocationDetails>();

            newLocations.ForEach(location =>
            {
                result.Add(new UserLocationDetails
                {
                    LocationId = location.LocationId,
                    Code = location.Code,
                    Description = location.Description,
                });
            });

            return result;
        }
    }
}