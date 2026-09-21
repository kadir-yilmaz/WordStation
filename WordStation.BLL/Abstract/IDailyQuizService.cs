using System.Collections.Generic;
using System.Threading.Tasks;
using WordStation.EL.Dtos;

namespace WordStation.BLL.Abstract
{
    public interface IDailyQuizService
    {
        Task<List<DailyQuizPlanDto>> GetAllPlansByUserIdAsync(string userId);
        Task<DailyQuizPlanDto?> GetPlanByIdAsync(int planId, string userId);
        Task<DailyQuizPlanDto?> GetActivePlanByUserIdAsync(string userId);
        Task<DailyQuizPlanDto> CreatePlanAsync(CreateDailyQuizPlanDto dto);
        Task<DailyQuizPlanDto> CreateOrResetPlanAsync(CreateDailyQuizPlanDto dto);
        Task<bool> SetActivePlanAsync(int planId, string userId);
        Task<DailyQuizPlanDto?> SelectBuffetWordsAsync(int planId, string userId, SelectBuffetWordsDto dto);
        Task<DailyQuizPlanDto?> ReturnWordToBuffetPoolAsync(int planId, string userId, int wordId);
        Task<DailyQuizPlanDto?> UpdateProgressAsync(UpdateDailyQuizProgressDto dto);
        Task<DailyQuizPlanDto?> ResetPlanProgressAsync(int planId, string userId);
        Task<bool> DeletePlanAsync(int planId, string userId);
        Task<bool> DeletePlanAsync(string userId);
    }
}
