using WebApp.Application.Services.Interfaces;
using WebApp.Domain.Models;
using WebApp.Infrastructure;
using WebApp.Infrastructure.Auth;

namespace WebApp.Application.Services
{
    public class IdentityService : IIdentityService
    {
        private readonly GatewayClient _gateway;
        private readonly CustomAuthStateProvider _authStateProvider;
        private readonly ILogger<IdentityService> _logger;

        public IdentityService(GatewayClient gateway, CustomAuthStateProvider authStateProvider, ILogger<IdentityService> logger)
        {
            _gateway = gateway;
            _authStateProvider = authStateProvider;
            _logger = logger;
        }

        public async Task<bool> LoginAsync(string userName, string password, bool rememberMe)
        {
            var response = await _gateway.Anonymous.PostAsJsonAsync("auth", new LoginRequest { UserName = userName, Password = password });

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogInformation("Login failed with status {StatusCode}", response.StatusCode);
                return false;
            }

            var login = await response.Content.ReadFromJsonAsync<LoginResponse>();

            //token and profile kept so other services can use them
            await _authStateProvider.SignInAsync(new UserSession
            {
                Token = login.UserToken,
                UserName = login.UserName,
                Email = login.Email,
                FirstName = login.FirstName,
                LastName = login.LastName,
                Expiration = login.Expiration
            }, rememberMe);

            return true;
        }

        public Task LogoutAsync() => _authStateProvider.SignOutAsync();
    }
}
