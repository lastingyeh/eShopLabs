using System;
using System.Collections.Generic;
using System.Net;
using System.Security.Claims;
using System.Threading.Tasks;
using eShopLabs.BuildingBlocks.EventBus.Abstractions;
using eShopLabs.Services.Basket.API.Controllers;
using eShopLabs.Services.Basket.API.Infrastructure.Repositories;
using eShopLabs.Services.Basket.API.IntegrationEvents.Events;
using eShopLabs.Services.Basket.API.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using IBasketIdentityService = eShopLabs.Services.Basket.API.Services.IIdentityService;

namespace Basket.UnitTests.Application
{
    public class BasketWebApiTest
    {
        private readonly Mock<IBasketRepository> _basketRepositoryMock;
        private readonly Mock<IBasketIdentityService> _identityServiceMock;
        private readonly Mock<IEventBus> _serviceBusMock;
        private readonly Mock<ILogger<BasketController>> _loggerMock;

        public BasketWebApiTest()
        {
            _basketRepositoryMock = new Mock<IBasketRepository>();
            _identityServiceMock = new Mock<IBasketIdentityService>();
            _serviceBusMock = new Mock<IEventBus>();
            _loggerMock = new Mock<ILogger<BasketController>>();
        }

        [Fact]
        public async Task Get_Customer_Basket_Success()
        {
            // setup
            var fakeCustomerId = "1";
            var fakeCustomerBasket = GetCustomerBasketFake(fakeCustomerId);

            _basketRepositoryMock.Setup(x => x.GetBasketAsync(It.IsAny<string>()))
                .Returns(Task.FromResult(fakeCustomerBasket));

            _identityServiceMock.Setup(x => x.GetUserIdentity())
                .Returns(fakeCustomerId);

            _serviceBusMock.Setup(x => x.Publish(It.IsAny<UserCheckoutAcceptedIntegrationEvent>()));

            // act
            var basketController = new BasketController(
                _loggerMock.Object,
                _basketRepositoryMock.Object,
                _identityServiceMock.Object,
                _serviceBusMock.Object);

            // action
            var actionResult = await basketController.GetBasketByIdAsync(fakeCustomerId);

            // assert
            Assert.Equal((actionResult.Result as OkObjectResult).StatusCode, (int)HttpStatusCode.OK);
            Assert.Equal((((ObjectResult)actionResult.Result).Value as CustomerBasket).BuyerId, fakeCustomerId);
        }

        [Fact]
        public async Task Post_Customer_Basket_Success()
        {
            // setup
            var fakeCustomerId = "1";
            var fakeCustomerBasket = GetCustomerBasketFake(fakeCustomerId);

            _basketRepositoryMock.Setup(x => x.UpdateBasketAsync(It.IsAny<CustomerBasket>()))
                .Returns(Task.FromResult(fakeCustomerBasket));

            _identityServiceMock.Setup(x => x.GetUserIdentity()).Returns(fakeCustomerId);

            _serviceBusMock.Setup(x => x.Publish(It.IsAny<UserCheckoutAcceptedIntegrationEvent>()));

            // act
            var basketController = new BasketController(
                _loggerMock.Object,
                _basketRepositoryMock.Object,
                _identityServiceMock.Object,
                _serviceBusMock.Object);

            var actionResult = await basketController.UpdateBasketAsync(fakeCustomerBasket);

            // assert
            Assert.Equal((actionResult.Result as OkObjectResult).StatusCode, (int)HttpStatusCode.OK);
            Assert.Equal((((ObjectResult)actionResult.Result).Value as CustomerBasket).BuyerId, fakeCustomerId);
        }

        [Fact]
        public async Task Doing_Checkout_Without_Basket_Should_Return_Bad_Request()
        {
            // setup
            var fakeCustomerId = "2";

            _basketRepositoryMock.Setup(x => x.GetBasketAsync(It.IsAny<string>()))
                .Returns(Task.FromResult((CustomerBasket)null));

            _identityServiceMock.Setup(x => x.GetUserIdentity()).Returns(fakeCustomerId);

            // act
            var basketController = new BasketController(
                _loggerMock.Object,
                _basketRepositoryMock.Object,
                _identityServiceMock.Object,
                _serviceBusMock.Object);

            var result = await basketController.CheckoutAsync(new BasketCheckout(), Guid.NewGuid().ToString()) as BadRequestResult;

            // assert
            Assert.NotNull(result);
        }

        [Fact]
        public async Task Doing_Checkout_With_Basket_Should_Publish_UserCheckoutAccepted_Integration_Event()
        {
            // setup
            var fakeCustomerId = "1";
            var fakeCustomerBasket = GetCustomerBasketFake(fakeCustomerId);

            _basketRepositoryMock.Setup(x => x.GetBasketAsync(It.IsAny<string>()))
                .Returns(Task.FromResult(fakeCustomerBasket));

            _identityServiceMock.Setup(x => x.GetUserIdentity()).Returns(fakeCustomerId);

            var basketController = new BasketController(
                _loggerMock.Object,
                _basketRepositoryMock.Object,
                _identityServiceMock.Object,
                _serviceBusMock.Object);

            basketController.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = new ClaimsPrincipal(
                        new ClaimsIdentity(
                            new Claim[]
                            {
                                new Claim("sub", "testuser"),
                                new Claim("unique_name", "testuser"),
                                new Claim(ClaimTypes.Name, "testuser"),
                            }
                        )
                    )
                }
            };

            // act
            var result = await basketController.CheckoutAsync(new BasketCheckout(), Guid.NewGuid().ToString()) as AcceptedResult;

            _serviceBusMock.Verify(m => m.Publish(It.IsAny<UserCheckoutAcceptedIntegrationEvent>()), Times.Once);
            // assert

            Assert.NotNull(result);
        }

        private CustomerBasket GetCustomerBasketFake(string fakeCustomerId) =>
            new CustomerBasket(fakeCustomerId)
            {
                Items = new List<BasketItem> { new BasketItem() }
            };
    }
}