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
        /// Tum es anlam gruplarini listeler
        /// </summary>
        public async Task<IActionResult> Index()
        {
            var userId = GetUserId();
            var token = GetToken();

            if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(token))
                return RedirectToAction("Login", "Account");

            var groups = await _synonymService.GetAllGroupsAsync(userId, token);

            // Kelime listelerini de al (yeni grup olusturma icin)
            var listNames = await _wordService.GetListNamesAsync(userId, token);
            ViewBag.ListNames = listNames.ToList();

            return View(groups);
        }

        /// <summary>
        /// Belirli bir liste icin kelimeleri JSON olarak doner (AJAX)
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
        /// Bir kelimenin es anlamlilarini JSON olarak doner (AJAX - Cross-List)
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
        /// Yeni es anlam grubu olusturur
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
                this.NotifyError("Hata", "En az 2 kelime secmelisiniz.");
                return RedirectToAction("Index");
            }

            var group = await _synonymService.CreateGroupAsync(name, wordIds, userId, token);

            if (group != null)
            {
                this.NotifySuccess("Basarili", $"Es anlam grubu \"{group.Name ?? $"Grup #{group.Id}"}\" olusturuldu!");
            }
            else
            {
                this.NotifyError("Hata", "Grup olusturulurken bir hata olustu.");
            }

            return RedirectToAction("Index");
        }

        /// <summary>
        /// Grubu siler
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id, string? groupName)
        {
            var userId = GetUserId();
            var token = GetToken();

            if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(token))
                return RedirectToAction("Login", "Account");

            var displayName = string.IsNullOrWhiteSpace(groupName)
                ? $"Grup #{id}"
                : groupName;

            if (await _synonymService.DeleteGroupAsync(id, userId, token))
            {
                this.NotifySuccess("Basarili", $"\"{displayName}\" grubu silindi.");
            }
            else
            {
                this.NotifyError("Hata", $"\"{displayName}\" grubu silinirken bir hata olustu.");
            }

            return RedirectToAction("Index");
        }

        /// <summary>
        /// Grubu toplu olarak gunceller (Ad, Kelime Ekleme, Kelime Cikarma)
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Update(int id, string? name, List<int> addedWordIds, List<int> removedWordIds)
        {
            var userId = GetUserId();
            var token = GetToken();

            if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(token))
                return RedirectToAction("Login", "Account");

            bool nameUpdated = false;
            int addedCount = 0;
            int removedCount = 0;

            // 1. Isim Guncelleme
            if (!string.IsNullOrWhiteSpace(name))
            {
                if (await _synonymService.UpdateGroupNameAsync(id, name, userId, token))
                    nameUpdated = true;
            }

            // 2. Kelime Ekleme
            if (addedWordIds != null && addedWordIds.Any())
            {
                foreach (var wordId in addedWordIds)
                {
                    if (await _synonymService.AddWordToGroupAsync(id, wordId, userId, token))
                        addedCount++;
                }
            }

            // 3. Kelime Cikarma
            if (removedWordIds != null && removedWordIds.Any())
            {
                foreach (var wordId in removedWordIds)
                {
                    if (await _synonymService.RemoveWordFromGroupAsync(id, wordId, userId, token))
                        removedCount++;
                }
            }

            if (nameUpdated || addedCount > 0 || removedCount > 0)
            {
                var message = "Grup guncellendi.";
                if (nameUpdated) message += " Ad guncellendi.";
                if (addedCount > 0) message += $" {addedCount} yeni kelime eklendi.";
                if (removedCount > 0) message += $" {removedCount} kelime cikarildi.";

                this.NotifySuccess("Basarili", message);
            }
            else
            {
                this.NotifyInfo("Bilgi", "Herhangi bir degisiklik yapilmadi.");
            }

            return RedirectToAction("Index");
        }

        /// <summary>
        /// Grup adini gunceller
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
                TempData["Success"] = "Grup adi guncellendi.";
            }
            else
            {
                TempData["Error"] = "Grup adi guncellenirken bir hata olustu.";
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
                this.NotifyError("Hata", "Secili kelime yok.");
                return RedirectToAction("Index");
            }

            int successCount = 0;
            foreach (var wordId in wordIds)
            {
                if (await _synonymService.AddWordToGroupAsync(groupId, wordId, userId, token))
                    successCount++;
            }

            if (successCount > 0)
                this.NotifySuccess("Basarili", $"{successCount} kelime gruba eklendi.");
            else
                this.NotifyError("Hata", "Kelimeler eklenirken hata olustu.");

            return RedirectToAction("Index");
        }

        /// <summary>
        /// Gruptan kelime cikarir
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
                this.NotifySuccess("Basarili", "Kelime gruptan cikarildi.");
            }
            else
            {
                this.NotifyError("Hata", "Kelime cikarilirken bir hata olustu.");
            }

            return RedirectToAction("Index");
        }

        /// <summary>
        /// Gruptan birden fazla kelime cikarir (toplu)
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
                this.NotifyError("Hata", "Cikarilacak kelime secilmedi.");
                return RedirectToAction("Index");
            }

            int successCount = 0;
            foreach (var wordId in wordIds)
            {
                if (await _synonymService.RemoveWordFromGroupAsync(groupId, wordId, userId, token))
                    successCount++;
            }

            if (successCount > 0)
                this.NotifySuccess("Basarili", $"{successCount} kelime gruptan cikarildi.");
            else
                this.NotifyError("Hata", "Kelimeler cikarilirken hata olustu.");

            return RedirectToAction("Index");
        }
    }
}
