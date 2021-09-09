using eShopLabs.BuildingBlocks.EventBus.Events;

namespace eShopLabs.Services.Catalog.API.IntegrationEvents.Events
{
    public class ProductPriceChangedIntegrationEvent : IntegrationEvent
    {
        public ProductPriceChangedIntegrationEvent(int productId, decimal newPrice, decimal oldPrice)
        {
            ProductId = productId;
            NewPrice = newPrice;
            OldPrice = oldPrice;
        }
        public int ProductId { get; set; }
        public decimal NewPrice { get; set; }
        public decimal OldPrice { get; set; }
    }
}