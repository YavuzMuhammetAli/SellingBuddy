using System.Globalization;
using System.Net.Http.Headers;
using WebApp.Infrastructure.Auth;

namespace WebApp.Infrastructure
{
    //one gateway client per circuit, bearer token and language added per call
    public class GatewayClient
    {
        private readonly HttpClient _httpClient;
        private readonly CustomAuthStateProvider _authStateProvider;

        public GatewayClient(HttpClient httpClient, CustomAuthStateProvider authStateProvider)
        {
            _httpClient = httpClient;
            _authStateProvider = authStateProvider;
        }

        public HttpClient Anonymous
        {
            get
            {
                ApplyLanguage();
                _httpClient.DefaultRequestHeaders.Authorization = null;
                return _httpClient;
            }
        }

        public async Task<HttpClient> AuthorizedAsync()
        {
            ApplyLanguage();

            var token = await _authStateProvider.GetTokenAsync();

            _httpClient.DefaultRequestHeaders.Authorization = string.IsNullOrEmpty(token)
                ? null
                : new AuthenticationHeaderValue("Bearer", token);

            return _httpClient;
        }

        //catalog texts come back in the ui language
        private void ApplyLanguage()
        {
            _httpClient.DefaultRequestHeaders.AcceptLanguage.Clear();
            _httpClient.DefaultRequestHeaders.AcceptLanguage.Add(new StringWithQualityHeaderValue(CultureInfo.CurrentUICulture.TwoLetterISOLanguageName));
        }
    }
}
