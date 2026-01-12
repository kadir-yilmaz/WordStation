using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using WordStation.BLL.Abstract;
using WordStation.DAL.Abstract;
using WordStation.EL.Dtos;
using WordStation.EL.Models;

namespace WordStation.BLL.Concrete
{
    public class AuthService : IAuthService
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly IConfiguration _configuration;
        private readonly IRefreshTokenRepository _refreshTokenRepository;

        public AuthService(UserManager<IdentityUser> userManager, IConfiguration configuration, IRefreshTokenRepository refreshTokenRepository)
        {
            _userManager = userManager;
            _configuration = configuration;
            _refreshTokenRepository = refreshTokenRepository;
        }

        public async Task<(bool Success, string Message, IEnumerable<string> Errors)> RegisterUserAsync(RegisterDto model)
        {
            var existingUser = await _userManager.FindByEmailAsync(model.Email!);
            if (existingUser != null)
                return (false, "Bu email adresi zaten kullanımda.", null);

            var user = new IdentityUser
            {
                UserName = model.Email,
                Email = model.Email,
                EmailConfirmed = true
            };

            var result = await _userManager.CreateAsync(user, model.Password!);
            if (!result.Succeeded)
            {
                var errors = result.Errors.Select(e => e.Description);
                return (false, "Kayıt hatası", errors);
            }

            return (true, "Kayıt başarılı.", null);
        }

        public async Task<(bool Success, string Message, object TokenData)> LoginUserAsync(LoginDto model)
        {
            var user = await _userManager.FindByEmailAsync(model.Email!);
            if (user == null || !await _userManager.CheckPasswordAsync(user, model.Password!))
            {
                return (false, "Geçersiz email veya şifre.", null);
            }

            var tokenData = await GenerateTokenResponseAsync(user);
            return (true, "Giriş başarılı", tokenData);
        }

        public async Task<(bool Success, string Message, object TokenData)> RefreshTokenAsync(TokenRequestDto model)
        {
            var storedRefreshToken = _refreshTokenRepository.GetByCondition(x => x.Token == model.RefreshToken, trackChanges: false)
                .FirstOrDefault();

            if (storedRefreshToken == null)
                return (false, "Invalid Refresh Token", null);

            if (storedRefreshToken.IsExpired)
                return (false, "Refresh Token Expired", null);

            if (storedRefreshToken.Revoked != null)
                return (false, "Refresh Token Revoked", null);

            var user = await _userManager.FindByIdAsync(storedRefreshToken.UserId);
            if (user == null)
                return (false, "User not found", null);

            // Fixed Refresh Token Model: Sadece Access Token yenilenir
            // Refresh Token aynı kalır, revoke edilmez, yenisi oluşturulmaz
            var newAccessToken = GenerateJwtToken(user);
            var accessTokenExpiration = DateTime.UtcNow.AddMinutes(
                double.Parse(_configuration["JwtSettings:ExpiryMinutes"]!));

            return (true, "Token refreshed", new
            {
                token = newAccessToken,
                refreshToken = storedRefreshToken.Token, // AYNI Refresh Token döndürülüyor
                expiration = accessTokenExpiration,
                userId = user.Id,
                email = user.Email,
                refreshTokenExpiration = storedRefreshToken.Expires // Orijinal süre korunuyor
            });
        }

        private async Task<object> GenerateTokenResponseAsync(IdentityUser user)
        {
            var token = GenerateJwtToken(user);
            var refreshToken = GenerateRefreshToken();

            var refreshTokenEntity = new RefreshToken
            {
                UserId = user.Id,
                Token = refreshToken,
                Expires = DateTime.UtcNow.AddDays(1), // Same as Cookie/Token life
                Created = DateTime.UtcNow
            };

            _refreshTokenRepository.Create(refreshTokenEntity);
            _refreshTokenRepository.Save();

            var expiration = DateTime.UtcNow.AddMinutes(
                double.Parse(_configuration["JwtSettings:ExpiryMinutes"]!));

            return new
            {
                token,
                refreshToken,
                expiration,
                userId = user.Id,
                email = user.Email,
                refreshTokenExpiration = refreshTokenEntity.Expires
            };
        }

        private string GenerateJwtToken(IdentityUser user)
        {
            var jwtSettings = _configuration.GetSection("JwtSettings");
            var secretKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings["SecretKey"]!));
            var credentials = new SigningCredentials(secretKey, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id),
                new Claim(ClaimTypes.Email, user.Email!),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            var token = new JwtSecurityToken(
                issuer: jwtSettings["Issuer"],
                audience: jwtSettings["Audience"],
                claims: claims,
                notBefore: DateTime.UtcNow, // Issued At (iat claim)
                expires: DateTime.UtcNow.AddMinutes(double.Parse(jwtSettings["ExpiryMinutes"]!)),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        private string GenerateRefreshToken()
        {
            var randomNumber = new byte[32];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomNumber);
            return Convert.ToBase64String(randomNumber);
        }

        public Task<(bool Success, string Message)> RevokeTokenAsync(string refreshToken)
        {
            var storedRefreshToken = _refreshTokenRepository.GetByCondition(x => x.Token == refreshToken, trackChanges: true)
                .FirstOrDefault();

            if (storedRefreshToken == null)
                return Task.FromResult((false, "Invalid Refresh Token"));

            if (storedRefreshToken.Revoked != null)
                return Task.FromResult((true, "Token already revoked"));

            storedRefreshToken.Revoked = DateTime.UtcNow;
            _refreshTokenRepository.Update(storedRefreshToken);
            _refreshTokenRepository.Save();

            return Task.FromResult((true, "Token revoked successfully"));
        }
    }
}
