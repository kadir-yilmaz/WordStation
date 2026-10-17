using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WordStation.BLL.Abstract;
using WordStation.EL.Dtos;

namespace WordStation.WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class StudyPlanController : ControllerBase
    {
        private readonly IStudyPlanService _studyPlanService;

        public StudyPlanController(IStudyPlanService studyPlanService)
        {
            _studyPlanService = studyPlanService;
        }

        private string? ResolveUserId(string? requestedUserId)
        {
            if (!string.IsNullOrWhiteSpace(requestedUserId))
                return requestedUserId.Trim();

            return User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                ?? User.FindFirst(ClaimTypes.Email)?.Value
                ?? User.Identity?.Name;
        }

        // GET: api/studyplan
        [HttpGet]
        public async Task<IActionResult> GetUserPlans([FromQuery] string? userId)
        {
            try
            {
                var effectiveUserId = ResolveUserId(userId);
                if (string.IsNullOrWhiteSpace(effectiveUserId))
                    return BadRequest("Kullanıcı ID gereklidir.");

                var plans = await _studyPlanService.GetUserPlansAsync(effectiveUserId);
                return Ok(plans);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message, details = ex.InnerException?.Message });
            }
        }

        // POST: api/studyplan
        [HttpPost]
        public async Task<IActionResult> CreatePlan([FromBody] CreateStudyPlanDto dto)
        {
            try
            {
                if (dto == null)
                    return BadRequest("Geçersiz istek.");

                var effectiveUserId = ResolveUserId(dto.UserId);
                if (string.IsNullOrWhiteSpace(effectiveUserId))
                    return BadRequest("Kullanıcı ID gereklidir.");

                dto.UserId = effectiveUserId;

                var plan = await _studyPlanService.CreatePlanAsync(dto);
                return Ok(plan);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message, details = ex.InnerException?.Message });
            }
        }

        // GET: api/studyplan/{id}
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetPlan(int id, [FromQuery] string? userId)
        {
            try
            {
                var effectiveUserId = ResolveUserId(userId);
                if (string.IsNullOrWhiteSpace(effectiveUserId))
                    return BadRequest("Kullanıcı ID gereklidir.");

                var plan = await _studyPlanService.GetPlanDetailAsync(id, effectiveUserId);
                if (plan == null)
                    return NotFound("Plan bulunamadı.");

                return Ok(plan);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message, details = ex.InnerException?.Message });
            }
        }

        // DELETE: api/studyplan/{id}
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> DeletePlan(int id, [FromQuery] string? userId)
        {
            try
            {
                var effectiveUserId = ResolveUserId(userId);
                if (string.IsNullOrWhiteSpace(effectiveUserId))
                    return BadRequest("Kullanıcı ID gereklidir.");

                var result = await _studyPlanService.DeletePlanAsync(id, effectiveUserId);
                if (!result)
                    return NotFound("Plan bulunamadı.");

                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message, details = ex.InnerException?.Message });
            }
        }

        // GET: api/studyplan/{id}/current-day
        [HttpGet("{id:int}/current-day")]
        public async Task<IActionResult> GetCurrentDay(int id, [FromQuery] string? userId)
        {
            try
            {
                var effectiveUserId = ResolveUserId(userId);
                if (string.IsNullOrWhiteSpace(effectiveUserId))
                    return BadRequest("Kullanıcı ID gereklidir.");

                var day = await _studyPlanService.GetCurrentDayAsync(id, effectiveUserId);
                return Ok(day);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message, details = ex.InnerException?.Message });
            }
        }

        // GET: api/studyplan/{id}/days
        [HttpGet("{id:int}/days")]
        public async Task<IActionResult> GetAllDays(int id, [FromQuery] string? userId)
        {
            try
            {
                var effectiveUserId = ResolveUserId(userId);
                if (string.IsNullOrWhiteSpace(effectiveUserId))
                    return BadRequest("Kullanıcı ID gereklidir.");

                var days = await _studyPlanService.GetAllDaysAsync(id, effectiveUserId);
                return Ok(days);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message, details = ex.InnerException?.Message });
            }
        }

        // GET: api/studyplan/{id}/day/{dayNumber}
        [HttpGet("{id:int}/day/{dayNumber:int}")]
        public async Task<IActionResult> GetDay(int id, int dayNumber, [FromQuery] string? userId)
        {
            try
            {
                var effectiveUserId = ResolveUserId(userId);
                if (string.IsNullOrWhiteSpace(effectiveUserId))
                    return BadRequest("Kullanıcı ID gereklidir.");

                var day = await _studyPlanService.GetDayAsync(id, dayNumber, effectiveUserId);
                if (day == null)
                    return NotFound("Gün bulunamadı.");

                return Ok(day);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message, details = ex.InnerException?.Message });
            }
        }

        // GET: api/studyplan/{id}/day/{dayNumber}/quiz
        [HttpGet("{id:int}/day/{dayNumber:int}/quiz")]
        public async Task<IActionResult> GenerateQuiz(int id, int dayNumber, [FromQuery] string? userId)
        {
            try
            {
                var effectiveUserId = ResolveUserId(userId);
                if (string.IsNullOrWhiteSpace(effectiveUserId))
                    return BadRequest("Kullanıcı ID gereklidir.");

                var quiz = await _studyPlanService.GenerateQuizAsync(id, dayNumber, effectiveUserId);
                return Ok(quiz);
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message, details = ex.InnerException?.Message });
            }
        }

        // POST: api/studyplan/{id}/day/{dayNumber}/quiz
        [HttpPost("{id:int}/day/{dayNumber:int}/quiz")]
        public async Task<IActionResult> SubmitQuiz(int id, int dayNumber, [FromBody] SubmitPlanQuizDto dto)
        {
            try
            {
                if (dto == null)
                    return BadRequest("Geçersiz istek.");

                var effectiveUserId = ResolveUserId(dto.UserId);
                if (string.IsNullOrWhiteSpace(effectiveUserId))
                    return BadRequest("Kullanıcı ID gereklidir.");

                dto.UserId = effectiveUserId;
                dto.StudyPlanId = id;
                dto.DayNumber = dayNumber;

                var result = await _studyPlanService.SubmitQuizAsync(dto);
                return Ok(result);
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message, details = ex.InnerException?.Message });
            }
        }

        // GET: api/studyplan/days/{dayId}/words
        [HttpGet("days/{dayId:int}/words")]
        public async Task<IActionResult> GetDayWords(int dayId, [FromQuery] string? userId)
        {
            try
            {
                var effectiveUserId = ResolveUserId(userId);
                if (string.IsNullOrWhiteSpace(effectiveUserId))
                    return BadRequest("Kullanıcı ID gereklidir.");

                var words = await _studyPlanService.GetDayWordsByDayIdAsync(dayId, effectiveUserId);
                return Ok(words);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message, details = ex.InnerException?.Message });
            }
        }

        // GET: api/studyplan/days/{dayId}/quiz
        [HttpGet("days/{dayId:int}/quiz")]
        public async Task<IActionResult> GetDayQuiz(int dayId, [FromQuery] string? userId)
        {
            try
            {
                var effectiveUserId = ResolveUserId(userId);
                if (string.IsNullOrWhiteSpace(effectiveUserId))
                    return BadRequest("Kullanıcı ID gereklidir.");

                var quiz = await _studyPlanService.GenerateQuizByDayIdAsync(dayId, effectiveUserId);
                return Ok(quiz.Questions);
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message, details = ex.InnerException?.Message });
            }
        }

        // POST: api/studyplan/days/{dayId}/submit
        [HttpPost("days/{dayId:int}/submit")]
        public async Task<IActionResult> SubmitDayQuiz(int dayId, [FromBody] SubmitDayQuizRequestDto dto, [FromQuery] string? userId)
        {
            try
            {
                if (dto == null)
                    return BadRequest("Geçersiz istek.");

                var effectiveUserId = ResolveUserId(userId);
                if (string.IsNullOrWhiteSpace(effectiveUserId))
                    return BadRequest("Kullanıcı ID gereklidir.");

                var result = await _studyPlanService.SubmitQuizByDayIdAsync(dayId, dto.Answers, effectiveUserId);
                return Ok(result);
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message, details = ex.InnerException?.Message });
            }
        }
    }
}
