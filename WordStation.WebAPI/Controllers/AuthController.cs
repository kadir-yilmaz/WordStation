using Microsoft.AspNetCore.Mvc;
using WordStation.BLL.Abstract;
using WordStation.EL.Dtos;

namespace WordStation.WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDto model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var (success, message, errors) = await _authService.RegisterUserAsync(model);
            if (!success)
            {
                if (errors != null)
                    return BadRequest(new { message, errors });
                return BadRequest(new { message });
            }

            return Ok(new { message });
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var (success, message, tokenData) = await _authService.LoginUserAsync(model);
            if (!success)
                return Unauthorized(new { message });

            //  Web SPA (Flutter Web vb.) istemcileri için HttpOnly Refresh Token Cookie'si ekle
            SetRefreshTokenCookie(tokenData);

            return Ok(tokenData);
        }

        [HttpPost("google-login")]
        public async Task<IActionResult> GoogleLogin([FromBody] GoogleLoginDto model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var (success, message, tokenData) = await _authService.GoogleLoginUserAsync(model);
            if (!success)
                return BadRequest(new { message });

            //  Web SPA (Flutter Web vb.) istemcileri için HttpOnly Refresh Token Cookie'si ekle
            SetRefreshTokenCookie(tokenData);

            return Ok(tokenData);
        }

        [HttpPost("refresh-token")]
        public async Task<IActionResult> RefreshToken([FromBody] TokenRequestDto? model)
        {
            model ??= new TokenRequestDto();

            //  Eğer body'de RefreshToken boşsa (Web SPA / Silent Refresh), Cookie'den oku
            if (string.IsNullOrEmpty(model.RefreshToken))
            {
                var cookieRefreshToken = Request.Cookies["refreshToken"];
                if (!string.IsNullOrEmpty(cookieRefreshToken))
                {
                    model.RefreshToken = cookieRefreshToken;
                }
            }

            var (success, message, tokenData) = await _authService.RefreshTokenAsync(model);
            if (!success)
                return Unauthorized(new { message });

            //  Yenilenen veya mevcut Refresh Token'ı Cookie olarak güncelle
            SetRefreshTokenCookie(tokenData);

            return Ok(tokenData);
        }

        [HttpPost("revoke-token")]
        public async Task<IActionResult> RevokeToken([FromBody] RevokeTokenDto? model)
        {
            var refreshToken = model?.RefreshToken ?? Request.Cookies["refreshToken"];

            if (!string.IsNullOrEmpty(refreshToken))
            {
                var (success, message) = await _authService.RevokeTokenAsync(refreshToken);
                if (!success)
                    return BadRequest(new { message });
            }

            // 🍪 Çıkış yapıldığında Cookie'yi temizle
            DeleteRefreshTokenCookie();

            return Ok(new { message = "Token revoked successfully" });
        }

        #region Helper Methods

        private void SetRefreshTokenCookie(object? tokenData)
        {
            if (tokenData == null) return;

            string? refreshToken = null;
            DateTime? refreshTokenExpiration = null;

            var propRefreshToken = tokenData.GetType().GetProperty("refreshToken") 
                                 ?? tokenData.GetType().GetProperty("RefreshToken");
            if (propRefreshToken != null)
            {
                refreshToken = propRefreshToken.GetValue(tokenData)?.ToString();
            }

            var propExpiration = tokenData.GetType().GetProperty("refreshTokenExpiration") 
                              ?? tokenData.GetType().GetProperty("RefreshTokenExpiration");
            if (propExpiration != null)
            {
                var val = propExpiration.GetValue(tokenData);
                if (val is DateTime dt)
                {
                    refreshTokenExpiration = dt;
                }
            }

            if (!string.IsNullOrEmpty(refreshToken))
            {
                var cookieOptions = new CookieOptions
                {
                    HttpOnly = true,
                    Secure = true,
                    SameSite = SameSiteMode.None,
                    Path = "/",
                    Expires = refreshTokenExpiration.HasValue 
                        ? new DateTimeOffset(refreshTokenExpiration.Value) 
                        : DateTimeOffset.UtcNow.AddDays(7)
                };

                Response.Cookies.Append("refreshToken", refreshToken, cookieOptions);
            }
        }

        private void DeleteRefreshTokenCookie()
        {
            Response.Cookies.Delete("refreshToken", new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.None,
                Path = "/"
            });
        }

        #endregion
    }
}