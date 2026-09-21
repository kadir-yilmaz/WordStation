using System.Linq;
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
            if (!trackChanges)
                query = query.AsNoTracking();

            return await query
                .OrderByDescending(p => p.UpdatedAt)
                .FirstOrDefaultAsync(p => p.UserId == userId);
        }

        public void CreatePlan(DailyQuizPlan plan) => _context.DailyQuizPlans.Add(plan);

        public void UpdatePlan(DailyQuizPlan plan) => _context.DailyQuizPlans.Update(plan);

        public void DeletePlan(DailyQuizPlan plan) => _context.DailyQuizPlans.Remove(plan);

        public async Task<System.Collections.Generic.List<DailyPlanDayHistory>> GetDayHistoriesByPlanIdAsync(int planId)
        {
            return await _context.DailyPlanDayHistories
                .AsNoTracking()
                .Where(h => h.DailyQuizPlanId == planId)
                .OrderByDescending(h => h.DayNumber)
                .ThenByDescending(h => h.CompletedAt)
                .ToListAsync();
        }

        public void AddDayHistory(DailyPlanDayHistory dayHistory) => _context.DailyPlanDayHistories.Add(dayHistory);

        public async Task SaveAsync() => await _context.SaveChangesAsync();
    }
}
