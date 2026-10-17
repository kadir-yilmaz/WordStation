using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using WordStation.DAL.Abstract;
using WordStation.EL.Models;

namespace WordStation.DAL.EFCore
{
    public class StudyPlanRepository : IStudyPlanRepository
    {
        private readonly AppDbContext _context;

        public StudyPlanRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<StudyPlan>> GetPlansByUserIdAsync(string userId, bool trackChanges = false)
        {
            var query = _context.StudyPlans
                .Where(p => p.UserId == userId)
                .Include(p => p.IncludedLists)
                .Include(p => p.Days)
                    .ThenInclude(d => d.Words)
                .OrderByDescending(p => p.CreatedAt);

            return trackChanges
                ? await query.ToListAsync()
                : await query.AsNoTracking().ToListAsync();
        }

        public async Task<StudyPlan?> GetPlanByIdAsync(int planId, bool trackChanges = false)
        {
            var query = _context.StudyPlans
                .Where(p => p.Id == planId)
                .Include(p => p.IncludedLists)
                .Include(p => p.Days)
                    .ThenInclude(d => d.Words)
                        .ThenInclude(w => w.Word);

            return trackChanges
                ? await query.FirstOrDefaultAsync()
                : await query.AsNoTracking().FirstOrDefaultAsync();
        }

        public async Task<bool> IsPlanOwnerAsync(int planId, string userId)
        {
            return await _context.StudyPlans
                .AsNoTracking()
                .AnyAsync(p => p.Id == planId && p.UserId == userId);
        }

        public async Task<StudyPlanDay?> GetDayAsync(int planId, int dayNumber, bool trackChanges = false)
        {
            var query = _context.StudyPlanDays
                .Where(d => d.StudyPlanId == planId && d.DayNumber == dayNumber)
                .Include(d => d.Words)
                    .ThenInclude(w => w.Word);

            return trackChanges
                ? await query.FirstOrDefaultAsync()
                : await query.AsNoTracking().FirstOrDefaultAsync();
        }

        public async Task<List<StudyPlanDay>> GetAllDaysAsync(int planId, bool trackChanges = false)
        {
            var query = _context.StudyPlanDays
                .Where(d => d.StudyPlanId == planId)
                .Include(d => d.Words)
                    .ThenInclude(w => w.Word)
                .OrderBy(d => d.DayNumber);

            return trackChanges
                ? await query.ToListAsync()
                : await query.AsNoTracking().ToListAsync();
        }

        public void CreatePlan(StudyPlan plan) => _context.StudyPlans.Add(plan);

        public void UpdatePlan(StudyPlan plan) => _context.StudyPlans.Update(plan);

        public void DeletePlan(StudyPlan plan) => _context.StudyPlans.Remove(plan);

        public async Task SaveAsync() => await _context.SaveChangesAsync();
    }
}
