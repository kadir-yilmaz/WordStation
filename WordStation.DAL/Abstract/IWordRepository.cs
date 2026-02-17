using System.Linq.Expressions;
using WordStation.EL.Models;

namespace WordStation.DAL.Abstract
{
    public interface IWordRepository
    {
        Task<List<Word>> GetAllWordsAsync(bool trackChanges);
        Task<List<Word>> GetWordsByConditionAsync(Expression<Func<Word, bool>> expression, bool trackChanges);
        void CreateWord(Word entity);
        void UpdateWord(Word entity);
        void DeleteWord(Word entity);
        Task SaveAsync();
    }
}

