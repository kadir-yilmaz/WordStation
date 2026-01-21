using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WordStation.WebUI.Models;
using WordStation.WebUI.Services.Abstract;

namespace WordStation.WebUI.Controllers
{
    [Authorize]
    public class WordController : Controller
    {
        private readonly IWordApiService _wordService;

        public WordController(IWordApiService wordService)
        {
            _wordService = wordService;
        }

        private string? GetToken() => User.FindFirstValue("Token");
        private string? GetUserId() => User.FindFirstValue(ClaimTypes.NameIdentifier);

        public async Task<IActionResult> Index(string listName, string SearchTerm = null, string searchMode = "starts")
        {
            var userId = GetUserId();
            var token = GetToken();

            if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(token))
                return RedirectToAction("Login", "Account");

            List<string> allLists = new();
            List<Word> words = new();

            try
            {
                // Listeleri Al
                var listsEnumerable = await _wordService.GetListNamesAsync(userId, token);
                allLists = listsEnumerable.ToList();
                ViewBag.AllLists = allLists;

                // listName yoksa ilk listeyi seç
                if (string.IsNullOrEmpty(listName) && allLists.Any())
                {
                    listName = allLists.First();
                }

                // Liste varsa kelimeleri al
                if (!string.IsNullOrEmpty(listName))
                {
                    IEnumerable<Word> wordsEnumerable;
                    if (!string.IsNullOrEmpty(SearchTerm))
                    {
                        wordsEnumerable = await _wordService.SearchWordAsync(SearchTerm, userId, listName, token, searchMode);
                        // Synonym aramaları için tüm kelimeleri de al
                        var allWordsEnumerable = await _wordService.GetAllWordsAsync(userId, listName, token);
                        ViewBag.AllWords = allWordsEnumerable.ToList();
                    }
                    else
                    {
                        wordsEnumerable = await _wordService.GetAllWordsAsync(userId, listName, token);
                    }
                    words = wordsEnumerable.ToList();
                }
                
                ViewBag.WordsCount = words.Count;
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error loading data: {ex.Message}";
            }

            var vm = new HomeViewModel
            {
                Words = words,
                ListNames = allLists,
                SelectedList = listName,
                SearchTerm = SearchTerm,
                SearchMode = searchMode
            };

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateWord(Word word, string SearchTerm, string searchMode)
        {
            var userId = GetUserId();
            var token = GetToken();

            if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(token))
                return RedirectToAction("Login", "Account");

            word.UserId = userId; // Ensure userId is set

            if (await _wordService.CreateWordAsync(word, token))
            {
                TempData["Success"] = $"Word \"{word.En}\" added successfully!";
            }
            else
            {
                TempData["Error"] = $"Error adding \"{word.En}\".";
            }

            return RedirectToAction("Index", new { listName = word.ListName, SearchTerm, searchMode });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateWord(Word word, string SearchTerm, string searchMode)
        {
            var userId = GetUserId();
            var token = GetToken();

            if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(token))
                return RedirectToAction("Login", "Account");
            
            word.UserId = userId;

            if (await _wordService.UpdateWordAsync(word, token))
            {
                TempData["Success"] = $"Word \"{word.En}\" updated successfully!";
            }
            else
            {
                TempData["Error"] = $"Error updating \"{word.En}\".";
            }

            return RedirectToAction("Index", new { listName = word.ListName, SearchTerm, searchMode });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteWord(int id, string listName, string SearchTerm, string searchMode, string wordEn)
        {
            var token = GetToken();
            if (string.IsNullOrEmpty(token))
                return RedirectToAction("Login", "Account");

            if (await _wordService.DeleteWordAsync(id, token))
            {
                TempData["Success"] = $"Word \"{wordEn}\" deleted successfully!";
            }
            else
            {
                TempData["Error"] = $"Error deleting \"{wordEn}\".";
            }

            return RedirectToAction("Index", new { listName, SearchTerm, searchMode });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RenameList(string listName, string newListName)
        {
            var userId = GetUserId();
            var token = GetToken();

            if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(token))
                return RedirectToAction("Login", "Account");

            if (await _wordService.UpdateListNameAsync(userId, listName, newListName, token))
            {
                TempData["Success"] = "List renamed successfully!";
                return RedirectToAction("Index", new { listName = newListName });
            }
            else
            {
                TempData["Error"] = "Error renaming list.";
                return RedirectToAction("Index", new { listName });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteList(string listName)
        {
            var userId = GetUserId();
            var token = GetToken();

            if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(token))
                return RedirectToAction("Login", "Account");

            if (await _wordService.DeleteListAsync(userId, listName, token))
            {
                TempData["Success"] = "List deleted successfully!";
                return RedirectToAction("Index", "Home"); // Or wherever appropriate
            }
            else
            {
                TempData["Error"] = "Error deleting list.";
                return RedirectToAction("Index", new { listName });
            }
        }
    }
}