using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WordStation.BLL.Abstract;

namespace WordStation.WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class SynonymGroupsController : ControllerBase
    {
        private readonly ISynonymGroupService _service;

        public SynonymGroupsController(ISynonymGroupService service)
        {
            _service = service;
        }

        /// <summary>
        /// Kullanıcının tüm eş anlam gruplarını getirir
        /// GET: api/synonymgroups?userId=xxx
        /// </summary>
        [HttpGet]
        public IActionResult GetAllGroups([FromQuery] string userId)
        {
            if (string.IsNullOrWhiteSpace(userId))
                return BadRequest("Kullanıcı ID gereklidir.");

            var groups = _service.GetAllGroups(userId);
            return Ok(groups);
        }

        /// <summary>
        /// ID'ye göre grup getirir
        /// GET: api/synonymgroups/{id}?userId=xxx
        /// </summary>
        [HttpGet("{id:int}")]
        public IActionResult GetGroupById(int id, [FromQuery] string userId)
        {
            if (string.IsNullOrWhiteSpace(userId))
                return BadRequest("Kullanıcı ID gereklidir.");

            var group = _service.GetGroupById(id, userId);
            if (group == null)
                return NotFound();

            return Ok(group);
        }

        /// <summary>
        /// Bir kelimenin eş anlamlılarını getirir
        /// GET: api/synonymgroups/words/{wordId}/synonyms?userId=xxx
        /// </summary>
        [HttpGet("words/{wordId:int}/synonyms")]
        public IActionResult GetSynonymsForWord(int wordId, [FromQuery] string userId)
        {
            if (string.IsNullOrWhiteSpace(userId))
                return BadRequest("Kullanıcı ID gereklidir.");

            var synonyms = _service.GetSynonymsForWord(wordId, userId);
            return Ok(synonyms);
        }

        /// <summary>
        /// Yeni eş anlam grubu oluşturur
        /// POST: api/synonymgroups
        /// Body: { "name": "Reddetme Grubu", "wordIds": [1, 2, 3], "userId": "xxx" }
        /// </summary>
        [HttpPost]
        public IActionResult CreateGroup([FromBody] CreateGroupRequest request)
        {
            if (request == null || request.WordIds == null || request.WordIds.Count < 2)
                return BadRequest("En az 2 kelime seçilmelidir.");

            if (string.IsNullOrWhiteSpace(request.UserId))
                return BadRequest("Kullanıcı ID gereklidir.");

            var group = _service.CreateGroup(request.Name, request.WordIds, request.UserId);
            return CreatedAtAction(nameof(GetGroupById), new { id = group.Id, userId = request.UserId }, group);
        }

        /// <summary>
        /// Gruba kelime ekler
        /// POST: api/synonymgroups/{id}/words
        /// Body: { "wordId": 5, "userId": "xxx" }
        /// </summary>
        [HttpPost("{id:int}/words")]
        public IActionResult AddWordToGroup(int id, [FromBody] WordGroupRequest request)
        {
            if (request == null || request.WordId <= 0)
                return BadRequest("Geçersiz kelime ID.");

            if (string.IsNullOrWhiteSpace(request.UserId))
                return BadRequest("Kullanıcı ID gereklidir.");

            try
            {
                _service.AddWordToGroup(id, request.WordId, request.UserId);
                return NoContent();
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
        }

        /// <summary>
        /// Gruptan kelime çıkarır
        /// DELETE: api/synonymgroups/{id}/words/{wordId}?userId=xxx
        /// </summary>
        [HttpDelete("{id:int}/words/{wordId:int}")]
        public IActionResult RemoveWordFromGroup(int id, int wordId, [FromQuery] string userId)
        {
            if (string.IsNullOrWhiteSpace(userId))
                return BadRequest("Kullanıcı ID gereklidir.");

            try
            {
                _service.RemoveWordFromGroup(id, wordId, userId);
                return NoContent();
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
        }

        /// <summary>
        /// Grup adını günceller
        /// PATCH: api/synonymgroups/{id}
        /// Body: { "name": "Yeni Ad", "userId": "xxx" }
        /// </summary>
        [HttpPatch("{id:int}")]
        public IActionResult UpdateGroupName(int id, [FromBody] UpdateGroupRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.UserId))
                return BadRequest("Kullanıcı ID gereklidir.");

            try
            {
                _service.UpdateGroupName(id, request.Name, request.UserId);
                return NoContent();
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
        }

        /// <summary>
        /// Grubu siler
        /// DELETE: api/synonymgroups/{id}?userId=xxx
        /// </summary>
        [HttpDelete("{id:int}")]
        public IActionResult DeleteGroup(int id, [FromQuery] string userId)
        {
            if (string.IsNullOrWhiteSpace(userId))
                return BadRequest("Kullanıcı ID gereklidir.");

            try
            {
                _service.DeleteGroup(id, userId);
                return NoContent();
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
        }
    }

    // Request DTOs
    public class CreateGroupRequest
    {
        public string? Name { get; set; }
        public List<int> WordIds { get; set; } = new();
        public string UserId { get; set; } = string.Empty;
    }

    public class WordGroupRequest
    {
        public int WordId { get; set; }
        public string UserId { get; set; } = string.Empty;
    }

    public class UpdateGroupRequest
    {
        public string? Name { get; set; }
        public string UserId { get; set; } = string.Empty;
    }
}
