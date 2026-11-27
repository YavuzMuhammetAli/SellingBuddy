namespace Web.ApiGateway.Infrastructure
{
    public class HttpClientDelegatingHandler : DelegatingHandler
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public HttpClientDelegatingHandler(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            //incoming token forwarded to downstream
            var authorizationHeader = _httpContextAccessor.HttpContext?.Request.Headers.Authorization.ToString();

            if (!string.IsNullOrEmpty(authorizationHeader))
            {
                request.Headers.Remove("Authorization");
                request.Headers.TryAddWithoutValidation("Authorization", authorizationHeader);
            }

            //language forwarded so catalog texts match the client
            var language = _httpContextAccessor.HttpContext?.Request.Headers.AcceptLanguage.ToString();

            if (!string.IsNullOrEmpty(language))
            {
                request.Headers.Remove("Accept-Language");
                request.Headers.TryAddWithoutValidation("Accept-Language", language);
            }

            return base.SendAsync(request, cancellationToken);
        }
    }
}
