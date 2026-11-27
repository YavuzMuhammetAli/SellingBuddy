using System.ComponentModel.DataAnnotations;

namespace WebApp.Domain.Models
{
    public class LoginRequest
    {
        [Required]
        public string UserName { get; set; }

        [Required]
        public string Password { get; set; }
    }

    public class LoginResponse
    {
        public string UserName { get; set; }

        public string UserToken { get; set; }

        public DateTime Expiration { get; set; }
    }
}
