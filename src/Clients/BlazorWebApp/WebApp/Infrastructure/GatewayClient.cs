using System.Net.Http.Headers;
using WebApp.Infrastructure.Auth;

namespace WebApp.Infrastructure
{
    //one gateway client per circuit, bearer token added per call
    public class GatewayClient
    {
        private readonly HttpClient _httpClient;
        private readonly CustomAuthStateProvider _authStateProvider;

        public GatewayClient(HttpClient httpClient, CustomAuthStateProvider authStateProvider)
        {
            _httpClient = httpClient;
            _authStateProvider = authStateProvider;
        }

        public HttpClient Anonymous => _httpClient;

        public async Task<HttpClient> AuthorizedAsync()
        {
            var token = await _authStateProvider.GetTokenAsync();

            _httpClient.DefaultRequestHeaders.Authorization = string.IsNullOrEmpty(token)
                ? null
                : new AuthenticationHeaderValue("Bearer", token);

            return _httpClient;
        }
    }
}
