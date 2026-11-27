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

            //price and seller always come from the catalog offer, never from the client
            var offer = request.OfferId.HasValue
                ? item.Offers.FirstOrDefault(o => o.Id == request.OfferId.Value)
                : item.Offers.FirstOrDefault(o => o.IsBestOffer) ?? item.Offers.FirstOrDefault();

            if (offer == null)
                return BadRequest($"Offer {request.OfferId} not found for item {request.CatalogItemId}");

            if (offer.Stock <= 0)
                return BadRequest("Offer is out of stock");

            var currentBasket = await _basketService.GetBasketAsync();

            var existingItem = currentBasket.Items.FirstOrDefault(i => i.ProductId == item.Id && i.OfferId == offer.Id);

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
                    ProductSlug = item.Slug,
                    BrandName = item.BrandName,
                    UnitPrice = offer.Price,
                    OldUnitPrice = offer.OldPrice ?? 0,
                    PictureUrl = item.PictureUri,
                    Quantity = request.Quantity,
                    OfferId = offer.Id,
                    SellerId = offer.SellerId,
                    SellerSlug = offer.SellerSlug,
                    SellerName = offer.SellerName,
                    ShippingFee = offer.ShippingFee,
                    DeliveryDaysMax = offer.DeliveryDaysMax
                });
            }

            await _basketService.UpdateBasketAsync(currentBasket);

            return Ok();
        }
    }
}
