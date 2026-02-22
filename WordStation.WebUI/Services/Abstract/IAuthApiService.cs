using WordStation.WebUI.Models;

namespace WordStation.WebUI.Services.Abstract
{
    public interface IAuthApiService
    {
        Task<(bool Success, TokenResponse? Data, string? Error)> LoginAsync(string email, string password);
        Task<(bool Success, string[] Errors)> RegisterAsync(string email, string password);
        Task<(bool Success, TokenResponse? Data, string? Error)> RefreshTokenAsync(string token, string refreshToken);
        Task<bool> RevokeTokenAsync(string refreshToken);
    }
}
