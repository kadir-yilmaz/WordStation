namespace WordStation.WebUI.Models
{
    public class CookieDemoViewModel
    {
        public string UserAccessToken { get; set; }
        public string UserRefreshToken { get; set; }
        
        public DateTime AccessTokenExpiration { get; set; }
        public DateTime RefreshTokenExpiration { get; set; }

        public bool IsAuthenticated { get; set; }
        
        // Cookie Info
        public string CookieName { get; set; }
        public bool IsPersistent { get; set; }
        public DateTime? CookieExpiresUtc { get; set; }
        public DateTime? CookieIssuedUtc { get; set; }
        public string CookieValue { get; set; }
        
        public List<System.Security.Claims.Claim> Claims { get; set; } = new();
        
        // JWT Decoded Info
        public JwtInfo JwtInfo { get; set; }
        
        // Cookie Configuration
        public CookieConfigInfo CookieConfig { get; set; }
        
        // Server Time
        public DateTime ServerTimeUtc { get; set; }
        public DateTime ServerTimeLocal { get; set; }
    }
    
    public class JwtInfo
    {
        public string Issuer { get; set; }
        public string Audience { get; set; }
        public string Subject { get; set; }
        public DateTime IssuedAt { get; set; }
        public DateTime Expires { get; set; }
        public List<JwtClaimInfo> Claims { get; set; } = new();
    }
    
    public class JwtClaimInfo
    {
        public string Type { get; set; }
        public string Value { get; set; }
    }
    
    public class CookieConfigInfo
    {
        public bool SlidingExpiration { get; set; }
        public TimeSpan ExpireTimeSpan { get; set; }
        public string LoginPath { get; set; }
        public string LogoutPath { get; set; }
        public string AccessDeniedPath { get; set; }
        public bool IsHttpOnly { get; set; }
        public string SameSite { get; set; }
    }
}
