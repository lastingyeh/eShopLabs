using Microsoft.eShopOnContainers.BuildingBlocks.EventBus.Events;

namespace Catalog.API.IntegrationEvents.Events
{
    public class OrderStockConfirmedIntegrationEvent : IntegrationEvent
    {
        public int OrderId { get; }
        public OrderStockConfirmedIntegrationEvent(int orderId) => OrderId = orderId;
    }
}
