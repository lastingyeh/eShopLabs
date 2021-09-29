using System.Linq;
using System.Threading.Tasks;
using eShopLabs.Services.Basket.API.Infrastructure.Repositories;
using eShopLabs.Services.Basket.API.Models;
using Grpc.Core;
using GrpcBasket;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;
using static GrpcBasket.Basket;

namespace eShopLabs.Services.Basket.API.Grpc
{
    public class BasketService : BasketBase
    {
        private readonly IBasketRepository _repository;
        private readonly ILogger<BasketService> _logger;
        public BasketService(IBasketRepository repository, ILogger<BasketService> logger)
        {
            _logger = logger;
            _repository = repository;
        }

        [AllowAnonymous]
        public override async Task<CustomerBasketResponse> GetBasketById(BasketRequest request, ServerCallContext context)
        {
            _logger.LogInformation("Begin grpc call from method {Method} for basket id {Id}", context.Method, request.Id);

            var data = await _repository.GetBasketAsync(request.Id);

            if (data != null)
            {
                context.Status = new Status(StatusCode.OK, $"Basket with id {request.Id} do exist");

                return MapToCustomerBasketResponse(data);
            }

            context.Status = new Status(StatusCode.NotFound, $"Basket with id {request.Id} do not exist");

            return new CustomerBasketResponse();
        }

        public override async Task<CustomerBasketResponse> UpdateBasket(CustomerBasketRequest request, ServerCallContext context)
        {
            _logger.LogInformation("Begin grpc call BasketService.UpdateBasketAsync for buyer id {Buyerid}", request.Buyerid);

            var customerBasket = MapToCustomerBasket(request);
            var response = await _repository.UpdateBasketAsync(customerBasket);

            if (response != null)
            {
                return MapToCustomerBasketResponse(response);
            }

            context.Status = new Status(StatusCode.NotFound, $"Basket with id {request.Buyerid} do not exist");

            return null;
        }

        private CustomerBasket MapToCustomerBasket(CustomerBasketRequest request)
        {
            var response = new CustomerBasket { BuyerId = request.Buyerid };

            request.Items.ToList().ForEach(item => response.Items.Add(
                new BasketItem
                {
                    Id = item.Id,
                    OldUnitPrice = (decimal)item.Oldunitprice,
                    PictureUrl = item.Pictureurl,
                    ProductId = item.Productid,
                    ProductName = item.Productname,
                    Quantity = item.Quantity,
                    UnitPrice = (decimal)item.Unitprice
                }
            ));

            return response;
        }

        private CustomerBasketResponse MapToCustomerBasketResponse(CustomerBasket data)
        {
            var response = new CustomerBasketResponse { Buyerid = data.BuyerId };

            data.Items.ForEach(item => response.Items.Add(
                new BasketItemResponse
                {
                    Id = item.Id,
                    Oldunitprice = (double)item.OldUnitPrice,
                    Pictureurl = item.PictureUrl,
                    Productid = item.ProductId,
                    Productname = item.ProductName,
                    Quantity = item.Quantity,
                    Unitprice = (double)item.UnitPrice,
                }
            ));

            return response;
        }
    }
}