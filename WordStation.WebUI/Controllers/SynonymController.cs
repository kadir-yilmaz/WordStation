using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WordStation.WebUI.Models;
using WordStation.WebUI.Services.Abstract;
using WordStation.WebUI.ViewModels;

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

        private static string WordKey(Word w) =>
            $"{(w.En ?? string.Empty).Trim().ToLowerInvariant()}::{(w.Tr ?? string.Empty).Trim().ToLowerInvariant()}";

        public async Task<IActionResult> Index()
        {
            var userId = GetUserId();
            var token = GetToken();

            if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(token))
                return RedirectToAction("Login", "Account");

            var groupsTask = _wordApiService.GetSynonymGroupsAsync(userId, token);
            var wordsTask = _wordApiService.GetAllWordsForUserAsync(userId, token);
            await Task.WhenAll(groupsTask, wordsTask);

            var groups = groupsTask.Result.Where(g => g.Words != null && g.Words.Count > 0).ToList();
            var allWords = wordsTask.Result;

            // Gruplarda zaten görünen kelimeleri hariç tut (Flutter client ile aynı mantık)
            var groupedKeys = new HashSet<string>(groups.SelectMany(g => g.Words).Select(WordKey));

            var singleWords = allWords
                .Where(w => !string.IsNullOrWhiteSpace(w.Tr) && !groupedKeys.Contains(WordKey(w)))
                .GroupBy(w => w.Tr.Trim())
                .Select(g => new WordGroupDto { Tr = g.Key, Words = g.ToList() })
                .ToList();

            var model = new SynonymViewModel
            {
                Groups = groups,
                SingleWords = singleWords
            };

            return View(model);
        }
    }
}
