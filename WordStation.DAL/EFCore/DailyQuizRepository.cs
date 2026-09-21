using System.Collections.Generic;
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

        public async Task<DailyQuizPlan?> GetPlanByIdAsync(int id, bool trackChanges = false)
        {
            var query = _context.DailyQuizPlans.AsQueryable();
            return trackChanges
                ? await query.FirstOrDefaultAsync(p => p.Id == id)
                : await query.AsNoTracking().FirstOrDefaultAsync(p => p.Id == id);
        }

        public async Task<List<DailyQuizPlan>> GetPlansByUserIdAsync(string userId, bool trackChanges = false)
        {
            var query = _context.DailyQuizPlans.AsQueryable();
            if (!trackChanges)
                query = query.AsNoTracking();

            return await query
                .Where(p => p.UserId == userId)
                .OrderByDescending(p => p.IsActive)
                .ThenByDescending(p => p.UpdatedAt)
                .ToListAsync();
        }

        public async Task<DailyQuizPlan?> GetActivePlanByUserIdAsync(string userId, bool trackChanges = false)
        {
            var query = _context.DailyQuizPlans.AsQueryable();
            if (!trackChanges)
                query = query.AsNoTracking();

            var active = await query.FirstOrDefaultAsync(p => p.UserId == userId && p.IsActive);
            if (active != null)
                return active;

            return await query
                .OrderByDescending(p => p.UpdatedAt)
                .FirstOrDefaultAsync(p => p.UserId == userId);
        }

        public async Task<DailyQuizPlan?> GetPlanByUserIdAsync(string userId, bool trackChanges = false)
        {
            return await GetActivePlanByUserIdAsync(userId, trackChanges);
        }

        public void CreatePlan(DailyQuizPlan plan) => _context.DailyQuizPlans.Add(plan);

        public void UpdatePlan(DailyQuizPlan plan) => _context.DailyQuizPlans.Update(plan);

        public void DeletePlan(DailyQuizPlan plan) => _context.DailyQuizPlans.Remove(plan);

        public async Task SaveAsync() => await _context.SaveChangesAsync();
    }
}
