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
    public class DailyQuizController : ControllerBase
    {
        private readonly IDailyQuizService _dailyQuizService;

        public DailyQuizController(IDailyQuizService dailyQuizService)
        {
            _dailyQuizService = dailyQuizService;
        }

        private string? ResolveUserId(string? requestedUserId)
        {
            if (!string.IsNullOrWhiteSpace(requestedUserId))
                return requestedUserId.Trim();

            return User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                ?? User.FindFirst(ClaimTypes.Email)?.Value
                ?? User.Identity?.Name;
        }

        // GET: api/dailyquiz?userId=xxx
        [HttpGet]
        public async Task<IActionResult> GetActivePlan([FromQuery] string? userId)
        {
            try
            {
                var effectiveUserId = ResolveUserId(userId);
                if (string.IsNullOrWhiteSpace(effectiveUserId))
                    return BadRequest("Kullanıcı ID gereklidir.");

                var plan = await _dailyQuizService.GetActivePlanByUserIdAsync(effectiveUserId);
                return Ok(plan);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message, details = ex.InnerException?.Message });
            }
        }

        // POST: api/dailyquiz
        [HttpPost]
        public async Task<IActionResult> CreateOrResetPlan([FromBody] CreateDailyQuizPlanDto dto)
        {
            try
            {
                if (dto == null)
                    return BadRequest("Geçersiz istek.");

                var effectiveUserId = ResolveUserId(dto.UserId);
                if (string.IsNullOrWhiteSpace(effectiveUserId))
                    return BadRequest("Kullanıcı ID gereklidir.");

                dto.UserId = effectiveUserId;

                var plan = await _dailyQuizService.CreateOrResetPlanAsync(dto);
                return Ok(plan);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message, details = ex.InnerException?.Message });
            }
        }

        // PUT: api/dailyquiz/progress
        [HttpPut("progress")]
        public async Task<IActionResult> UpdateProgress([FromBody] UpdateDailyQuizProgressDto dto)
        {
            try
            {
                if (dto == null)
                    return BadRequest("Geçersiz istek.");

                var effectiveUserId = ResolveUserId(dto.UserId);
                if (string.IsNullOrWhiteSpace(effectiveUserId))
                    return BadRequest("Kullanıcı ID gereklidir.");

                dto.UserId = effectiveUserId;

                var plan = await _dailyQuizService.UpdateProgressAsync(dto);
                if (plan == null)
                    return NotFound("Kullanıcıya ait aktif günlük quiz planı bulunamadı.");

                return Ok(plan);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message, details = ex.InnerException?.Message });
            }
        }

        // DELETE: api/dailyquiz?userId=xxx
        [HttpDelete]
        public async Task<IActionResult> DeletePlan([FromQuery] string? userId)
        {
            try
            {
                var effectiveUserId = ResolveUserId(userId);
                if (string.IsNullOrWhiteSpace(effectiveUserId))
                    return BadRequest("Kullanıcı ID gereklidir.");

                var result = await _dailyQuizService.DeletePlanAsync(effectiveUserId);
                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message, details = ex.InnerException?.Message });
            }
        }
    }
}
