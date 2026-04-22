using WordStation.WebUI.Models;

namespace WordStation.WebUI.Services.Abstract
{
    public interface IWordApiService
    {
        Task<IEnumerable<Word>> GetAllWordsAsync(string userId, string listName, string token);
        Task<IEnumerable<Word>> SearchWordAsync(string en, string userId, string listName, string token, string searchMode = "starts");
        Task<IEnumerable<string>> GetListNamesAsync(string userId, string token);
        Task<bool> CreateWordAsync(Word word, string token);
        Task<bool> UpdateWordAsync(Word word, string token);
        Task<bool> DeleteWordAsync(int id, string token);
        Task<bool> UpdateListNameAsync(string userId, string listName, string newListName, string token);
        Task<bool> DeleteListAsync(string userId, string listName, string token);
        Task<IEnumerable<WordGroupDto>> GetSynonymGroupsAsync(string userId, string token);
        Task<IEnumerable<Word>> GetAllWordsForUserAsync(string userId, string token);
    }
}
