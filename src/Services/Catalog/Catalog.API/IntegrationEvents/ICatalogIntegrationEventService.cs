using System.Threading.Tasks;
using eShopLabs.BuildingBlocks.EventBus.Events;

namespace eShopLabs.Services.Catalog.API.IntegrationEvents
{
    public interface ICatalogIntegrationEventService
    {
        Task SaveEventAndCatalogContextChangesAsync(IntegrationEvent evt);
        Task PublishThroughEventBusAsync(IntegrationEvent evt);
    }
}
