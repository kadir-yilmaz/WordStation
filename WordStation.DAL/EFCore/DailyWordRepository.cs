using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using WordStation.DAL.Abstract;
using WordStation.EL.Models;

namespace WordStation.DAL.EFCore
{
    public class DailyWordRepository : IDailyWordRepository
    {
        private readonly AppDbContext _context;

        public DailyWordRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<DailyWordSession?> GetByUserAndListAsync(string userId, string listName, bool trackChanges = false)
        {
            var query = _context.DailyWordSessions
                .Include(s => s.SessionItems)
                .ThenInclude(si => si.Word)
                .AsQueryable();
                
            if (!trackChanges)
                query = query.AsNoTracking();

            return await query.FirstOrDefaultAsync(s => s.UserId == userId && s.ListName == listName);
        }

        public void Create(DailyWordSession session) => _context.DailyWordSessions.Add(session);

        public void Update(DailyWordSession session) => _context.DailyWordSessions.Update(session);

        public void Delete(DailyWordSession session) => _context.DailyWordSessions.Remove(session);

        public async Task SaveAsync() => await _context.SaveChangesAsync();
    }
}
