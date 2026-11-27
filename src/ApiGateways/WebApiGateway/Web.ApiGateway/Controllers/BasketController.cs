using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using Web.ApiGateway.Models;
using Web.ApiGateway.Services.Interfaces;

namespace Web.ApiGateway.Controllers
{
    //aggregator, needs both catalog and basket
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class BasketController : ControllerBase
    {
        private readonly ICatalogService _catalogService;
        private readonly IBasketService _basketService;

        public BasketController(ICatalogService catalogService, IBasketService basketService)
        {
            _catalogService = catalogService;
            _basketService = basketService;
        }

        [HttpPost("items")]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        public async Task<ActionResult> AddBasketItemAsync([FromBody] AddBasketItemRequest request)
        {
            if (request == null || request.Quantity == 0)
                return BadRequest("Invalid payload");

            //item read from catalog first
            var item = await _catalogService.GetCatalogItemAsync(request.CatalogItemId);

            if (item == null)
                return BadRequest($"Catalog item {request.CatalogItemId} not found");

            var currentBasket = await _basketService.GetBasketAsync();

            var existingItem = currentBasket.Items.FirstOrDefault(i => i.ProductId == item.Id);

            if (existingItem != null)
            {
                existingItem.Quantity += request.Quantity;
            }
            else
            {
                currentBasket.Items.Add(new BasketDataItem
                {
                    Id = Guid.NewGuid().ToString(),
                    ProductId = item.Id,
                    ProductName = item.Name,
                    UnitPrice = item.Price,
                    PictureUrl = item.PictureUri,
                    Quantity = request.Quantity
                });
            }

            await _basketService.UpdateBasketAsync(currentBasket);

            return Ok();
        }
    }
}
