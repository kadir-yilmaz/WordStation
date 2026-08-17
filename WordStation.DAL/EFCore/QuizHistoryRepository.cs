using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using WordStation.DAL.Abstract;
using WordStation.EL.Models;

namespace WordStation.DAL.EFCore
{
    public class QuizHistoryRepository : IQuizHistoryRepository
    {
        private readonly AppDbContext _context;

        public QuizHistoryRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<QuizHistory>> GetHistoryByUserIdAsync(string userId, bool? isDailyQuiz = null, int limit = 50, bool trackChanges = false)
        {
            var query = _context.QuizHistories.Where(h => h.UserId == userId);

            if (isDailyQuiz.HasValue)
            {
                query = query.Where(h => h.IsDailyQuiz == isDailyQuiz.Value);
            }

            query = query.OrderByDescending(h => h.Date).Take(limit);

            return trackChanges
                ? await query.ToListAsync()
                : await query.AsNoTracking().ToListAsync();
        }

        public void CreateHistory(QuizHistory history) => _context.QuizHistories.Add(history);

        public async Task DeleteHistoryAsync(string userId, bool? isDailyQuiz = null)
        {
            var query = _context.QuizHistories.Where(h => h.UserId == userId);

            if (isDailyQuiz.HasValue)
            {
                query = query.Where(h => h.IsDailyQuiz == isDailyQuiz.Value);
            }

            var items = await query.ToListAsync();
            if (items.Any())
            {
                _context.QuizHistories.RemoveRange(items);
            }
        }

        public async Task SaveAsync() => await _context.SaveChangesAsync();
    }
}
