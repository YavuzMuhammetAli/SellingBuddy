using CatalogService.Api.Core.Application;
using CatalogService.Api.Core.Application.ViewModels;
using CatalogService.Api.Core.Domain;
using CatalogService.Api.Infrastructure.Context;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Net;

namespace CatalogService.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CatalogController : ControllerBase
    {
        private readonly CatalogContext _catalogContext;
        private readonly CatalogQueryService _queries;
        private readonly IWebHostEnvironment _environment;

        public CatalogController(CatalogContext catalogContext, CatalogQueryService queries, IWebHostEnvironment environment)
        {
            _catalogContext = catalogContext;
            _queries = queries;
            _environment = environment;
        }

        //GET api/catalog/items?category=electronics&brands=1,2&sort=price_asc&pageIndex=0&pageSize=12
        [HttpGet("items")]
        [ProducesResponseType(typeof(PaginatedItemsViewModel<ProductSummaryViewModel>), (int)HttpStatusCode.OK)]
        public async Task<ActionResult<PaginatedItemsViewModel<ProductSummaryViewModel>>> ItemsAsync([FromQuery] CatalogItemQuery query, CancellationToken cancellationToken)
        {
            return await _queries.GetItemsAsync(query, cancellationToken);
        }

        //GET api/catalog/items/facets?category=electronics&q=sony
        [HttpGet("items/facets")]
        [ProducesResponseType(typeof(FacetsViewModel), (int)HttpStatusCode.OK)]
        public async Task<ActionResult<FacetsViewModel>> FacetsAsync([FromQuery] CatalogItemQuery query, CancellationToken cancellationToken)
        {
            return await _queries.GetFacetsAsync(query, cancellationToken);
        }

        //GET api/catalog/items/bulk?ids=1,2,3 for wishlist and basket lookups
        [HttpGet("items/bulk")]
        [ProducesResponseType(typeof(List<ProductSummaryViewModel>), (int)HttpStatusCode.OK)]
        public async Task<ActionResult<List<ProductSummaryViewModel>>> ItemsByIdsAsync([FromQuery] string ids, CancellationToken cancellationToken)
        {
            var query = new CatalogItemQuery { Ids = ids };

            if (!query.ItemIds.Any())
                return new List<ProductSummaryViewModel>();

            return await _queries.GetItemsByIdsAsync(query.ItemIds, cancellationToken);
        }

        //detail with offers, also used by the gateway aggregator
        [HttpGet("items/{id:int}")]
        [ProducesResponseType(typeof(ProductDetailViewModel), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        public async Task<ActionResult<ProductDetailViewModel>> ItemByIdAsync(int id, CancellationToken cancellationToken)
        {
            var item = await _queries.GetDetailAsync(id, null, cancellationToken);
            return item == null ? NotFound() : item;
        }

        [HttpGet("items/slug/{slug}")]
        [ProducesResponseType(typeof(ProductDetailViewModel), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        public async Task<ActionResult<ProductDetailViewModel>> ItemBySlugAsync(string slug, CancellationToken cancellationToken)
        {
            var item = await _queries.GetDetailAsync(null, slug.ToLowerInvariant(), cancellationToken);
            return item == null ? NotFound() : item;
        }

        [HttpGet("items/{id:int}/reviews")]
        [ProducesResponseType(typeof(List<ReviewViewModel>), (int)HttpStatusCode.OK)]
        public async Task<ActionResult<List<ReviewViewModel>>> ReviewsAsync(int id, CancellationToken cancellationToken)
        {
            return await _queries.GetReviewsAsync(id, cancellationToken);
        }

        //GET api/catalog/items/1/pic
        [HttpGet("items/{catalogItemId:int}/pic")]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        public async Task<IActionResult> GetImageAsync(int catalogItemId, CancellationToken cancellationToken)
        {
            var fileName = await _catalogContext.CatalogItems.AsNoTracking()
                .Where(ci => ci.Id == catalogItemId)
                .Select(ci => ci.PictureFileName)
                .FirstOrDefaultAsync(cancellationToken);

            return await PictureFileAsync(fileName);
        }

        //GET api/catalog/pics/electronics.jpg for categories, sellers and campaigns
        [HttpGet("pics/{fileName}")]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        public Task<IActionResult> GetFileAsync(string fileName)
        {
            return PictureFileAsync(fileName);
        }

        private async Task<IActionResult> PictureFileAsync(string fileName)
        {
            //only plain file names, no paths
            if (string.IsNullOrEmpty(fileName) || fileName != Path.GetFileName(fileName))
                return NotFound();

            var path = Path.Combine(_environment.WebRootPath, "pics", fileName);

            if (!System.IO.File.Exists(path))
                return NotFound();

            Response.Headers.CacheControl = "public,max-age=86400";

            return File(await System.IO.File.ReadAllBytesAsync(path), GetImageMimeType(Path.GetExtension(fileName)));
        }

        [HttpGet("catalogtypes")]
        [HttpGet("categories")]
        [ProducesResponseType(typeof(List<CategoryViewModel>), (int)HttpStatusCode.OK)]
        public async Task<ActionResult<List<CategoryViewModel>>> CategoriesAsync(CancellationToken cancellationToken)
        {
            return await _queries.GetCategoriesAsync(cancellationToken);
        }

        [HttpGet("categories/{slug}")]
        [ProducesResponseType(typeof(CategoryViewModel), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        public async Task<ActionResult<CategoryViewModel>> CategoryAsync(string slug, CancellationToken cancellationToken)
        {
            var category = await _queries.GetCategoryAsync(slug, cancellationToken);
            return category == null ? NotFound() : category;
        }

        [HttpGet("catalogbrands")]
        [ProducesResponseType(typeof(List<BrandViewModel>), (int)HttpStatusCode.OK)]
        public async Task<ActionResult<List<BrandViewModel>>> BrandsAsync(CancellationToken cancellationToken)
        {
            return await _queries.GetBrandsAsync(cancellationToken);
        }

        [HttpGet("sellers")]
        [ProducesResponseType(typeof(List<SellerViewModel>), (int)HttpStatusCode.OK)]
        public async Task<ActionResult<List<SellerViewModel>>> SellersAsync(CancellationToken cancellationToken)
        {
            return await _queries.GetSellersAsync(cancellationToken);
        }

        [HttpGet("sellers/{slug}")]
        [ProducesResponseType(typeof(SellerViewModel), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        public async Task<ActionResult<SellerViewModel>> SellerAsync(string slug, CancellationToken cancellationToken)
        {
            var seller = await _queries.GetSellerAsync(slug.ToLowerInvariant(), cancellationToken);
            return seller == null ? NotFound() : seller;
        }

        [HttpGet("campaigns")]
        [ProducesResponseType(typeof(List<CampaignViewModel>), (int)HttpStatusCode.OK)]
        public async Task<ActionResult<List<CampaignViewModel>>> CampaignsAsync(CancellationToken cancellationToken)
        {
            return await _queries.GetCampaignsAsync(cancellationToken);
        }

        [HttpGet("campaigns/{slug}")]
        [ProducesResponseType(typeof(CampaignViewModel), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        public async Task<ActionResult<CampaignViewModel>> CampaignAsync(string slug, CancellationToken cancellationToken)
        {
            var campaign = await _queries.GetCampaignAsync(slug.ToLowerInvariant(), cancellationToken);
            return campaign == null ? NotFound() : campaign;
        }

        [HttpDelete("{id:int}")]
        [ProducesResponseType((int)HttpStatusCode.NoContent)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        public async Task<ActionResult> DeleteProductAsync(int id)
        {
            var product = await _catalogContext.CatalogItems.SingleOrDefaultAsync(x => x.Id == id);

            if (product == null)
                return NotFound();

            _catalogContext.CatalogItems.Remove(product);
            await _catalogContext.SaveChangesAsync();

            return NoContent();
        }

        private static string GetImageMimeType(string extension) => extension.ToLowerInvariant() switch
        {
            ".png" => "image/png",
            ".gif" => "image/gif",
            ".jpg" or ".jpeg" => "image/jpeg",
            ".svg" => "image/svg+xml",
            ".webp" => "image/webp",
            _ => "application/octet-stream"
        };
    }
}
