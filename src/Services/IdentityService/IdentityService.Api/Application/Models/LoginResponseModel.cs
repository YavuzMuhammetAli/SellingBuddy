namespace IdentityService.Api.Application.Models
{
    public class LoginResponseModel
    {
        public string UserName { get; set; }

        public string UserToken { get; set; }

        public DateTime Expiration { get; set; }

        public string Email { get; set; }

        public string FirstName { get; set; }

        public string LastName { get; set; }
    }
}
