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

        [HttpGet]
        public IActionResult GoogleLogin(string returnUrl = "/")
        {
            var redirectUrl = Url.Action("GoogleResponse", "Account", new { returnUrl });
            var properties = new AuthenticationProperties { RedirectUri = redirectUrl };
            return Challenge(properties, Microsoft.AspNetCore.Authentication.Google.GoogleDefaults.AuthenticationScheme);
        }

        [HttpGet]
        public async Task<IActionResult> GoogleResponse(string returnUrl = "/")
        {
            var authenticateResult = await HttpContext.AuthenticateAsync("ExternalCookie");
            if (!authenticateResult.Succeeded || authenticateResult.Principal == null)
            {
                this.NotifyError("Hata", "Google ile kimlik doğrulaması yapılamadı.");
                return RedirectToAction("Login");
            }

            var email = authenticateResult.Principal.FindFirstValue(ClaimTypes.Email);
            var name = authenticateResult.Principal.FindFirstValue(ClaimTypes.Name);
            var googleId = authenticateResult.Principal.FindFirstValue(ClaimTypes.NameIdentifier);

            // Geçici external cookie'yi temizle
            await HttpContext.SignOutAsync("ExternalCookie");

            if (string.IsNullOrEmpty(email))
            {
                this.NotifyError("Hata", "Google hesabından e-posta bilgisi alınamadı.");
                return RedirectToAction("Login");
            }

            var (success, tokenResponse, error) = await _authService.GoogleLoginAsync(email, googleId, name);

            if (success && tokenResponse != null)
            {
                await HttpContext.SignInAsync(
                    CookieAuthenticationDefaults.AuthenticationScheme,
                    tokenResponse.ToClaimsPrincipal(),
                    tokenResponse.ToAuthProperties());

                this.NotifySuccess("Giriş Yapıldı", "Google hesabınız ile başarıyla giriş yaptınız.");
                return Redirect(string.IsNullOrEmpty(returnUrl) ? "/" : returnUrl);
            }

            this.NotifyError("Giriş Başarısız", error ?? "Google ile giriş yapılırken bir hata oluştu.");
            return RedirectToAction("Login");
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
