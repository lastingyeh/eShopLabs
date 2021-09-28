using eShopLabs.BuildingBlocks.EventBus.Events;

namespace eShopLabs.Services.Basket.API.IntegrationEvents.Events
{
    public class OrderStartedIntegrationEvent : IntegrationEvent
    {
        public string UserId { get; set; }
        public OrderStartedIntegrationEvent(string userId) => UserId = userId;
    }
}