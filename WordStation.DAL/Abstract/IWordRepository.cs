using System.Linq.Expressions;
using WordStation.EL.Models;

namespace WordStation.DAL.Abstract
{
    public interface IWordRepository
    {
        // Query metodları
        IQueryable<Word> GetAllWords(bool trackChanges);
        IQueryable<Word> GetWordsByCondition(Expression<Func<Word, bool>> expression, bool trackChanges);

        // CRUD metodları
        void CreateWord(Word entity);
        void UpdateWord(Word entity);
        void DeleteWord(Word entity);

        // Persistence
        void Save();
    }
}
