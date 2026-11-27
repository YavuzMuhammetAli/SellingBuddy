using CatalogService.Api.Core.Application.ViewModels;
using CatalogService.Api.Core.Domain;
using CatalogService.Api.Infrastructure;
using CatalogService.Api.Infrastructure.Context;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System.Net;

namespace CatalogService.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CatalogController : ControllerBase
    {
        private readonly CatalogContext _catalogContext;
        private readonly CatalogSettings _settings;
        private readonly IWebHostEnvironment _environment;

        public CatalogController(CatalogContext catalogContext, IOptionsSnapshot<CatalogSettings> settings, IWebHostEnvironment environment)
        {
            _catalogContext = catalogContext;
            _settings = settings.Value;
            _environment = environment;

            //read only queries, no tracking needed
            _catalogContext.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;
        }

        //GET api/catalog/items?pageSize=10&pageIndex=0 or ?ids=1,2,3
        [HttpGet("items")]
        [ProducesResponseType(typeof(PaginatedItemsViewModel<CatalogItem>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(IEnumerable<CatalogItem>), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        public async Task<IActionResult> ItemsAsync([FromQuery] int pageSize = 10, [FromQuery] int pageIndex = 0, string ids = null)
        {
            if (!string.IsNullOrEmpty(ids))
            {
                var items = await GetItemsByIdsAsync(ids);

                if (!items.Any())
                    return BadRequest("ids value invalid. Must be comma-separated list of numbers");

                return Ok(items);
            }

            var query = _catalogContext.CatalogItems
                .Include(ci => ci.CatalogBrand)
                .Include(ci => ci.CatalogType);

            return Ok(await PaginateAsync(query, pageIndex, pageSize));
        }

        [HttpGet("items/{id:int}")]
        [ProducesResponseType(typeof(CatalogItem), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        public async Task<ActionResult<CatalogItem>> ItemByIdAsync(int id)
        {
            if (id <= 0)
                return BadRequest();

            var item = await _catalogContext.CatalogItems
                .Include(ci => ci.CatalogBrand)
                .Include(ci => ci.CatalogType)
                .SingleOrDefaultAsync(ci => ci.Id == id);

            if (item == null)
                return NotFound();

            ChangeUriPlaceholder(new[] { item });

            return item;
        }

        //GET api/catalog/items/withname/samplename?pageSize=3&pageIndex=0
        [HttpGet("items/withname/{name:minlength(1)}")]
        [ProducesResponseType(typeof(PaginatedItemsViewModel<CatalogItem>), (int)HttpStatusCode.OK)]
        public async Task<ActionResult<PaginatedItemsViewModel<CatalogItem>>> ItemsWithNameAsync(string name, [FromQuery] int pageSize = 10, [FromQuery] int pageIndex = 0)
        {
            var query = _catalogContext.CatalogItems
                .Include(ci => ci.CatalogBrand)
                .Include(ci => ci.CatalogType)
                .Where(ci => ci.Name.StartsWith(name));

            return await PaginateAsync(query, pageIndex, pageSize);
        }

        //GET api/catalog/items/type/1/brand/2
        [HttpGet("items/type/{catalogTypeId:int}/brand/{catalogBrandId:int?}")]
        [ProducesResponseType(typeof(PaginatedItemsViewModel<CatalogItem>), (int)HttpStatusCode.OK)]
        public async Task<ActionResult<PaginatedItemsViewModel<CatalogItem>>> ItemsByTypeIdAndBrandIdAsync(int catalogTypeId, int? catalogBrandId, [FromQuery] int pageSize = 10, [FromQuery] int pageIndex = 0)
        {
            var query = _catalogContext.CatalogItems
                .Include(ci => ci.CatalogBrand)
                .Include(ci => ci.CatalogType)
                .Where(ci => ci.CatalogTypeId == catalogTypeId);

            if (catalogBrandId.HasValue)
                query = query.Where(ci => ci.CatalogBrandId == catalogBrandId);

            return await PaginateAsync(query, pageIndex, pageSize);
        }

        //GET api/catalog/items/type/all/brand/2
        [HttpGet("items/type/all/brand/{catalogBrandId:int?}")]
        [ProducesResponseType(typeof(PaginatedItemsViewModel<CatalogItem>), (int)HttpStatusCode.OK)]
        public async Task<ActionResult<PaginatedItemsViewModel<CatalogItem>>> ItemsByBrandIdAsync(int? catalogBrandId, [FromQuery] int pageSize = 10, [FromQuery] int pageIndex = 0)
        {
            IQueryable<CatalogItem> query = _catalogContext.CatalogItems
                .Include(ci => ci.CatalogBrand)
                .Include(ci => ci.CatalogType);

            if (catalogBrandId.HasValue)
                query = query.Where(ci => ci.CatalogBrandId == catalogBrandId);

            return await PaginateAsync(query, pageIndex, pageSize);
        }

        [HttpGet("catalogtypes")]
        [ProducesResponseType(typeof(List<CatalogType>), (int)HttpStatusCode.OK)]
        public async Task<ActionResult<List<CatalogType>>> CatalogTypesAsync()
        {
            return await _catalogContext.CatalogTypes.ToListAsync();
        }

        [HttpGet("catalogbrands")]
        [ProducesResponseType(typeof(List<CatalogBrand>), (int)HttpStatusCode.OK)]
        public async Task<ActionResult<List<CatalogBrand>>> CatalogBrandsAsync()
        {
            return await _catalogContext.CatalogBrands.ToListAsync();
        }

        //GET api/catalog/items/1/pic
        [HttpGet("items/{catalogItemId:int}/pic")]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        public async Task<IActionResult> GetImageAsync(int catalogItemId)
        {
            if (catalogItemId <= 0)
                return BadRequest();

            var item = await _catalogContext.CatalogItems.SingleOrDefaultAsync(ci => ci.Id == catalogItemId);

            if (item == null || string.IsNullOrEmpty(item.PictureFileName))
                return NotFound();

            var path = Path.Combine(_environment.WebRootPath, "pics", item.PictureFileName);

            if (!System.IO.File.Exists(path))
                return NotFound();

            var buffer = await System.IO.File.ReadAllBytesAsync(path);

            return File(buffer, GetImageMimeType(Path.GetExtension(item.PictureFileName)));
        }

        [HttpPost("items")]
        [ProducesResponseType((int)HttpStatusCode.Created)]
        public async Task<ActionResult> CreateProductAsync([FromBody] CatalogItem product)
        {
            var item = new CatalogItem
            {
                CatalogBrandId = product.CatalogBrandId,
                CatalogTypeId = product.CatalogTypeId,
                Description = product.Description,
                Name = product.Name,
                PictureFileName = product.PictureFileName,
                Price = product.Price,
                AvailableStock = product.AvailableStock
            };

            _catalogContext.CatalogItems.Add(item);
            await _catalogContext.SaveChangesAsync();

            return CreatedAtAction(nameof(ItemByIdAsync), new { id = item.Id }, null);
        }

        [HttpPut("items")]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        [ProducesResponseType((int)HttpStatusCode.Created)]
        public async Task<ActionResult> UpdateProductAsync([FromBody] CatalogItem productToUpdate)
        {
            var catalogItem = await _catalogContext.CatalogItems.AsTracking().SingleOrDefaultAsync(i => i.Id == productToUpdate.Id);

            if (catalogItem == null)
                return NotFound(new { Message = $"Item with id {productToUpdate.Id} not found." });

            catalogItem.Name = productToUpdate.Name;
            catalogItem.Description = productToUpdate.Description;
            catalogItem.Price = productToUpdate.Price;
            catalogItem.AvailableStock = productToUpdate.AvailableStock;
            catalogItem.CatalogBrandId = productToUpdate.CatalogBrandId;
            catalogItem.CatalogTypeId = productToUpdate.CatalogTypeId;
            catalogItem.PictureFileName = productToUpdate.PictureFileName;

            await _catalogContext.SaveChangesAsync();

            return CreatedAtAction(nameof(ItemByIdAsync), new { id = catalogItem.Id }, null);
        }

        [HttpDelete("{id:int}")]
        [ProducesResponseType((int)HttpStatusCode.NoContent)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        public async Task<ActionResult> DeleteProductAsync(int id)
        {
            var product = await _catalogContext.CatalogItems.AsTracking().SingleOrDefaultAsync(x => x.Id == id);

            if (product == null)
                return NotFound();

            _catalogContext.CatalogItems.Remove(product);
            await _catalogContext.SaveChangesAsync();

            return NoContent();
        }

        private async Task<List<CatalogItem>> GetItemsByIdsAsync(string ids)
        {
            var numIds = ids.Split(',').Select(id => (Ok: int.TryParse(id, out int x), Value: x)).ToList();

            if (!numIds.All(nid => nid.Ok))
                return new List<CatalogItem>();

            var idsToSelect = numIds.Select(id => id.Value).ToList();

            var items = await _catalogContext.CatalogItems
                .Include(ci => ci.CatalogBrand)
                .Include(ci => ci.CatalogType)
                .Where(ci => idsToSelect.Contains(ci.Id))
                .ToListAsync();

            ChangeUriPlaceholder(items);

            return items;
        }

        private async Task<PaginatedItemsViewModel<CatalogItem>> PaginateAsync(IQueryable<CatalogItem> query, int pageIndex, int pageSize)
        {
            var totalItems = await query.LongCountAsync();

            var itemsOnPage = await query
                .OrderBy(c => c.Name)
                .Skip(pageSize * pageIndex)
                .Take(pageSize)
                .ToListAsync();

            ChangeUriPlaceholder(itemsOnPage);

            return new PaginatedItemsViewModel<CatalogItem>(pageIndex, pageSize, totalItems, itemsOnPage);
        }

        //picture uri filled from settings
        private void ChangeUriPlaceholder(IEnumerable<CatalogItem> items)
        {
            foreach (var item in items)
                item.PictureUri = _settings.PicBaseUrl.Replace("[0]", item.Id.ToString());
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
