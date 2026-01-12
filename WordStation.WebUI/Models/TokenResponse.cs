namespace WordStation.WebUI.Models
{
    public class TokenResponse
    {
        public string Token { get; set; } = string.Empty;
        public string RefreshToken { get; set; } = string.Empty; // Added
        public DateTime Expiration { get; set; }
        public DateTime RefreshTokenExpiration { get; set; } // Added
        public string UserId { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
    }
}
