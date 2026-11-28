using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using System.Security.Claims;
using WebApp.Domain.Models;

namespace WebApp.Infrastructure.Auth
{
    public class CustomAuthStateProvider : AuthenticationStateProvider
    {
        private const string SessionKey = "userSession";

        private static readonly AuthenticationState Anonymous = new(new ClaimsPrincipal(new ClaimsIdentity()));

        private readonly ProtectedLocalStorage _localStorage;
        private readonly ProtectedSessionStorage _sessionStorage;
        private readonly ILogger<CustomAuthStateProvider> _logger;

        private AuthenticationState _cachedState;

        public CustomAuthStateProvider(ProtectedLocalStorage localStorage, ProtectedSessionStorage sessionStorage, ILogger<CustomAuthStateProvider> logger)
        {
            _localStorage = localStorage;
            _sessionStorage = sessionStorage;
            _logger = logger;
        }

        public UserSession Session { get; private set; }

        public override async Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            if (_cachedState != null)
                return _cachedState;

            try
            {
                //remember me uses local storage, otherwise the tab session
                var stored = await _localStorage.GetAsync<UserSession>(SessionKey);

                if (!stored.Success || stored.Value == null)
                    stored = await _sessionStorage.GetAsync<UserSession>(SessionKey);

                if (stored.Success && stored.Value != null && stored.Value.Expiration > DateTime.UtcNow)
                {
                    Session = stored.Value;
                    _cachedState = BuildState(Session);
                    return _cachedState;
                }
            }
            catch (Exception ex)
            {
                //storage is not reachable before the circuit is ready
                _logger.LogDebug(ex, "Auth state could not be read from browser storage");
                return Anonymous;
            }

            _cachedState = Anonymous;
            return _cachedState;
        }

        public async Task<string> GetTokenAsync()
        {
            if (_cachedState == null)
                await GetAuthenticationStateAsync();

            return Session?.Token;
        }

        public async Task SignInAsync(UserSession session, bool rememberMe)
        {
            if (rememberMe)
                await _localStorage.SetAsync(SessionKey, session);
            else
                await _sessionStorage.SetAsync(SessionKey, session);

            Session = session;
            _cachedState = BuildState(session);

            NotifyAuthenticationStateChanged(Task.FromResult(_cachedState));
        }

        public async Task SignOutAsync()
        {
            await _localStorage.DeleteAsync(SessionKey);
            await _sessionStorage.DeleteAsync(SessionKey);

            Session = null;
            _cachedState = Anonymous;

            NotifyAuthenticationStateChanged(Task.FromResult(_cachedState));
        }

        private static AuthenticationState BuildState(UserSession session)
        {
            var identity = new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.Name, session.UserName ?? string.Empty),
                new Claim(ClaimTypes.Email, session.Email ?? string.Empty),
                new Claim(ClaimTypes.GivenName, session.FirstName ?? string.Empty),
                new Claim(ClaimTypes.Surname, session.LastName ?? string.Empty)
            }, "jwt");

            return new AuthenticationState(new ClaimsPrincipal(identity));
        }
    }
}
