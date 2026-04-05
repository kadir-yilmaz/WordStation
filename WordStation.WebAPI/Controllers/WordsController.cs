using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WordStation.BLL.Abstract;
using WordStation.EL.Models;

namespace WordStation.WebAPI.Controllers
{
    // Test: Multi-Deploy WebDeploy 1.0.8 (Automated)
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]  // JWT token kontrolü - Expired veya geçersiz token 401 döner
    public class WordsController : ControllerBase
    {
        private readonly IWordService _wordService;

        public WordsController(IWordService wordService)
        {
            _wordService = wordService;
        }

        // GET: api/words?userId=xxx&listName=yyy
        [HttpGet]
        public async Task<IActionResult> GetAllWords([FromQuery] string userId, [FromQuery] string listName)
        {
            if (string.IsNullOrWhiteSpace(userId) || string.IsNullOrWhiteSpace(listName))
                return BadRequest("Kullanıcı ID ve liste adı gereklidir.");

            var words = await _wordService.GetAllWordsAsync(userId, listName);
            return Ok(words);
        }

        // GET: api/words/search?en=xxx&userId=yyy&listName=zzz&searchMode=starts|contains
        [HttpGet("search")]
        public async Task<IActionResult> SearchWord([FromQuery] string en, [FromQuery] string userId, [FromQuery] string listName, [FromQuery] string searchMode = "starts")
        {
            if (string.IsNullOrWhiteSpace(en) || string.IsNullOrWhiteSpace(userId) || string.IsNullOrWhiteSpace(listName))
                return BadRequest("Arama kriterleri eksik.");

            var results = await _wordService.SearchWordAsync(en, userId, listName, searchMode);
            return Ok(results);
        }

        // GET: api/words/lists?userId=xxx
        [HttpGet("lists")]
        public async Task<IActionResult> GetListNames([FromQuery] string userId)
        {
            if (string.IsNullOrWhiteSpace(userId))
                return BadRequest("Kullanıcı ID gereklidir.");

            var listNames = await _wordService.GetListNamesAsync(userId);
            return Ok(listNames);
        }

        // POST: api/words
        [HttpPost]
        public async Task<IActionResult> CreateWord([FromBody] Word word)
        {
            if (word == null || string.IsNullOrWhiteSpace(word.En))
                return BadRequest("Geçersiz kelime verisi.");

            await _wordService.CreateWordAsync(word);
            return CreatedAtAction(nameof(GetAllWords), new { userId = word.UserId, listName = word.ListName }, word);
        }

        // PUT: api/words
        [HttpPut]
        public async Task<IActionResult> UpdateWord([FromBody] Word word)
        {
            if (word == null || word.Id <= 0)
                return BadRequest("Geçersiz kelime verisi.");

            await _wordService.UpdateWordAsync(word);
            return NoContent();
        }

        // DELETE: api/words/{id}
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> DeleteWord(int id)
        {
            if (id <= 0)
                return BadRequest("Geçersiz ID.");

            await _wordService.DeleteWordAsync(id);
            return NoContent();
        }

        // PUT: api/words/lists/rename?userId=xxx&listName=yyy&newListName=zzz
        [HttpPut("lists/rename")]
        public async Task<IActionResult> UpdateListName([FromQuery] string userId, [FromQuery] string listName, [FromQuery] string newListName)
        {
            if (string.IsNullOrWhiteSpace(userId) || string.IsNullOrWhiteSpace(listName) || string.IsNullOrWhiteSpace(newListName))
                return BadRequest("Tüm parametreler gereklidir.");

            await _wordService.UpdateListNameAsync(listName, newListName, userId);
            return NoContent();
        }

        // DELETE: api/words/lists?userId=xxx&listName=yyy
        [HttpDelete("lists")]
        public async Task<IActionResult> DeleteList([FromQuery] string userId, [FromQuery] string listName)
        {
            if (string.IsNullOrWhiteSpace(userId) || string.IsNullOrWhiteSpace(listName))
                return BadRequest("Kullanıcı ID ve liste adı gereklidir.");

            await _wordService.DeleteListAsync(listName, userId);
            return NoContent();
        }
    }
}

