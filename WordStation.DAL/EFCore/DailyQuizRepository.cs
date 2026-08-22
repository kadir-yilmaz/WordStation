using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using WordStation.DAL.Abstract;
using WordStation.EL.Models;

namespace WordStation.DAL.EFCore
{
    public class DailyQuizRepository : IDailyQuizRepository
    {
        private readonly AppDbContext _context;

        public DailyQuizRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<DailyQuizPlan?> GetPlanByUserIdAsync(string userId, bool trackChanges = false)
        {
            var query = _context.DailyQuizPlans.AsQueryable();

            return trackChanges
                ? await query.FirstOrDefaultAsync(p => p.UserId == userId)
                : await query.AsNoTracking().FirstOrDefaultAsync(p => p.UserId == userId);
        }

        public void CreatePlan(DailyQuizPlan plan) => _context.DailyQuizPlans.Add(plan);

        public void UpdatePlan(DailyQuizPlan plan) => _context.DailyQuizPlans.Update(plan);

        public void DeletePlan(DailyQuizPlan plan) => _context.DailyQuizPlans.Remove(plan);

        public async Task SaveAsync() => await _context.SaveChangesAsync();
    }
}
