using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WordStation.EL.Dtos;
using WordStation.WebUI.Services.Abstract;

namespace WordStation.WebUI.Controllers
{
    [Authorize]
    public class StudyPlanController : Controller
    {
        private readonly IStudyPlanApiService _studyPlanService;
        private readonly IWordApiService _wordService;

        public StudyPlanController(IStudyPlanApiService studyPlanService, IWordApiService wordService)
        {
            _studyPlanService = studyPlanService;
            _wordService = wordService;
        }

        #region Auth Context

        private string? AccessToken => User.FindFirstValue("Token");
        private string? CurrentUserId => User.FindFirstValue(ClaimTypes.NameIdentifier);
        private bool IsAuthenticated => !string.IsNullOrEmpty(CurrentUserId) && !string.IsNullOrEmpty(AccessToken);

        private IActionResult RedirectToLogin() => RedirectToAction("Login", "Account");

        #endregion

        // GET: /StudyPlan
        public async Task<IActionResult> Index()
        {
            if (!IsAuthenticated) return RedirectToLogin();

            var plans = await _studyPlanService.GetUserPlansAsync(CurrentUserId!, AccessToken!);
            return View(plans);
        }

        // GET: /StudyPlan/Create
        public async Task<IActionResult> Create()
        {
            if (!IsAuthenticated) return RedirectToLogin();

            var lists = (await _wordService.GetListNamesAsync(CurrentUserId!, AccessToken!)).ToList();
            ViewBag.AvailableLists = lists;
            return View(new CreateStudyPlanDto { DailyWordLimit = 20 });
        }

        // POST: /StudyPlan/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateStudyPlanDto dto)
        {
            if (!IsAuthenticated) return RedirectToLogin();

            ModelState.Remove("Name");

            if (dto.ListNames == null || !dto.ListNames.Any())
            {
                ModelState.AddModelError("ListNames", "En az bir liste seçmelisiniz.");
            }

            if (string.IsNullOrWhiteSpace(dto.Name))
            {
                dto.Name = (dto.ListNames != null && dto.ListNames.Any()) 
                    ? string.Join(", ", dto.ListNames) + " Planı" 
                    : "Çalışma Planı";
            }

            if (!ModelState.IsValid)
            {
                ViewBag.AvailableLists = (await _wordService.GetListNamesAsync(CurrentUserId!, AccessToken!)).ToList();
                return View(dto);
            }

            dto.UserId = CurrentUserId;
            var created = await _studyPlanService.CreatePlanAsync(dto, AccessToken!);

            if (created == null)
            {
                ModelState.AddModelError("", "Plan oluşturulamadı. Seçilen listelerde yeterli kelime olduğundan emin olun.");
                ViewBag.AvailableLists = (await _wordService.GetListNamesAsync(CurrentUserId!, AccessToken!)).ToList();
                return View(dto);
            }

            TempData["SuccessMessage"] = "Çalışma planınız başarıyla oluşturuldu!";
            return RedirectToAction(nameof(Detail), new { id = created.Id });
        }

        // GET: /StudyPlan/Detail/{id}
        public async Task<IActionResult> Detail(int id)
        {
            if (!IsAuthenticated) return RedirectToLogin();

            var plans = await _studyPlanService.GetUserPlansAsync(CurrentUserId!, AccessToken!);
            var plan = plans.FirstOrDefault(p => p.Id == id);
            if (plan == null) return NotFound("Plan bulunamadı.");

            var days = await _studyPlanService.GetAllDaysAsync(id, CurrentUserId!, AccessToken!);

            ViewBag.Plan = plan;
            return View(days);
        }

        // POST: /StudyPlan/Delete/{id}
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            if (!IsAuthenticated) return RedirectToLogin();

            var success = await _studyPlanService.DeletePlanAsync(id, CurrentUserId!, AccessToken!);
            if (success)
            {
                TempData["SuccessMessage"] = "Plan başarıyla silindi.";
            }
            else
            {
                TempData["ErrorMessage"] = "Plan silinirken hata oluştu.";
            }

            return RedirectToAction(nameof(Index));
        }

        // GET: /StudyPlan/DayWords/{planId}?dayNumber={dayNumber} (AJAX modal için)
        [HttpGet]
        public async Task<IActionResult> DayWords(int planId, int dayNumber)
        {
            if (!IsAuthenticated) return Unauthorized();

            var day = await _studyPlanService.GetDayAsync(planId, dayNumber, CurrentUserId!, AccessToken!);
            if (day == null) return NotFound();

            return PartialView("_DayWordsModal", day);
        }

        // GET: /StudyPlan/Quiz/{id}?dayNumber={dayNumber}
        public async Task<IActionResult> Quiz(int id, int dayNumber)
        {
            if (!IsAuthenticated) return RedirectToLogin();

            var quiz = await _studyPlanService.GenerateQuizAsync(id, dayNumber, CurrentUserId!, AccessToken!);
            if (quiz == null || !quiz.Questions.Any())
            {
                TempData["ErrorMessage"] = "Bu gün için quiz soruları oluşturulamadı.";
                return RedirectToAction(nameof(Detail), new { id });
            }

            ViewBag.PlanId = id;
            ViewBag.DayNumber = dayNumber;
            return View(quiz);
        }

        // POST: /StudyPlan/SubmitQuiz/{id}?dayNumber={dayNumber} (AJAX)
        [HttpPost]
        public async Task<IActionResult> SubmitQuiz(int id, int dayNumber, [FromBody] SubmitPlanQuizDto dto)
        {
            if (!IsAuthenticated) return Unauthorized();

            dto.UserId = CurrentUserId;
            dto.StudyPlanId = id;
            dto.DayNumber = dayNumber;

            var result = await _studyPlanService.SubmitQuizAsync(id, dayNumber, dto, AccessToken!);
            if (result == null)
            {
                return BadRequest(new { message = "Quiz sonucu kaydedilemedi." });
            }

            var percent = result.QuizScore.HasValue && result.QuizScore.Value <= 100
                ? result.QuizScore.Value
                : ((result.QuizMaxScore.GetValueOrDefault() > 0)
                    ? (int)Math.Round((double)result.QuizScore.GetValueOrDefault() / result.QuizMaxScore.Value * 100)
                    : 0);

            return Json(new
            {
                success = true,
                score = percent,
                correctCount = result.CorrectCount,
                wrongCount = result.WrongCount,
                maxScore = 100
            });
        }
    }
}
