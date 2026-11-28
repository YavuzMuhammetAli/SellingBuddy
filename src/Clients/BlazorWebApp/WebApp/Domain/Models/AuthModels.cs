namespace WebApp.Domain.Models
{
    public class LoginRequest
    {
        //email or user name, identity accepts both
        public string UserName { get; set; }

        public string Password { get; set; }
    }

    public class LoginResponse
    {
        public string UserName { get; set; }
        public string UserToken { get; set; }
        public DateTime Expiration { get; set; }
        public string Email { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
    }

    //what the webapp keeps about the signed in user
    public class UserSession
    {
        public string Token { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public DateTime Expiration { get; set; }

        public string DisplayName => string.IsNullOrWhiteSpace(FirstName) ? UserName : $"{FirstName} {LastName}".Trim();

        public string Initials => string.Concat(DisplayName.Split(' ', StringSplitOptions.RemoveEmptyEntries).Take(2).Select(p => char.ToUpperInvariant(p[0])));
    }
}
