using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using WordStation.WebUI.Models;
using WordStation.WebUI.Services.Abstract;

namespace WordStation.WebUI.Controllers
{
    public class HomeController : Controller
    {
        private readonly IWordApiService _wordService;

        public HomeController(IWordApiService wordService)
        {
            _wordService = wordService;
        }

        private string? GetToken() => User.FindFirstValue("Token");
        private string? GetUserId() => User.FindFirstValue(ClaimTypes.NameIdentifier);

        public async Task<IActionResult> Index()
        {
            var userId = GetUserId();
            var token = GetToken();

            if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(token))
            {
                return View(null);
            }

            List<string> listNames = new();

            try
            {
                var lists = await _wordService.GetListNamesAsync(userId, token);
                listNames = lists.ToList();
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Could not retrieve your lists: {ex.Message}";
            }

            return View(listNames);
        }

        [HttpPost]
        public async Task<IActionResult> CreateList(Word word)
        {
            var userId = GetUserId();
            var token = GetToken();

            if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(token))
                return RedirectToAction("Login", "Account");

            word.UserId = userId;

            if (string.IsNullOrWhiteSpace(word.ListName) || string.IsNullOrWhiteSpace(word.En) || string.IsNullOrWhiteSpace(word.Tr))
            {
                TempData["Error"] = "List name and the first word's English/Turkish translations cannot be empty.";
                return RedirectToAction("Index");
            }

            if (await _wordService.CreateWordAsync(word, token))
            {
                TempData["Success"] = $"List '{word.ListName}' has been created successfully!";
            }
            else
            {
                TempData["Error"] = "Error creating list (adding first word).";
            }

            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> RenameList(string oldListName, string newListName)
        {
            var userId = GetUserId();
            var token = GetToken();

            if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(token))
                return RedirectToAction("Login", "Account");

            if (string.IsNullOrWhiteSpace(oldListName) || string.IsNullOrWhiteSpace(newListName))
            {
                TempData["Error"] = "Old and new list names cannot be empty.";
                return RedirectToAction("Index");
            }

            if (await _wordService.UpdateListNameAsync(userId, oldListName, newListName, token))
            {
                TempData["Success"] = "List name has been updated successfully.";
            }
            else
            {
                TempData["Error"] = "Error updating list name.";
            }

            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> DeleteList(string listName)
        {
            var userId = GetUserId();
            var token = GetToken();

            if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(token))
                return RedirectToAction("Login", "Account");

            if (string.IsNullOrWhiteSpace(listName))
            {
                TempData["Error"] = "List name to delete was not specified.";
                return RedirectToAction("Index");
            }

            if (await _wordService.DeleteListAsync(userId, listName, token))
            {
                TempData["Success"] = $"List '{listName}' has been deleted successfully.";
            }
            else
            {
                TempData["Error"] = "Error deleting list.";
            }

            return RedirectToAction("Index");
        }

        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = System.Diagnostics.Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}