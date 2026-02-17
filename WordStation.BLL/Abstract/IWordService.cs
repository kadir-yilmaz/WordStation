using WordStation.EL.Models;

namespace WordStation.BLL.Abstract
{
    public interface IWordService
    {
        // Query metodları
        Task<IEnumerable<Word>> GetAllWordsAsync(string userId, string listName);
        Task<IEnumerable<Word>> SearchWordAsync(string en, string userId, string listName, string searchMode = "starts");
        Task<IEnumerable<string>> GetListNamesAsync(string userId);

        // CRUD metodları
        Task CreateWordAsync(Word word);
        Task UpdateWordAsync(Word word);
        Task DeleteWordAsync(int id);

        // List operasyonları
        Task UpdateListNameAsync(string listName, string newListName, string userId);
        Task DeleteListAsync(string listName, string userId);
    }
}
