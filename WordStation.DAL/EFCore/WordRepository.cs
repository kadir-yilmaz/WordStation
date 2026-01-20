using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using WordStation.DAL.Abstract;
using WordStation.EL.Models;

namespace WordStation.DAL.EFCore
{
    public class WordRepository : IWordRepository
    {
        private readonly AppDbContext _context;

        public WordRepository(AppDbContext context)
        {
            _context = context;
        }

        public IQueryable<Word> GetAllWords(bool trackChanges) =>
            trackChanges 
                ? _context.Words
                    .Include(w => w.SynonymWords)
                    .ThenInclude(sw => sw.SynonymGroup)
                    .ThenInclude(sg => sg.SynonymWords)
                    .ThenInclude(sw => sw.Word)
                : _context.Words
                    .Include(w => w.SynonymWords)
                    .ThenInclude(sw => sw.SynonymGroup)
                    .ThenInclude(sg => sg.SynonymWords)
                    .ThenInclude(sw => sw.Word)
                    .AsNoTrackingWithIdentityResolution();

        public IQueryable<Word> GetWordsByCondition(Expression<Func<Word, bool>> expression, bool trackChanges) =>
            trackChanges
                ? _context.Words.Where(expression)
                    .Include(w => w.SynonymWords)
                    .ThenInclude(sw => sw.SynonymGroup)
                    .ThenInclude(sg => sg.SynonymWords)
                    .ThenInclude(sw => sw.Word)
                : _context.Words.Where(expression)
                    .Include(w => w.SynonymWords)
                    .ThenInclude(sw => sw.SynonymGroup)
                    .ThenInclude(sg => sg.SynonymWords)
                    .ThenInclude(sw => sw.Word)
                    .AsNoTrackingWithIdentityResolution();

        public void CreateWord(Word entity) => _context.Words.Add(entity);

        public void UpdateWord(Word entity) => _context.Words.Update(entity);

        public void DeleteWord(Word entity) => _context.Words.Remove(entity);

        public void Save() => _context.SaveChanges();
    }
}
