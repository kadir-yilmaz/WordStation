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
    public class QuizHistoryController : ControllerBase
    {
        private readonly IQuizHistoryService _quizHistoryService;

        public QuizHistoryController(IQuizHistoryService quizHistoryService)
        {
            _quizHistoryService = quizHistoryService;
        }

        private string? ResolveUserId(string? requestedUserId)
        {
            if (!string.IsNullOrWhiteSpace(requestedUserId))
                return requestedUserId.Trim();

            return User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                ?? User.FindFirst(ClaimTypes.Email)?.Value
                ?? User.Identity?.Name;
        }

        // GET: api/quizhistory?userId=xxx&isDailyQuiz=true|false
        [HttpGet]
        public async Task<IActionResult> GetHistory([FromQuery] string? userId, [FromQuery] bool? isDailyQuiz)
        {
            try
            {
                var effectiveUserId = ResolveUserId(userId);
                if (string.IsNullOrWhiteSpace(effectiveUserId))
                    return BadRequest("Kullanıcı ID gereklidir.");

                var history = await _quizHistoryService.GetHistoryAsync(effectiveUserId, isDailyQuiz);
                return Ok(history);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message, details = ex.InnerException?.Message });
            }
        }

        // POST: api/quizhistory
        [HttpPost]
        public async Task<IActionResult> SaveHistory([FromBody] CreateQuizHistoryDto dto)
        {
            try
            {
                if (dto == null)
                    return BadRequest("Geçersiz istek.");

                var effectiveUserId = ResolveUserId(dto.UserId);
                if (string.IsNullOrWhiteSpace(effectiveUserId))
                    return BadRequest("Kullanıcı ID gereklidir.");

                dto.UserId = effectiveUserId;

                var saved = await _quizHistoryService.SaveHistoryAsync(dto);
                return Ok(saved);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message, details = ex.InnerException?.Message });
            }
        }

        // DELETE: api/quizhistory?userId=xxx&isDailyQuiz=true|false
        [HttpDelete]
        public async Task<IActionResult> ClearHistory([FromQuery] string? userId, [FromQuery] bool? isDailyQuiz)
        {
            try
            {
                var effectiveUserId = ResolveUserId(userId);
                if (string.IsNullOrWhiteSpace(effectiveUserId))
                    return BadRequest("Kullanıcı ID gereklidir.");

                var result = await _quizHistoryService.ClearHistoryAsync(effectiveUserId, isDailyQuiz);
                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message, details = ex.InnerException?.Message });
            }
        }
    }
}
