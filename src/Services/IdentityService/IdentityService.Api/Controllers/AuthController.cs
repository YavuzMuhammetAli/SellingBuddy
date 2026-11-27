using IdentityService.Api.Application.Models;
using IdentityService.Api.Application.Services;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace IdentityService.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IIdentityService _identityService;

        public AuthController(IIdentityService identityService)
        {
            _identityService = identityService;
        }

        [HttpPost]
        [ProducesResponseType(typeof(LoginResponseModel), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
        public async Task<IActionResult> Login([FromBody] LoginRequestModel loginRequestModel)
        {
            var result = await _identityService.Login(loginRequestModel);

            if (result == null)
                return Unauthorized();

            return Ok(result);
        }
    }
}
