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
    public class DailyWordController : ControllerBase
    {
        private readonly IDailyWordService _dailyWordService;

        public DailyWordController(IDailyWordService dailyWordService)
        {
            _dailyWordService = dailyWordService;
        }

        private string? ResolveUserId(string? requestedUserId)
        {
            if (!string.IsNullOrWhiteSpace(requestedUserId))
                return requestedUserId.Trim();

            return User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                ?? User.FindFirst(ClaimTypes.Email)?.Value
                ?? User.Identity?.Name;
        }

        // GET: api/dailyword?userId=x&listName=y
        [HttpGet]
        public async Task<IActionResult> GetSession([FromQuery] string? userId, [FromQuery] string listName)
        {
            try
            {
                var effectiveUserId = ResolveUserId(userId);
                if (string.IsNullOrWhiteSpace(effectiveUserId))
                    return BadRequest("Kullanıcı ID gereklidir.");

                if (string.IsNullOrWhiteSpace(listName))
                    return BadRequest("Liste adı gereklidir.");

                var session = await _dailyWordService.GetSessionAsync(effectiveUserId, listName);
                return Ok(session); // null olabilir
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message, details = ex.InnerException?.Message });
            }
        }

        // POST: api/dailyword/init
        [HttpPost("init")]
        public async Task<IActionResult> InitializeSession([FromBody] InitDailyWordSessionDto dto)
        {
            try
            {
                if (dto == null)
                    return BadRequest("Geçersiz istek.");

                var effectiveUserId = ResolveUserId(dto.UserId);
                if (string.IsNullOrWhiteSpace(effectiveUserId))
                    return BadRequest("Kullanıcı ID gereklidir.");

                dto.UserId = effectiveUserId;

                var session = await _dailyWordService.InitializeSessionAsync(dto.UserId, dto.ListName);
                return Ok(session);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message, details = ex.InnerException?.Message });
            }
        }

        // POST: api/dailyword/add
        [HttpPost("add")]
        public async Task<IActionResult> AddToDaily([FromBody] AddToDailyDto dto)
        {
            try
            {
                if (dto == null || dto.WordIds == null || dto.WordIds.Count == 0)
                    return BadRequest("Eklenecek kelime belirtilmedi.");

                var effectiveUserId = ResolveUserId(dto.UserId);
                if (string.IsNullOrWhiteSpace(effectiveUserId))
                    return BadRequest("Kullanıcı ID gereklidir.");

                dto.UserId = effectiveUserId;

                var session = await _dailyWordService.AddToDailyAsync(dto);
                return Ok(session);
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

        // POST: api/dailyword/remove
        [HttpPost("remove")]
        public async Task<IActionResult> RemoveFromDaily([FromBody] RemoveFromDailyDto dto)
        {
            try
            {
                if (dto == null || dto.WordIds == null || dto.WordIds.Count == 0)
                    return BadRequest("Çıkarılacak kelime belirtilmedi.");

                var effectiveUserId = ResolveUserId(dto.UserId);
                if (string.IsNullOrWhiteSpace(effectiveUserId))
                    return BadRequest("Kullanıcı ID gereklidir.");

                dto.UserId = effectiveUserId;

                var session = await _dailyWordService.RemoveFromDailyAsync(dto);
                return Ok(session);
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

        // POST: api/dailyword/complete
        [HttpPost("complete")]
        public async Task<IActionResult> CompleteWord([FromBody] CompleteDailyWordDto dto)
        {
            try
            {
                if (dto == null)
                    return BadRequest("Geçersiz istek.");

                var effectiveUserId = ResolveUserId(dto.UserId);
                if (string.IsNullOrWhiteSpace(effectiveUserId))
                    return BadRequest("Kullanıcı ID gereklidir.");

                dto.UserId = effectiveUserId;

                var session = await _dailyWordService.CompleteWordAsync(dto);
                return Ok(session);
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

        // POST: api/dailyword/complete-bulk
        [HttpPost("complete-bulk")]
        public async Task<IActionResult> CompleteWords([FromBody] CompleteDailyWordsDto dto)
        {
            try
            {
                if (dto == null || dto.WordIds == null || dto.WordIds.Count == 0)
                    return BadRequest("Kelime seçilmedi.");

                var effectiveUserId = ResolveUserId(dto.UserId);
                if (string.IsNullOrWhiteSpace(effectiveUserId))
                    return BadRequest("Kullanıcı ID gereklidir.");

                dto.UserId = effectiveUserId;

                var session = await _dailyWordService.CompleteWordsAsync(dto);
                return Ok(session);
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

        // DELETE: api/dailyword?userId=x&listName=y
        [HttpDelete]
        public async Task<IActionResult> DeleteSession([FromQuery] string? userId, [FromQuery] string listName)
        {
            try
            {
                var effectiveUserId = ResolveUserId(userId);
                if (string.IsNullOrWhiteSpace(effectiveUserId))
                    return BadRequest("Kullanıcı ID gereklidir.");

                var result = await _dailyWordService.DeleteSessionAsync(effectiveUserId, listName);
                if (!result)
                    return NotFound("Session bulunamadı.");

                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message, details = ex.InnerException?.Message });
            }
        }
    }
}
