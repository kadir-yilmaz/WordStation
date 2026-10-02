using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WordStation.WebUI.Extensions;
using WordStation.WebUI.Models;
using WordStation.WebUI.Services.Abstract;

namespace WordStation.WebUI.Controllers
{
    [Authorize]
    public class DailyWordController : Controller
    {
        private readonly IDailyWordApiService _dailyWordService;
        private readonly IWordApiService _wordService;

        public DailyWordController(IDailyWordApiService dailyWordService, IWordApiService wordService)
        {
            _dailyWordService = dailyWordService;
            _wordService = wordService;
        }

        #region Properties (Auth Context)

        private string AccessToken => User.FindFirstValue("Token");
        private string CurrentUserId => User.FindFirstValue(ClaimTypes.NameIdentifier);
        private bool IsAuthenticated => !string.IsNullOrEmpty(CurrentUserId) && !string.IsNullOrEmpty(AccessToken);

        #endregion

        // GET: /DailyWord — Liste seçme sayfası
        public IActionResult Index()
        {
            if (!IsAuthenticated) return RedirectToAction("Login", "Account");
            return RedirectToAction("Study");
        }

        // GET: /DailyWord/Study?listName=YDS
        public async Task<IActionResult> Study(string listName)
        {
            if (!IsAuthenticated) return RedirectToAction("Login", "Account");

            var vm = new DailyWordViewModel();

            try
            {
                // Kullanıcının listelerini çek
                var lists = (await _wordService.GetListNamesAsync(CurrentUserId, AccessToken)).ToList();
                vm.AllLists = lists;

                if (string.IsNullOrWhiteSpace(listName))
                {
                    if (lists.Any())
                        listName = lists.First();
                    else
                        listName = "default"; // Hiç listesi yoksa
                }

                vm.SelectedList = listName;

                // Listedeki tüm kelimeleri al
                var allWords = (await _wordService.GetAllWordsAsync(CurrentUserId, listName, AccessToken)).ToList();
                vm.TotalWordCount = allWords.Count;

                // Session al veya oluştur
                var session = await _dailyWordService.GetSessionAsync(CurrentUserId, listName, AccessToken);

                if (session == null)
                {
                    // İlk kez açılıyor, session oluştur
                    session = await _dailyWordService.InitializeSessionAsync(CurrentUserId, listName, AccessToken);
                }

                if (session != null)
                {
                    vm.HasSession = true;

                    // Günlük kelimeler (sağ panel)
                    vm.DailyWords = session.DailyWords.Select(d => {
                        var orig = allWords.FirstOrDefault(w => w.Id == d.WordId);
                        return new Word
                        {
                            Id = d.WordId,
                            En = d.En,
                            Tr = d.Tr,
                            Example = orig?.Example
                        };
                    }).ToList();

                    // Çalışılmış kelimeler
                    vm.CompletedWords = session.CompletedWords.Select(d => {
                        var orig = allWords.FirstOrDefault(w => w.Id == d.WordId);
                        return new Word
                        {
                            Id = d.WordId,
                            En = d.En,
                            Tr = d.Tr,
                            Example = orig?.Example
                        };
                    }).ToList();
                    vm.CompletedWordCount = vm.CompletedWords.Count;

                    // Sol panel: tüm kelimeler (günlüktekiler ve tamamlananlar hariç)
                    var dailyWordIds = session.DailyWords.Select(d => d.WordId).ToHashSet();
                    var completedWordIds = session.CompletedWords.Select(d => d.WordId).ToHashSet();

                    vm.AllWords = allWords
                        .Where(w => !dailyWordIds.Contains(w.Id) && !completedWordIds.Contains(w.Id))
                        .ToList();
                }
                else
                {
                    vm.AllWords = allWords;
                }
            }
            catch (Exception ex)
            {
                this.NotifyError("Hata", "Kelimeler yüklenirken bir hata oluştu: " + ex.Message);
                vm.AllWords = new List<Word>();
            }

            return View(vm);
        }

        #region AJAX Endpoints (Proxy to WebAPI)

        // POST: /DailyWord/AddToDaily
        [HttpPost]
        public async Task<IActionResult> AddToDaily([FromBody] AjaxWordRequest request)
        {
            if (!IsAuthenticated)
                return Unauthorized();

            try
            {
                var result = await _dailyWordService.AddToDailyAsync(
                    CurrentUserId, request.ListName, request.WordIds, AccessToken);

                return Json(new { success = true, data = result });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        // POST: /DailyWord/RemoveFromDaily
        [HttpPost]
        public async Task<IActionResult> RemoveFromDaily([FromBody] AjaxWordRequest request)
        {
            if (!IsAuthenticated)
                return Unauthorized();

            try
            {
                var result = await _dailyWordService.RemoveFromDailyAsync(
                    CurrentUserId, request.ListName, request.WordIds, AccessToken);

                return Json(new { success = true, data = result });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        // POST: /DailyWord/CompleteWord
        [HttpPost]
        public async Task<IActionResult> CompleteWord([FromBody] AjaxCompleteRequest request)
        {
            if (!IsAuthenticated)
                return Unauthorized();

            try
            {
                var result = await _dailyWordService.CompleteWordAsync(
                    CurrentUserId, request.ListName, request.WordId, AccessToken);

                return Json(new { success = true, data = result });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        // POST: /DailyWord/CompleteWords
        [HttpPost]
        public async Task<IActionResult> CompleteWords([FromBody] AjaxWordRequest request)
        {
            if (!IsAuthenticated)
                return Unauthorized();

            try
            {
                var result = await _dailyWordService.CompleteWordsAsync(
                    CurrentUserId, request.ListName, request.WordIds, AccessToken);

                return Json(new { success = true, data = result });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        #endregion
    }

    // AJAX Request Models
    public class AjaxWordRequest
    {
        public string ListName { get; set; } = string.Empty;
        public List<int> WordIds { get; set; } = new();
    }

    public class AjaxCompleteRequest
    {
        public string ListName { get; set; } = string.Empty;
        public int WordId { get; set; }
    }
}
