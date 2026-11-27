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

        public async Task<bool> LoginAsync(string userName, string password)
        {
            var response = await _gateway.Anonymous.PostAsJsonAsync("auth", new LoginRequest { UserName = userName, Password = password });

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("Login failed for {UserName} with status {StatusCode}", userName, response.StatusCode);
                return false;
            }

            var loginResponse = await response.Content.ReadFromJsonAsync<LoginResponse>();

            //token stored so other services can use it
            await _authStateProvider.SignInAsync(loginResponse.UserName, loginResponse.UserToken);

            return true;
        }

        public Task LogoutAsync()
        {
            return _authStateProvider.SignOutAsync();
        }
    }
}
