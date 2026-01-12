using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Security.Claims;
using WordStation.WebUI.Models;

namespace WordStation.WebUI.Extensions
{
    public static class AuthExtensions
    {
        public static ClaimsPrincipal ToClaimsPrincipal(this TokenResponse tokenResponse)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, tokenResponse.Email),
                new Claim(ClaimTypes.Email, tokenResponse.Email),
                new Claim(ClaimTypes.NameIdentifier, tokenResponse.Email),
                new Claim("Token", tokenResponse.Token),
                new Claim("RefreshToken", tokenResponse.RefreshToken),
                new Claim("RefreshTokenExpiration", tokenResponse.RefreshTokenExpiration.ToString("o")),
                new Claim("AccessTokenExpiration", tokenResponse.Expiration.ToString("o"))
            };

            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            return new ClaimsPrincipal(claimsIdentity);
        }

        public static AuthenticationProperties ToAuthProperties(this TokenResponse tokenResponse)
        {
            return new AuthenticationProperties
            {
                IsPersistent = true,
                ExpiresUtc = tokenResponse.RefreshTokenExpiration
            };
        }
    }
}
