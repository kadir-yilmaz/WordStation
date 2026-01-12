using WordStation.EL.Models;

namespace WordStation.BLL.Abstract
{
    public interface IWordService
    {
        // Query metodları
        IEnumerable<Word> GetAllWords(string userId, string listName);
        IEnumerable<Word> SearchWord(string en, string userId, string listName);
        IEnumerable<string> GetListNames(string userId);

        // CRUD metodları
        void CreateWord(Word word);
        void UpdateWord(Word word);
        void DeleteWord(int id);

        // List operasyonları
        void UpdateListName(string listName, string newListName, string userId);
        void DeleteList(string listName, string userId);
    }
}
