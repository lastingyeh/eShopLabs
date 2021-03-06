using System.Collections.Generic;
using eShopLabs.BuildingBlocks.EventBus.Events;

namespace eShopLabs.Services.Catalog.API.IntegrationEvents.Events
{
    public class OrderStatusChangedToAwaitingValidationIntegrationEvent : IntegrationEvent
    {
        public int OrderId { get; }
        public IEnumerable<OrderStockItem> OrderStockItems { get; }
        public OrderStatusChangedToAwaitingValidationIntegrationEvent(int orderId, IEnumerable<OrderStockItem> orderStockItems)
        {
            OrderId = orderId;
            OrderStockItems = orderStockItems;
        }
    }

    public class OrderStockItem
    {
        public int ProductId { get; }
        public int Units { get; }
        public OrderStockItem(int productId, int units)
        {
            ProductId = productId;
            Units = units;
        }
    }
}
