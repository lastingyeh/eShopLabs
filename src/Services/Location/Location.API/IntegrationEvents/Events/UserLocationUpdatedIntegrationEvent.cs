using System.Collections.Generic;
using eShopLabs.BuildingBlocks.EventBus.Events;
using eShopLabs.Services.Location.API.Models;

namespace eShopLabs.Services.Location.API.IntegrationEvents.Events
{
    public class UserLocationUpdatedIntegrationEvent : IntegrationEvent
    {
        public string UserId { get; set; }
        public List<UserLocationDetails> LocationList { get; set; }

        public UserLocationUpdatedIntegrationEvent(string userId, List<UserLocationDetails> locationList)
        {
            UserId = userId;
            locationList = LocationList;
        }
    }
}