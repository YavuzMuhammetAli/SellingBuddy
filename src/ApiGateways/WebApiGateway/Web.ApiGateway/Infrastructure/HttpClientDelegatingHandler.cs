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

            return base.SendAsync(request, cancellationToken);
        }
    }
}
