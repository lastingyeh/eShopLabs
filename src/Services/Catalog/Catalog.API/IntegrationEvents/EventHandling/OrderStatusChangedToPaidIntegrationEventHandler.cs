using System.Threading.Tasks;
using eShopLabs.BuildingBlocks.EventBus.Abstractions;
using Microsoft.Extensions.Logging;
using Serilog.Context;
using eShopLabs.Services.Catalog.API.Infrastructure;
using eShopLabs.Services.Catalog.API.IntegrationEvents.Events;

namespace eShopLabs.Services.Catalog.API.IntegrationEvents.EventHandling
{
    public class OrderStatusChangedToPaidIntegrationEventHandler :
        IIntegrationEventHandler<OrderStatusChangedToPaidIntegrationEvent>
    {
        private readonly CatalogContext _catalogContext;
        private readonly ILogger<OrderStatusChangedToPaidIntegrationEventHandler> _logger;

        public OrderStatusChangedToPaidIntegrationEventHandler(
            CatalogContext catalogContext,
            ILogger<OrderStatusChangedToPaidIntegrationEventHandler> logger)
        {
            _catalogContext = catalogContext;
            _logger = logger;
        }
        public async Task Handle(OrderStatusChangedToPaidIntegrationEvent @event)
        {
            using (LogContext.PushProperty("IntegrationEventContext", $"{@event.Id}-{Program.AppName}"))
            {
                _logger.LogInformation("----- Handling integration event: {IntegrationEventId} at {AppName} - ({@IntegrationEvent})", @event.Id, Program.AppName, @event);

                //we're not blocking stock/inventory
                foreach (var orderStockItem in @event.OrderStockItems)
                {
                    var catalogItem = _catalogContext.CatalogItems.Find(orderStockItem.ProductId);

                    catalogItem.RemoveStock(orderStockItem.Units);
                }

                await _catalogContext.SaveChangesAsync();
            }
        }
    }
}
