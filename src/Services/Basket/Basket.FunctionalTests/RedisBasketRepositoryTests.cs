using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Basket.FunctionalTests.Base;
using eShopLabs.Services.Basket.API.Infrastructure.Repositories;
using eShopLabs.Services.Basket.API.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;
using Xunit;

namespace Basket.FunctionalTests
{
    public class RedisBasketRepositoryTests : BasketScenarioBase
    {
        [Fact]
        public async Task Update_Basket_Return_Add_Basket()
        {
            using var server = CreateServer();

            var redis = server.Host.Services.GetRequiredService<ConnectionMultiplexer>();
            var redisBasketRepository = BuildBasketRepository(redis);

            var basket = await redisBasketRepository.UpdateBasketAsync(new CustomerBasket("customerId")
            {
                BuyerId = "buyerId",
                Items = BuildBasketItems(),
            });

            Assert.NotNull(basket);
            Assert.Single(basket.Items);
        }

        [Fact]
        public async Task Delete_Basket_Return_Null()
        {
            using var server = CreateServer();

            var redis = server.Services.GetRequiredService<ConnectionMultiplexer>();
            var redisBasketRepository = BuildBasketRepository(redis);

            var basket = await redisBasketRepository.UpdateBasketAsync(new CustomerBasket("customerId")
            {
                BuyerId = "buyerId",
                Items = BuildBasketItems(),
            });

            var deleteResult = await redisBasketRepository.DeleteBasketAsync("buyerId");
            var result = await redisBasketRepository.GetBasketAsync(basket.BuyerId);

            Assert.True(deleteResult);
            Assert.Null(result);
        }

        private List<BasketItem> BuildBasketItems() =>
            new List<BasketItem>
            {
                new BasketItem
                {
                    Id = "basketId",
                    PictureUrl = "pictureurl",
                    ProductId = 1,
                    ProductName = "productName",
                    Quantity = 1,
                    UnitPrice = 1,
                }
            };

        private RedisBasketRepository BuildBasketRepository(ConnectionMultiplexer redis)
        {
            var loggerFactory = new LoggerFactory();

            return new RedisBasketRepository(loggerFactory, redis);
        }
    }
}