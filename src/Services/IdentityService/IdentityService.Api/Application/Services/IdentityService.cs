using IdentityService.Api.Application.Models;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using System.Text;

namespace IdentityService.Api.Application.Services
{
    public class IdentityService : IIdentityService
    {
        private readonly AuthConfig _authConfig;
        private readonly ILogger<IdentityService> _logger;

        public IdentityService(IOptions<AuthConfig> authConfig, ILogger<IdentityService> logger)
        {
            _authConfig = authConfig.Value;
            _logger = logger;
        }

        public Task<LoginResponseModel> Login(LoginRequestModel requestModel)
        {
            //demo users come from config, no user store yet
            var user = _authConfig.Users.FirstOrDefault(u =>
                string.Equals(u.UserName, requestModel.UserName, StringComparison.OrdinalIgnoreCase) &&
                u.Password == requestModel.Password);

            if (user == null)
            {
                _logger.LogWarning("Login failed for {UserName}", requestModel.UserName);
                return Task.FromResult<LoginResponseModel>(null);
            }

            var expiration = DateTime.UtcNow.AddDays(_authConfig.ExpirationDays);
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_authConfig.Secret));

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.NameIdentifier, user.UserName),
                    new Claim(ClaimTypes.Name, user.UserName)
                }),
                Expires = expiration,
                SigningCredentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256)
            };

            var token = new JsonWebTokenHandler().CreateToken(tokenDescriptor);

            _logger.LogInformation("Token created for {UserName}", user.UserName);

            return Task.FromResult(new LoginResponseModel
            {
                UserName = user.UserName,
                UserToken = token,
                Expiration = expiration
            });
        }
    }
}
