using System.Threading.Tasks;
using WordStation.EL.Dtos;

namespace WordStation.BLL.Abstract
{
    public interface IDailyQuizService
    {
        Task<DailyQuizPlanDto?> GetPlanByUserIdAsync(string userId);
        Task<DailyQuizPlanDto> CreateOrResetPlanAsync(CreateDailyQuizPlanDto dto);
        Task<DailyQuizPlanDto?> UpdateProgressAsync(UpdateDailyQuizProgressDto dto);
        Task<bool> DeletePlanAsync(string userId);
        Task<System.Collections.Generic.List<DailyPlanDayHistoryDto>> GetDayHistoriesAsync(string userId);
        Task<DailyPlanDayHistoryDto?> SaveDayHistoryAsync(string userId, SaveDailyPlanDayDto dto);
    }
}
