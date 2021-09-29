using System;
using System.Net;
using System.Security.Claims;
using System.Threading.Tasks;
using eShopLabs.BuildingBlocks.EventBus.Abstractions;
using eShopLabs.Services.Basket.API.Infrastructure.Repositories;
using eShopLabs.Services.Basket.API.IntegrationEvents.Events;
using eShopLabs.Services.Basket.API.Models;
using eShopLabs.Services.Basket.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace eShopLabs.Services.Basket.API.Controllers
{
    [Route("api/v1/[controller]")]
    [Authorize]
    [ApiController]
    public class BasketController : ControllerBase
    {
        private readonly ILogger<BasketController> _logger;
        private readonly IBasketRepository _repository;
        private readonly IIdentityService _identityService;
        private readonly IEventBus _eventBus;
        public BasketController(
            ILogger<BasketController> logger,
            IBasketRepository repository,
            IIdentityService identityService,
            IEventBus eventBus)
        {
            _eventBus = eventBus;
            _identityService = identityService;
            _repository = repository;
            _logger = logger;
        }

        [HttpGet("{id}")]
        [ProducesResponseType(typeof(CustomerBasket), (int)HttpStatusCode.OK)]
        public async Task<ActionResult<CustomerBasket>> GetBasketByIdAsync(string id)
        {
            var basket = await _repository.GetBasketAsync(id);

            return Ok(basket ?? new CustomerBasket(id));
        }

        [HttpPost]
        [ProducesResponseType(typeof(CustomerBasket), (int)HttpStatusCode.OK)]
        public async Task<ActionResult<CustomerBasket>> UpdateBasketAsync([FromBody] CustomerBasket value)
        {
            return Ok(await _repository.UpdateBasketAsync(value));
        }

        [Route("checkout")]
        [HttpPost]
        [ProducesResponseType((int)HttpStatusCode.Accepted)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        public async Task<ActionResult> CheckoutAsync(
            [FromBody] BasketCheckout basketCheckout,
            [FromHeader(Name = "x-requestid")] string requestId)
        {
            var userId = _identityService.GetUserIdentity();

            basketCheckout.RequestId = (Guid.TryParse(requestId, out Guid guid) && guid != Guid.Empty) ?
                guid : basketCheckout.RequestId;

            var basket = await _repository.GetBasketAsync(userId);

            if (basket is null)
            {
                return BadRequest();
            }

            var userName = HttpContext.User.FindFirst(x => x.Type == ClaimTypes.Name).Value;

            var eventMessage = new UserCheckoutAcceptedIntegrationEvent(
                userId, userName, basketCheckout.City,
                basketCheckout.Street, basketCheckout.State, basketCheckout.Country, basketCheckout.ZipCode,
                basketCheckout.CardNumber, basketCheckout.CardHolderName, basketCheckout.CardExpiration,
                basketCheckout.CardSecurityNumber, basketCheckout.CardTypeId, basketCheckout.Buyer,
                basketCheckout.RequestId, basket);

            // Once basket is checkout, sends an integration event to
            // ordering.api to convert basket to order and proceeds with
            // order creation process
            try
            {
                _eventBus.Publish(eventMessage);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "ERROR Publishing integration event: {IntegrationEventId} from {AppName}", eventMessage.Id, Program.AppName);

                throw;
            }

            return Accepted();
        }

        [HttpDelete("{id}")]
        [ProducesResponseType(typeof(void), (int)HttpStatusCode.OK)]
        public async Task DeleteBasketByIdAsync(string id)
        {
            await _repository.DeleteBasketAsync(id);
        }
    }
}