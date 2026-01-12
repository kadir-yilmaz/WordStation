using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Authentication.Cookies;
using WordStation.WebUI.Models;
using WordStation.WebUI.Services.Abstract;
using WordStation.WebUI.Extensions;
using System.IdentityModel.Tokens.Jwt;

namespace WordStation.WebUI.Controllers
{
    [Authorize]
    public class CookieDemoController : Controller
    {
        private readonly IOptionsMonitor<CookieAuthenticationOptions> _optionsMonitor;
        private readonly IConfiguration _configuration;
        private readonly IAuthApiService _authApiService;

        public CookieDemoController(
            IOptionsMonitor<CookieAuthenticationOptions> optionsMonitor, 
            IConfiguration configuration,
            IAuthApiService authApiService)
        {
            _optionsMonitor = optionsMonitor;
            _configuration = configuration;
            _authApiService = authApiService;
        }

        [Route("cookiedemo")]
        public async Task<IActionResult> Index()
        {
            var options = _optionsMonitor.Get(CookieAuthenticationDefaults.AuthenticationScheme);
            var authResult = await HttpContext.AuthenticateAsync();
            var claims = User.Claims.ToList();

            // Token'ları Claim'lerden alıyoruz
            var accessToken = User.FindFirst("Token")?.Value;
            var refreshToken = User.FindFirst("RefreshToken")?.Value;

            // Expiration'ları alıyoruz
            var accessTokenExpirationStr = User.FindFirst("AccessTokenExpiration")?.Value;
            var accessTokenExpiration = DateTime.TryParse(accessTokenExpirationStr, out var accDate) ? accDate : authResult.Properties?.ExpiresUtc?.UtcDateTime ?? DateTime.MinValue;

            var refreshTokenExpirationStr = User.FindFirst("RefreshTokenExpiration")?.Value;
            var refreshTokenExpiration = DateTime.TryParse(refreshTokenExpirationStr, out var refDate) ? refDate : DateTime.MinValue;

            // Cookie Bilgileri
            var cookieName = options.Cookie.Name ?? "WordStationAuth";
            var cookieValue = Request.Cookies[cookieName];

            // JWT Decode
            JwtInfo jwtInfo = null;
            if (!string.IsNullOrEmpty(accessToken))
            {
                jwtInfo = DecodeJwt(accessToken);
            }

            var model = new CookieDemoViewModel
            {
                IsAuthenticated = User.Identity?.IsAuthenticated == true,
                UserAccessToken = accessToken,
                UserRefreshToken = refreshToken,
                AccessTokenExpiration = accessTokenExpiration,
                RefreshTokenExpiration = refreshTokenExpiration,
                Claims = claims,
                CookieName = cookieName,
                CookieValue = cookieValue,
                IsPersistent = authResult.Properties?.IsPersistent ?? false,
                CookieExpiresUtc = authResult.Properties?.ExpiresUtc?.UtcDateTime,
                CookieIssuedUtc = authResult.Properties?.IssuedUtc?.UtcDateTime,
                JwtInfo = jwtInfo,
                // Cookie Configuration
                CookieConfig = new CookieConfigInfo
                {
                    SlidingExpiration = options.SlidingExpiration,
                    ExpireTimeSpan = options.ExpireTimeSpan,
                    LoginPath = options.LoginPath.Value,
                    LogoutPath = options.LogoutPath.Value,
                    AccessDeniedPath = options.AccessDeniedPath.Value,
                    IsHttpOnly = options.Cookie.HttpOnly,
                    SameSite = options.Cookie.SameSite.ToString()
                },
                ServerTimeUtc = DateTime.UtcNow,
                ServerTimeLocal = DateTime.Now
            };

            return View(model);
        }

        [HttpPost]
        [Route("cookiedemo/refresh")]
        public async Task<IActionResult> RefreshToken()
        {
            try
            {
                var currentToken = User.FindFirst("Token")?.Value;
                var currentRefreshToken = User.FindFirst("RefreshToken")?.Value;

                if (string.IsNullOrEmpty(currentRefreshToken))
                {
                    TempData["RefreshError"] = "Refresh Token bulunamadı. Lütfen tekrar giriş yapın.";
                    return RedirectToAction(nameof(Index));
                }

                // Mevcut Cookie özelliklerini sakla (ExpiresUtc değişmesin)
                var authResult = await HttpContext.AuthenticateAsync();
                var existingProperties = authResult.Properties;

                var (success, tokenResponse, error) = await _authApiService.RefreshTokenAsync(currentToken, currentRefreshToken);

                if (success && tokenResponse != null)
                {
                    // Cookie'yi yeni token'larla güncelle AMA eski ExpiresUtc'yi koru
                    await HttpContext.SignInAsync(
                        CookieAuthenticationDefaults.AuthenticationScheme,
                        tokenResponse.ToClaimsPrincipal(),
                        existingProperties); // 👈 Orijinal süreyi koru, uzatma

                    TempData["RefreshSuccess"] = "Token başarıyla yenilendi! (Cookie süresi değişmedi)";
                }
                else
                {
                    TempData["RefreshError"] = $"Token yenilenemedi: {error}";
                }
            }
            catch (Exception ex)
            {
                TempData["RefreshError"] = $"Hata: {ex.Message}";
            }

            return RedirectToAction(nameof(Index));
        }

        private JwtInfo DecodeJwt(string token)
        {
            try
            {
                var handler = new JwtSecurityTokenHandler();
                var jwt = handler.ReadJwtToken(token);

                return new JwtInfo
                {
                    Issuer = jwt.Issuer,
                    Audience = string.Join(", ", jwt.Audiences),
                    Subject = jwt.Subject,
                    IssuedAt = jwt.IssuedAt,
                    Expires = jwt.ValidTo,
                    Claims = jwt.Claims.Select(c => new JwtClaimInfo { Type = c.Type, Value = c.Value }).ToList()
                };
            }
            catch
            {
                return null;
            }
        }
    }
}
