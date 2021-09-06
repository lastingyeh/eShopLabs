using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using eShopLabs.BuildingBlocks.EventBus.Abstractions;
using eShopLabs.BuildingBlocks.EventBus.Events;
using eShopLabs.Services.Catalog.API.Infrastructure;
using eShopLabs.Services.Catalog.API.IntegrationEvents.Events;
using Microsoft.Extensions.Logging;
using Serilog.Context;

namespace eShopLabs.Services.Catalog.API.IntegrationEvents.EventHandling
{
    public class OrderStatusChangedToAwaitingValidationIntegrationEventHandler :
        IIntegrationEventHandler<OrderStatusChangedToAwaitingValidationIntegrationEvent>
    {
        private readonly CatalogContext _catalogContext;
        private readonly ICatalogIntegrationEventService _catalogIntegrationEventService;
        private readonly ILogger<OrderStatusChangedToAwaitingValidationIntegrationEventHandler> _logger;
        public OrderStatusChangedToAwaitingValidationIntegrationEventHandler(
            CatalogContext catalogContext,
            ICatalogIntegrationEventService catalogIntegrationEventService,
            ILogger<OrderStatusChangedToAwaitingValidationIntegrationEventHandler> logger)
        {
            _logger = logger;
            _catalogIntegrationEventService = catalogIntegrationEventService;
            _catalogContext = catalogContext;

        }
        public async Task Handle(OrderStatusChangedToAwaitingValidationIntegrationEvent @event)
        {
            using (LogContext.PushProperty("IntegrationEventContext", $"{@event.Id}-{Program.AppName}"))
            {
                _logger.LogInformation("----- Handling integration event: {IntegrationEventId} at {AppName} - ({@IntegrationEvent})",
                    @event.Id, Program.AppName, @event);

                var confirmedOrderStockItems = new List<ConfirmedOrderStockItem>();

                foreach (var orderStockItem in @event.OrderStockItems)
                {
                    var catalogItem = _catalogContext.CatalogItems.Find(orderStockItem.ProductId);
                    var hasStock = catalogItem.AvailableStock >= orderStockItem.Units;
                    var confirmedOrderStockItem = new ConfirmedOrderStockItem(catalogItem.Id, hasStock);

                    confirmedOrderStockItems.Add(confirmedOrderStockItem);
                }

                var confirmedIntegrationEvent = confirmedOrderStockItems.Any(c => !c.HasStock) ?
                    (IntegrationEvent)new OrderStockRejectedIntegrationEvent(@event.OrderId, confirmedOrderStockItems) :
                    new OrderStockConfirmedIntegrationEvent(@event.OrderId);

                await _catalogIntegrationEventService.SaveEventAndCatalogContextChangesAsync(confirmedIntegrationEvent);
                await _catalogIntegrationEventService.PublishThroughEventBusAsync(confirmedIntegrationEvent);
            }
        }
    }
}
