using BasketService.Api.Core.Application.Repository;
using BasketService.Api.Core.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace BasketService.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class WishlistController : ControllerBase
    {
        private readonly IWishlistRepository _repository;
        private readonly IIdentityService _identityService;

        public WishlistController(IWishlistRepository repository, IIdentityService identityService)
        {
            _repository = repository;
            _identityService = identityService;
        }

        //product ids only, details come from the catalog
        [HttpGet]
        [ProducesResponseType(typeof(List<int>), (int)HttpStatusCode.OK)]
        public async Task<ActionResult<List<int>>> GetAsync()
        {
            return await _repository.GetProductIdsAsync(_identityService.GetUserName());
        }

        [HttpPost("{productId:int}")]
        [ProducesResponseType((int)HttpStatusCode.NoContent)]
        public async Task<IActionResult> AddAsync(int productId)
        {
            await _repository.AddAsync(_identityService.GetUserName(), productId);
            return NoContent();
        }

        [HttpDelete("{productId:int}")]
        [ProducesResponseType((int)HttpStatusCode.NoContent)]
        public async Task<IActionResult> RemoveAsync(int productId)
        {
            await _repository.RemoveAsync(_identityService.GetUserName(), productId);
            return NoContent();
        }
    }
}
