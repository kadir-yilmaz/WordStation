#nullable enable
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WordStation.WebUI.Models;
using WordStation.WebUI.Services.Abstract;
using WordStation.WebUI.Extensions;

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
                this.NotifyError("Hata", "En az 2 kelime seçmelisiniz.");
                return RedirectToAction("Index");
            }

            var group = await _synonymService.CreateGroupAsync(name, wordIds, userId, token);

            if (group != null)
            {
                this.NotifySuccess("Başarılı", $"Eş anlam grubu \"{group.Name ?? $"Grup #{group.Id}"}\" oluşturuldu!");
            }
            else
            {
                this.NotifyError("Hata", "Grup oluşturulurken bir hata oluştu.");
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
                this.NotifySuccess("Başarılı", "Grup silindi.");
            }
            else
            {
                this.NotifyError("Hata", "Grup silinirken bir hata oluştu.");
            }

            return RedirectToAction("Index");
        }

        /// <summary>
        /// Grup adını günceller
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateName(int id, string? name)
        {
            var userId = GetUserId();
            var token = GetToken();

            if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(token))
                return RedirectToAction("Login", "Account");

            if (await _synonymService.UpdateGroupNameAsync(id, name, userId, token))
            {
                TempData["Success"] = "Grup adı güncellendi.";
            }
            else
            {
                TempData["Error"] = "Grup adı güncellenirken bir hata oluştu.";
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
                this.NotifyError("Hata", "Seçili kelime yok.");
                return RedirectToAction("Index");
            }

            int successCount = 0;
            foreach (var wordId in wordIds)
            {
                if (await _synonymService.AddWordToGroupAsync(groupId, wordId, userId, token))
                    successCount++;
            }

            if (successCount > 0)
                this.NotifySuccess("Başarılı", $"{successCount} kelime gruba eklendi.");
            else
                this.NotifyError("Hata", "Kelimeler eklenirken hata oluştu.");

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
                this.NotifySuccess("Başarılı", "Kelime gruptan çıkarıldı.");
            }
            else
            {
                this.NotifyError("Hata", "Kelime çıkarılırken bir hata oluştu.");
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
                this.NotifyError("Hata", "Çıkarılacak kelime seçilmedi.");
                return RedirectToAction("Index");
            }

            int successCount = 0;
            foreach (var wordId in wordIds)
            {
                if (await _synonymService.RemoveWordFromGroupAsync(groupId, wordId, userId, token))
                    successCount++;
            }

            if (successCount > 0)
                this.NotifySuccess("Başarılı", $"{successCount} kelime gruptan çıkarıldı.");
            else
                this.NotifyError("Hata", "Kelimeler çıkarılırken hata oluştu.");

            return RedirectToAction("Index");
        }
    }
}
