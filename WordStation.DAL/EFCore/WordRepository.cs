using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using WordStation.DAL.Abstract;
using WordStation.EL.Models;

namespace WordStation.DAL.Abstract.EFCore
{
    public class WordRepository : IWordRepository
    {
        private readonly AppDbContext _context;

        public WordRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<Word>> GetAllWordsAsync(bool trackChanges)
        {
            var query = _context.Words.AsQueryable();

            return trackChanges
                ? await query.ToListAsync()
                : await query.AsNoTracking().ToListAsync();
        }

        public async Task<List<Word>> GetWordsByConditionAsync(Expression<Func<Word, bool>> expression, bool trackChanges)
        {
            var query = _context.Words.Where(expression);

            return trackChanges
                ? await query.ToListAsync()
                : await query.AsNoTracking().ToListAsync();
        }

        public void CreateWord(Word entity) => _context.Words.Add(entity);

        public void UpdateWord(Word entity) => _context.Words.Update(entity);

        public void DeleteWord(Word entity) => _context.Words.Remove(entity);

        public async Task SaveAsync() => await _context.SaveChangesAsync();
    }
}

