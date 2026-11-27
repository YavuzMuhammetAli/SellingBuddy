namespace IdentityService.Api.Application.Models
{
    public class AuthConfig
    {
        //shared with basket, order and gateway
        public string Secret { get; set; }

        public int ExpirationDays { get; set; } = 10;

        public List<DemoUser> Users { get; set; } = new();
    }

    public class DemoUser
    {
        public string UserName { get; set; }

        public string Password { get; set; }
    }
}
