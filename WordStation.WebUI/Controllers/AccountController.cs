using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using WordStation.WebUI.Models;
using WordStation.WebUI.Services.Abstract;
using WordStation.WebUI.Extensions; // Extension metodlar için namespace eklendi

namespace WordStation.WebUI.Controllers
{
    public class AccountController : Controller
    {
        private readonly IAuthApiService _authService;

        public AccountController(IAuthApiService authService)
        {
            _authService = authService;
        }

        public IActionResult Login(string ReturnUrl = "/")
        {
            return View(new LoginModel { ReturnUrl = ReturnUrl });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginModel model)
        {
            if (ModelState.IsValid)
            {
                var (success, tokenResponse, error) = await _authService.LoginAsync(model.Email, model.Password);

                if (success && tokenResponse != null)
                {
                    await HttpContext.SignInAsync(
                        CookieAuthenticationDefaults.AuthenticationScheme,
                        tokenResponse.ToClaimsPrincipal(),
                        tokenResponse.ToAuthProperties());

                    return Redirect(model.ReturnUrl ?? "/");
                }

                ModelState.AddModelError("", error ?? "Giriş başarısız.");
            }

            return View(model);
        }

        public async Task<IActionResult> Logout()
        {
            // Refresh Token'ı revoke et (veritabanında geçersiz kıl)
            var refreshToken = User.FindFirst("RefreshToken")?.Value;
            if (!string.IsNullOrEmpty(refreshToken))
            {
                await _authService.RevokeTokenAsync(refreshToken);
            }

            // Cookie'yi sil
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login");
        }

        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterModel model)
        {
            if (ModelState.IsValid)
            {
                var (success, errors) = await _authService.RegisterAsync(model.Email, model.Password);
                if (success)
                {
                    return RedirectToAction("Login");
                }

                foreach (var error in errors)
                {
                    ModelState.AddModelError("", error);
                }
            }
            return View(model);
        }

        public IActionResult AccessDenied()
        {
            return View();
        }
    }
}
