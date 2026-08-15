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

            return Ok(tokenData);
        }

        [HttpPost("refresh-token")]
        public async Task<IActionResult> RefreshToken([FromBody] TokenRequestDto model)
        {
            var (success, message, tokenData) = await _authService.RefreshTokenAsync(model);
            if (!success)
                return Unauthorized(new { message });

            return Ok(tokenData);
        }

        [HttpPost("revoke-token")]
        public async Task<IActionResult> RevokeToken([FromBody] RevokeTokenDto model)
        {
            var (success, message) = await _authService.RevokeTokenAsync(model.RefreshToken);
            if (!success)
                return BadRequest(new { message });

            return Ok(new { message });
        }
    }
}
