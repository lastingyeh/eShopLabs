using eShopLabs.BuildingBlocks.EventBus.Events;

namespace eShopLabs.Services.Catalog.API.IntegrationEvents.Events
{
    public class OrderStockConfirmedIntegrationEvent : IntegrationEvent
    {
        public int OrderId { get; }
        public OrderStockConfirmedIntegrationEvent(int orderId) => OrderId = orderId;
    }
}
