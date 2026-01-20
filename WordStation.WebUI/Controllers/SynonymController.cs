using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WordStation.WebUI.Models;
using WordStation.WebUI.Services.Abstract;

namespace WordStation.WebUI.Controllers
{
    [Authorize]
    public class SynonymController : Controller
    {
        private readonly ISynonymApiService _synonymService;
        private readonly IWordApiService _wordService;

        public SynonymController(ISynonymApiService synonymService, IWordApiService wordService)
        {
            _synonymService = synonymService;
            _wordService = wordService;
        }

        private string? GetToken() => User.FindFirstValue("Token");
        private string? GetUserId() => User.FindFirstValue(ClaimTypes.NameIdentifier);

        /// <summary>
        /// Tüm eş anlam gruplarını listeler
        /// </summary>
        public async Task<IActionResult> Index()
        {
            var userId = GetUserId();
            var token = GetToken();

            if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(token))
                return RedirectToAction("Login", "Account");

            var groups = await _synonymService.GetAllGroupsAsync(userId, token);
            
            // Kelime listelerini de al (yeni grup oluşturma için)
            var listNames = await _wordService.GetListNamesAsync(userId, token);
            ViewBag.ListNames = listNames.ToList();

            return View(groups);
        }

        /// <summary>
        /// Belirli bir liste için kelimeleri JSON olarak döner (AJAX)
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetWordsForList(string listName)
        {
            var userId = GetUserId();
            var token = GetToken();

            if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(token))
                return Unauthorized();

            var words = await _wordService.GetAllWordsAsync(userId, listName, token);
            return Json(words.Select(w => new { w.Id, w.En, w.Tr }));
        }

        /// <summary>
        /// Bir kelimenin eş anlamlılarını JSON olarak döner (AJAX - Cross-List)
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetSynonymsForWord(int wordId)
        {
            var userId = GetUserId();
            var token = GetToken();

            if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(token))
                return Unauthorized();

            var synonyms = await _synonymService.GetSynonymsForWordAsync(wordId, userId, token);
            return Json(synonyms.Select(w => new { w.Id, w.En, w.Tr, w.Example }));
        }

        /// <summary>
        /// Yeni eş anlam grubu oluşturur
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(string? name, List<int> wordIds)
        {
            var userId = GetUserId();
            var token = GetToken();

            if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(token))
                return RedirectToAction("Login", "Account");

            if (wordIds == null || wordIds.Count < 2)
            {
                TempData["Error"] = "En az 2 kelime seçmelisiniz.";
                return RedirectToAction("Index");
            }

            var group = await _synonymService.CreateGroupAsync(name, wordIds, userId, token);
            
            if (group != null)
            {
                TempData["Success"] = $"Eş anlam grubu \"{group.Name ?? $"Grup #{group.Id}"}\" oluşturuldu!";
            }
            else
            {
                TempData["Error"] = "Grup oluşturulurken bir hata oluştu.";
            }

            return RedirectToAction("Index");
        }

        /// <summary>
        /// Grubu siler
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var userId = GetUserId();
            var token = GetToken();

            if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(token))
                return RedirectToAction("Login", "Account");

            if (await _synonymService.DeleteGroupAsync(id, userId, token))
            {
                TempData["Success"] = "Grup silindi.";
            }
            else
            {
                TempData["Error"] = "Grup silinirken bir hata oluştu.";
            }

            return RedirectToAction("Index");
        }

        /// <summary>
        /// Gruba kelime(ler) ekler
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddWords(int groupId, List<int> wordIds)
        {
            var userId = GetUserId();
            var token = GetToken();

            if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(token))
                return RedirectToAction("Login", "Account");

            if (wordIds == null || !wordIds.Any())
            {
                TempData["Error"] = "Seçili kelime yok.";
                return RedirectToAction("Index");
            }

            int successCount = 0;
            foreach (var wordId in wordIds)
            {
                if (await _synonymService.AddWordToGroupAsync(groupId, wordId, userId, token))
                    successCount++;
            }

            if (successCount > 0)
                TempData["Success"] = $"{successCount} kelime gruba eklendi.";
            else
                TempData["Error"] = "Kelimeler eklenirken hata oluştu.";

            return RedirectToAction("Index");
        }

        /// <summary>
        /// Gruptan kelime çıkarır
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveWord(int groupId, int wordId)
        {
            var userId = GetUserId();
            var token = GetToken();

            if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(token))
                return RedirectToAction("Login", "Account");

            if (await _synonymService.RemoveWordFromGroupAsync(groupId, wordId, userId, token))
            {
                TempData["Success"] = "Kelime gruptan çıkarıldı.";
            }
            else
            {
                TempData["Error"] = "Kelime çıkarılırken bir hata oluştu.";
            }

            return RedirectToAction("Index");
        }

        /// <summary>
        /// Gruptan birden fazla kelime çıkarır (toplu)
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveWords(int groupId, List<int> wordIds)
        {
            var userId = GetUserId();
            var token = GetToken();

            if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(token))
                return RedirectToAction("Login", "Account");

            if (wordIds == null || wordIds.Count == 0)
            {
                TempData["Error"] = "Çıkarılacak kelime seçilmedi.";
                return RedirectToAction("Index");
            }

            int successCount = 0;
            foreach (var wordId in wordIds)
            {
                if (await _synonymService.RemoveWordFromGroupAsync(groupId, wordId, userId, token))
                    successCount++;
            }

            if (successCount > 0)
                TempData["Success"] = $"{successCount} kelime gruptan çıkarıldı.";
            else
                TempData["Error"] = "Kelimeler çıkarılırken hata oluştu.";

            return RedirectToAction("Index");
        }
    }
}
