namespace AS.Infrastructure.DTOs
{
    public sealed class LoginResponse
    {
        public bool IsAccess { get; set; }
        public string Token { get; set; } = string.Empty;
    }
}