using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WordStation.WebUI.Models;
using WordStation.WebUI.Services.Abstract;
using WordStation.WebUI.Extensions;

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

        #region Properties (Auth Context)

        private string? AccessToken => User.FindFirstValue("Token");
        private string? CurrentUserId => User.FindFirstValue(ClaimTypes.NameIdentifier);

        private bool IsAuthenticated => !string.IsNullOrEmpty(CurrentUserId) && !string.IsNullOrEmpty(AccessToken);

        #endregion

        public async Task<IActionResult> Index(string listName, string SearchTerm = null, string searchMode = "starts")
        {
            if (!IsAuthenticated) return RedirectToLogin();

            var vm = new HomeViewModel
            {
                SearchTerm = SearchTerm ?? string.Empty,
                SearchMode = searchMode
            };

            try
            {
                // Parallel Fetching (Senior Optimization)
                var listsTask = _wordService.GetListNamesAsync(CurrentUserId!, AccessToken!);
                var allWordsTask = _wordService.GetAllWordsForUserAsync(CurrentUserId!, AccessToken!);
                var synonymsTask = _wordService.GetSynonymGroupsAsync(CurrentUserId!, AccessToken!);

                await Task.WhenAll(listsTask, allWordsTask, synonymsTask);

                vm.AllLists = (await listsTask).ToList();
                vm.AllWords = (await allWordsTask).ToList();
                vm.SynonymGroups = (await synonymsTask).ToList();

                // List Selection Logic
                vm.SelectedList = string.IsNullOrEmpty(listName) && vm.AllLists.Any() 
                    ? vm.AllLists.First() 
                    : listName;

                // Conditional Fetching for the main grid
                if (!string.IsNullOrEmpty(vm.SelectedList))
                {
                    vm.Words = !string.IsNullOrEmpty(SearchTerm)
                        ? await _wordService.SearchWordAsync(SearchTerm, CurrentUserId!, vm.SelectedList, AccessToken!, searchMode)
                        : await _wordService.GetAllWordsAsync(CurrentUserId!, vm.SelectedList, AccessToken!);
                }
            }
            catch (Exception ex)
            {
                this.NotifyError("System Error", "An error occurred while loading your words.");
            }

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateWord(Word word, string SearchTerm, string searchMode)
        {
            if (!IsAuthenticated) return RedirectToLogin();
            if (!ModelState.IsValid) return RedirectToIndex(word.ListName, SearchTerm, searchMode, "Invalid data.");

            word.UserId = CurrentUserId!;

            if (await _wordService.CreateWordAsync(word, AccessToken!))
                this.NotifySuccess("Success", $"Word \"{word.En}\" added!");
            else
                this.NotifyError("Failure", "Could not add the word.");

            return RedirectToIndex(word.ListName, SearchTerm, searchMode);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateWord(Word word, string SearchTerm, string searchMode)
        {
            if (!IsAuthenticated) return RedirectToLogin();
            if (!ModelState.IsValid) return RedirectToIndex(word.ListName, SearchTerm, searchMode, "Invalid data.");

            word.UserId = CurrentUserId!;

            if (await _wordService.UpdateWordAsync(word, AccessToken!))
                this.NotifySuccess("Success", "Word updated!");
            else
                this.NotifyError("Failure", "Update failed.");

            return RedirectToIndex(word.ListName, SearchTerm, searchMode);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteWord(int id, string listName, string SearchTerm, string searchMode, string wordEn)
        {
            if (!IsAuthenticated) return RedirectToLogin();

            if (await _wordService.DeleteWordAsync(id, AccessToken!))
                this.NotifySuccess("Deleted", $"\"{wordEn}\" has been removed.");
            else
                this.NotifyError("Failure", "Delete operation failed.");

            return RedirectToIndex(listName, SearchTerm, searchMode);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RenameList(string listName, string newListName)
        {
            if (!IsAuthenticated) return RedirectToLogin();
            if (string.IsNullOrWhiteSpace(newListName)) return RedirectToIndex(listName);

            if (await _wordService.UpdateListNameAsync(CurrentUserId!, listName, newListName, AccessToken!))
            {
                this.NotifySuccess("Renamed", "List name updated successfully.");
                return RedirectToIndex(newListName);
            }

            this.NotifyError("Failure", "Rename failed.");
            return RedirectToIndex(listName);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteList(string listName)
        {
            if (!IsAuthenticated) return RedirectToLogin();

            if (await _wordService.DeleteListAsync(CurrentUserId!, listName, AccessToken!))
            {
                this.NotifySuccess("Deleted", "List removed.");
                return RedirectToAction("Index", "Home");
            }

            this.NotifyError("Failure", "Could not delete list.");
            return RedirectToIndex(listName);
        }

        #region Helper Methods

        private IActionResult RedirectToLogin() => RedirectToAction("Login", "Account");

        private IActionResult RedirectToIndex(string? listName = null, string? searchTerm = null, string? mode = null, string? error = null)
        {
            if (error != null) this.NotifyError("Error", error);
            return RedirectToAction("Index", new { listName, SearchTerm = searchTerm, searchMode = mode });
        }

        #endregion
    }
}