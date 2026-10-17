using System.Collections.Generic;
using System.Threading.Tasks;
using WordStation.EL.Models;

namespace WordStation.DAL.Abstract
{
    public interface IStudyPlanRepository
    {
        Task<List<StudyPlan>> GetPlansByUserIdAsync(string userId, bool trackChanges = false);
        Task<StudyPlan?> GetPlanByIdAsync(int planId, bool trackChanges = false);
        Task<bool> IsPlanOwnerAsync(int planId, string userId);
        Task<StudyPlanDay?> GetDayAsync(int planId, int dayNumber, bool trackChanges = false);
        Task<StudyPlanDay?> GetDayByIdAsync(int dayId, bool trackChanges = false);
        Task<List<StudyPlanDay>> GetAllDaysAsync(int planId, bool trackChanges = false);
        void CreatePlan(StudyPlan plan);
        void UpdatePlan(StudyPlan plan);
        void DeletePlan(StudyPlan plan);
        Task SaveAsync();
    }
}
