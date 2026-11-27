using System.Security.Claims;

namespace BasketService.Api.Core.Application.Services
{
    public class IdentityService : IIdentityService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public IdentityService(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        //user name read from jwt claims
        public string GetUserName()
        {
            return _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.Name)?.Value;
        }
    }
}
