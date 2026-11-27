using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using System.Security.Claims;

namespace WebApp.Infrastructure.Auth
{
    public class CustomAuthStateProvider : AuthenticationStateProvider
    {
        private const string TokenKey = "authToken";
        private const string UserNameKey = "userName";

        private static readonly AuthenticationState Anonymous = new(new ClaimsPrincipal(new ClaimsIdentity()));

        private readonly ProtectedLocalStorage _localStorage;
        private readonly ILogger<CustomAuthStateProvider> _logger;

        private AuthenticationState _cachedState;
        private string _token;

        public CustomAuthStateProvider(ProtectedLocalStorage localStorage, ILogger<CustomAuthStateProvider> logger)
        {
            _localStorage = localStorage;
            _logger = logger;
        }

        public override async Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            if (_cachedState != null)
                return _cachedState;

            try
            {
                //token kept in browser storage between visits
                var token = await _localStorage.GetAsync<string>(TokenKey);
                var userName = await _localStorage.GetAsync<string>(UserNameKey);

                if (token.Success && userName.Success && !string.IsNullOrEmpty(token.Value))
                {
                    _token = token.Value;
                    _cachedState = BuildState(userName.Value);
                    return _cachedState;
                }
            }
            catch (Exception ex)
            {
                //storage is not reachable before the circuit is ready
                _logger.LogDebug(ex, "Auth state could not be read from local storage");
                return Anonymous;
            }

            _cachedState = Anonymous;
            return _cachedState;
        }

        public async Task<string> GetTokenAsync()
        {
            if (_token == null)
                await GetAuthenticationStateAsync();

            return _token;
        }

        public async Task SignInAsync(string userName, string token)
        {
            await _localStorage.SetAsync(TokenKey, token);
            await _localStorage.SetAsync(UserNameKey, userName);

            _token = token;
            _cachedState = BuildState(userName);

            NotifyAuthenticationStateChanged(Task.FromResult(_cachedState));
        }

        public async Task SignOutAsync()
        {
            await _localStorage.DeleteAsync(TokenKey);
            await _localStorage.DeleteAsync(UserNameKey);

            _token = null;
            _cachedState = Anonymous;

            NotifyAuthenticationStateChanged(Task.FromResult(_cachedState));
        }

        private static AuthenticationState BuildState(string userName)
        {
            var identity = new ClaimsIdentity(new[] { new Claim(ClaimTypes.Name, userName) }, "jwt");

            return new AuthenticationState(new ClaimsPrincipal(identity));
        }
    }
}
