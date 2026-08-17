using System.Threading.Tasks;
using WordStation.EL.Dtos;

namespace WordStation.BLL.Abstract
{
    public interface IDailyQuizService
    {
        Task<DailyQuizPlanDto?> GetActivePlanByUserIdAsync(string userId);
        Task<DailyQuizPlanDto> CreateOrResetPlanAsync(CreateDailyQuizPlanDto dto);
        Task<DailyQuizPlanDto?> UpdateProgressAsync(UpdateDailyQuizProgressDto dto);
        Task<bool> DeletePlanAsync(string userId);
    }
}
