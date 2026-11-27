using BasketService.Api.Core.Application.Repository;
using BasketService.Api.Core.Application.Services;
using BasketService.Api.Core.Domain.Models;
using BasketService.Api.IntegrationEvents.Events;
using EventBus.Base.Abstraction;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace BasketService.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class BasketController : ControllerBase
    {
        private readonly IBasketRepository _repository;
        private readonly IIdentityService _identityService;
        private readonly IEventBus _eventBus;
        private readonly ILogger<BasketController> _logger;

        public BasketController(IBasketRepository repository, IIdentityService identityService, IEventBus eventBus, ILogger<BasketController> logger)
        {
            _repository = repository;
            _identityService = identityService;
            _eventBus = eventBus;
            _logger = logger;
        }

        //basket of the current user
        [HttpGet]
        [ProducesResponseType(typeof(CustomerBasket), (int)HttpStatusCode.OK)]
        public async Task<ActionResult<CustomerBasket>> GetBasketAsync()
        {
            var userName = _identityService.GetUserName();
            var basket = await _repository.GetBasketAsync(userName);

            return Ok(basket ?? new CustomerBasket(userName));
        }

        [HttpGet("{id}")]
        [ProducesResponseType(typeof(CustomerBasket), (int)HttpStatusCode.OK)]
        public async Task<ActionResult<CustomerBasket>> GetBasketByIdAsync(string id)
        {
            var basket = await _repository.GetBasketAsync(id);

            return Ok(basket ?? new CustomerBasket(id));
        }

        [HttpPost("update")]
        [ProducesResponseType(typeof(CustomerBasket), (int)HttpStatusCode.OK)]
        public async Task<ActionResult<CustomerBasket>> UpdateBasketAsync([FromBody] CustomerBasket value)
        {
            return Ok(await _repository.UpdateBasketAsync(value));
        }

        [HttpPost("additem")]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        public async Task<ActionResult> AddItemToBasketAsync([FromBody] BasketItem basketItem)
        {
            var userName = _identityService.GetUserName();

            var basket = await _repository.GetBasketAsync(userName) ?? new CustomerBasket(userName);

            //same product from the same offer increases quantity
            var existingItem = basket.Items.FirstOrDefault(i => i.ProductId == basketItem.ProductId && i.OfferId == basketItem.OfferId);

            if (existingItem != null)
                existingItem.Quantity += basketItem.Quantity;
            else
                basket.Items.Add(basketItem);

            await _repository.UpdateBasketAsync(basket);

            return Ok();
        }

        [HttpPost("checkout")]
        [ProducesResponseType((int)HttpStatusCode.Accepted)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        public async Task<ActionResult> CheckoutAsync([FromBody] BasketCheckout basketCheckout)
        {
            var userName = _identityService.GetUserName();

            var basket = await _repository.GetBasketAsync(userName);

            if (basket == null || !basket.Items.Any())
                return BadRequest();

            var eventMessage = new OrderCreatedIntegrationEvent(userName, userName, basketCheckout.City, basketCheckout.Street,
                basketCheckout.State, basketCheckout.Country, basketCheckout.ZipCode, basketCheckout.CardNumber,
                basketCheckout.CardHolderName, basketCheckout.CardExpiration, basketCheckout.CardSecurityNumber,
                basketCheckout.CardTypeId, userName, basket)
            {
                OrderId = Guid.NewGuid(),
                ContactName = basketCheckout.ContactName,
                Phone = basketCheckout.Phone,
                ShippingMethod = ShippingOptions.Normalize(basketCheckout.ShippingMethod),
                ShippingFee = ShippingOptions.FeeFor(ShippingOptions.Normalize(basketCheckout.ShippingMethod))
            };

            try
            {
                //checkout event published to order service
                _eventBus.Publish(eventMessage);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error publishing integration event {IntegrationEventId} from BasketService", eventMessage.Id);
                throw;
            }

            return Accepted(new { orderId = eventMessage.OrderId });
        }

        [HttpDelete("{id}")]
        [ProducesResponseType(typeof(void), (int)HttpStatusCode.OK)]
        public async Task DeleteBasketByIdAsync(string id)
        {
            await _repository.DeleteBasketAsync(id);
        }
    }
}
