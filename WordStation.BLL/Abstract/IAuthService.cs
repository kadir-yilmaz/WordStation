using Microsoft.AspNetCore.Identity;
using WordStation.EL.Dtos;
using WordStation.EL.Models;

namespace WordStation.BLL.Abstract
{
    public interface IAuthService
    {
        Task<(bool Success, string Message, IEnumerable<string> Errors)> RegisterUserAsync(RegisterDto model);
        Task<(bool Success, string Message, object TokenData)> LoginUserAsync(LoginDto model);
        Task<(bool Success, string Message, object TokenData)> RefreshTokenAsync(TokenRequestDto model);
        Task<(bool Success, string Message)> RevokeTokenAsync(string refreshToken);
    }
}
