using System.Collections.Generic;
using System.Threading.Tasks;
using eShopLabs.Services.Basket.API.Models;

namespace eShopLabs.Services.Basket.API.Infrastructure.Repositories
{
    public interface IBasketRepository
    {
        Task<CustomerBasket> GetBasketAsync(string customerId);
        IEnumerable<string> GetUsers();
        Task<CustomerBasket> UpdateBasketAsync(CustomerBasket basket);
        Task<bool> DeleteBasketAsync(string id);
    }
}