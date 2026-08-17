using System.Threading.Tasks;
using WordStation.EL.Models;

namespace WordStation.DAL.Abstract
{
    public interface IDailyQuizRepository
    {
        Task<DailyQuizPlan?> GetPlanByUserIdAsync(string userId, bool trackChanges = false);
        void CreatePlan(DailyQuizPlan plan);
        void UpdatePlan(DailyQuizPlan plan);
        void DeletePlan(DailyQuizPlan plan);
        Task SaveAsync();
    }
}
