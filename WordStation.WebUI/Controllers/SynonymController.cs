using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WordStation.WebUI.Services.Abstract;

namespace WordStation.WebUI.Controllers
{
    [Authorize]
    public class SynonymController : Controller
    {
        private readonly IWordApiService _wordApiService;

        public SynonymController(IWordApiService wordApiService)
        {
            _wordApiService = wordApiService;
        }

        private string? GetToken() => User.FindFirstValue("Token");
        private string? GetUserId() => User.FindFirstValue(ClaimTypes.NameIdentifier);

        public async Task<IActionResult> Index()
        {
            var userId = GetUserId();
            var token = GetToken();

            if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(token))
                return RedirectToAction("Login", "Account");

            var groups = await _wordApiService.GetSynonymGroupsAsync(userId, token);
            return View(groups);
        }
    }
}
