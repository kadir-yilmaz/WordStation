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

        // GET: api/dailyquiz/plans?userId=xxx
        [HttpGet("plans")]
        public async Task<IActionResult> GetAllPlans([FromQuery] string? userId)
        {
            try
            {
                var effectiveUserId = ResolveUserId(userId);
                if (string.IsNullOrWhiteSpace(effectiveUserId))
                    return BadRequest("Kullanıcı ID gereklidir.");

                var plans = await _dailyQuizService.GetAllPlansByUserIdAsync(effectiveUserId);
                return Ok(plans);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message, details = ex.InnerException?.Message });
            }
        }

        // GET: api/dailyquiz/plans/{id}?userId=xxx
        [HttpGet("plans/{id}")]
        public async Task<IActionResult> GetPlanById(int id, [FromQuery] string? userId)
        {
            try
            {
                var effectiveUserId = ResolveUserId(userId);
                if (string.IsNullOrWhiteSpace(effectiveUserId))
                    return BadRequest("Kullanıcı ID gereklidir.");

                var plan = await _dailyQuizService.GetPlanByIdAsync(id, effectiveUserId);
                if (plan == null)
                    return NotFound("Plan bulunamadı.");

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

                var plan = await _dailyQuizService.CreatePlanAsync(dto);
                return Ok(plan);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message, details = ex.InnerException?.Message });
            }
        }

        // POST: api/dailyquiz/plans
        [HttpPost("plans")]
        public async Task<IActionResult> CreatePlan([FromBody] CreateDailyQuizPlanDto dto)
        {
            try
            {
                if (dto == null)
                    return BadRequest("Geçersiz istek.");

                var effectiveUserId = ResolveUserId(dto.UserId);
                if (string.IsNullOrWhiteSpace(effectiveUserId))
                    return BadRequest("Kullanıcı ID gereklidir.");

                dto.UserId = effectiveUserId;

                var plan = await _dailyQuizService.CreatePlanAsync(dto);
                return Ok(plan);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message, details = ex.InnerException?.Message });
            }
        }

        // POST: api/dailyquiz/plans/{id}/activate?userId=xxx
        [HttpPost("plans/{id}/activate")]
        public async Task<IActionResult> SetActivePlan(int id, [FromQuery] string? userId)
        {
            try
            {
                var effectiveUserId = ResolveUserId(userId);
                if (string.IsNullOrWhiteSpace(effectiveUserId))
                    return BadRequest("Kullanıcı ID gereklidir.");

                var success = await _dailyQuizService.SetActivePlanAsync(id, effectiveUserId);
                if (!success)
                    return NotFound("Plan bulunamadı.");

                return Ok(new { success = true, planId = id });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message, details = ex.InnerException?.Message });
            }
        }

        // POST: api/dailyquiz/plans/{id}/buffet-words
        [HttpPost("plans/{id}/buffet-words")]
        public async Task<IActionResult> SelectBuffetWords(int id, [FromBody] SelectBuffetWordsDto dto)
        {
            try
            {
                if (dto == null)
                    return BadRequest("Geçersiz istek.");

                var effectiveUserId = ResolveUserId(dto.UserId);
                if (string.IsNullOrWhiteSpace(effectiveUserId))
                    return BadRequest("Kullanıcı ID gereklidir.");

                dto.UserId = effectiveUserId;

                var plan = await _dailyQuizService.SelectBuffetWordsAsync(id, effectiveUserId, dto);
                if (plan == null)
                    return NotFound("Plan bulunamadı.");

                return Ok(plan);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message, details = ex.InnerException?.Message });
            }
        }

        // POST: api/dailyquiz/plans/{id}/return-word/{wordId}?userId=xxx
        [HttpPost("plans/{id}/return-word/{wordId}")]
        public async Task<IActionResult> ReturnWordToBuffetPool(int id, int wordId, [FromQuery] string? userId)
        {
            try
            {
                var effectiveUserId = ResolveUserId(userId);
                if (string.IsNullOrWhiteSpace(effectiveUserId))
                    return BadRequest("Kullanıcı ID gereklidir.");

                var plan = await _dailyQuizService.ReturnWordToBuffetPoolAsync(id, effectiveUserId, wordId);
                if (plan == null)
                    return NotFound("Plan bulunamadı.");

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

        // PUT: api/dailyquiz/plans/{id}/reset?userId=xxx
        [HttpPut("plans/{id}/reset")]
        public async Task<IActionResult> ResetPlan(int id, [FromQuery] string? userId)
        {
            try
            {
                var effectiveUserId = ResolveUserId(userId);
                if (string.IsNullOrWhiteSpace(effectiveUserId))
                    return BadRequest("Kullanıcı ID gereklidir.");

                var plan = await _dailyQuizService.ResetPlanProgressAsync(id, effectiveUserId);
                if (plan == null)
                    return NotFound("Plan bulunamadı.");

                return Ok(plan);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message, details = ex.InnerException?.Message });
            }
        }

        // DELETE: api/dailyquiz/plans/{id}?userId=xxx
        [HttpDelete("plans/{id}")]
        public async Task<IActionResult> DeletePlanById(int id, [FromQuery] string? userId)
        {
            try
            {
                var effectiveUserId = ResolveUserId(userId);
                if (string.IsNullOrWhiteSpace(effectiveUserId))
                    return BadRequest("Kullanıcı ID gereklidir.");

                var result = await _dailyQuizService.DeletePlanAsync(id, effectiveUserId);
                if (!result)
                    return NotFound("Plan bulunamadı.");

                return NoContent();
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
