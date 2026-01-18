using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WordStation.BLL.Abstract;
using WordStation.EL.Models;

namespace WordStation.WebAPI.Controllers
{
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
        public IActionResult GetAllWords([FromQuery] string userId, [FromQuery] string listName)
        {
            if (string.IsNullOrWhiteSpace(userId) || string.IsNullOrWhiteSpace(listName))
                return BadRequest("Kullanıcı ID ve liste adı gereklidir.");

            var words = _wordService.GetAllWords(userId, listName);
            return Ok(words);
        }

        // GET: api/words/search?en=xxx&userId=yyy&listName=zzz&searchMode=starts|contains
        [HttpGet("search")]
        public IActionResult SearchWord([FromQuery] string en, [FromQuery] string userId, [FromQuery] string listName, [FromQuery] string searchMode = "starts")
        {
            if (string.IsNullOrWhiteSpace(en) || string.IsNullOrWhiteSpace(userId) || string.IsNullOrWhiteSpace(listName))
                return BadRequest("Arama kriterleri eksik.");

            var results = _wordService.SearchWord(en, userId, listName, searchMode);
            return Ok(results);
        }

        // GET: api/words/lists?userId=xxx
        [HttpGet("lists")]
        public IActionResult GetListNames([FromQuery] string userId)
        {
            if (string.IsNullOrWhiteSpace(userId))
                return BadRequest("Kullanıcı ID gereklidir.");

            var listNames = _wordService.GetListNames(userId);
            return Ok(listNames);
        }

        // POST: api/words
        [HttpPost]
        public IActionResult CreateWord([FromBody] Word word)
        {
            if (word == null || string.IsNullOrWhiteSpace(word.En))
                return BadRequest("Geçersiz kelime verisi.");

            _wordService.CreateWord(word);
            return CreatedAtAction(nameof(GetAllWords), new { userId = word.UserId, listName = word.ListName }, word);
        }

        // PUT: api/words
        [HttpPut]
        public IActionResult UpdateWord([FromBody] Word word)
        {
            if (word == null || word.Id <= 0)
                return BadRequest("Geçersiz kelime verisi.");

            _wordService.UpdateWord(word);
            return NoContent();
        }

        // DELETE: api/words/{id}
        [HttpDelete("{id:int}")]
        public IActionResult DeleteWord(int id)
        {
            if (id <= 0)
                return BadRequest("Geçersiz ID.");

            _wordService.DeleteWord(id);
            return NoContent();
        }

        // PUT: api/words/lists/rename?userId=xxx&listName=yyy&newListName=zzz
        [HttpPut("lists/rename")]
        public IActionResult UpdateListName([FromQuery] string userId, [FromQuery] string listName, [FromQuery] string newListName)
        {
            if (string.IsNullOrWhiteSpace(userId) || string.IsNullOrWhiteSpace(listName) || string.IsNullOrWhiteSpace(newListName))
                return BadRequest("Tüm parametreler gereklidir.");

            _wordService.UpdateListName(listName, newListName, userId);
            return NoContent();
        }

        // DELETE: api/words/lists?userId=xxx&listName=yyy
        [HttpDelete("lists")]
        public IActionResult DeleteList([FromQuery] string userId, [FromQuery] string listName)
        {
            if (string.IsNullOrWhiteSpace(userId) || string.IsNullOrWhiteSpace(listName))
                return BadRequest("Kullanıcı ID ve liste adı gereklidir.");

            _wordService.DeleteList(listName, userId);
            return NoContent();
        }
    }
}
