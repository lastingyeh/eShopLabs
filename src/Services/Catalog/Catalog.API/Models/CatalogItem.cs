using System;
using eShopLabs.Services.Catalog.API.Infrastructure.Exceptions;

namespace eShopLabs.Services.Catalog.API.Models
{
    public class CatalogItem
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public decimal Price { get; set; }
        public string PictureFileName { get; set; }
        public string PictureUri { get; set; }
        public int CatalogTypeId { get; set; }
        public CatalogType CatalogType { get; set; }
        public int CatalogBrandId { get; set; }
        public CatalogBrand CatalogBrand { get; set; }
        // Quantity in stock
        public int AvailableStock { get; set; }
        // Available stock at which we should reorder
        public int RestockThreshold { get; set; }
        // Maximum number of units that can be in-stock at any time (due to physicial/logistical constraints in warehouses)
        public int MaxStockThreshold { get; set; }
        /// <summary>
        /// True if item is on reorder
        /// </summary>
        public bool OnReorder { get; set; }
        public CatalogItem() { }

        public int RemoveStock(int quantityDesired)
        {
            if (AvailableStock == 0)
            {
                throw new CatalogDomainException($"Empty stock, product item {Name} is sold out");
            }

            if (quantityDesired <= 0)
            {
                throw new CatalogDomainException($"Item units desired should be greater than zero");
            }

            int removed = Math.Min(quantityDesired, this.AvailableStock);

            this.AvailableStock -= removed;

            return removed;
        }

        public int AddStock(int quantity)
        {
            var original = AvailableStock;

            if (AvailableStock + quantity > MaxStockThreshold)
            {
                AvailableStock += (MaxStockThreshold - AvailableStock);
            }
            else
            {
                AvailableStock += quantity;
            }

            OnReorder = false;

            return AvailableStock - original;
        }
    }
}
