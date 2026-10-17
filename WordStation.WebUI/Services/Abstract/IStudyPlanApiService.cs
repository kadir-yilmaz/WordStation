using System.Collections.Generic;
using System.Threading.Tasks;
using WordStation.EL.Dtos;

namespace WordStation.WebUI.Services.Abstract
{
    public interface IStudyPlanApiService
    {
        Task<List<StudyPlanSummaryDto>> GetUserPlansAsync(string userId, string token);
        Task<StudyPlanSummaryDto?> CreatePlanAsync(CreateStudyPlanDto dto, string token);
        Task<bool> DeletePlanAsync(int planId, string userId, string token);
        Task<List<StudyPlanDayDto>> GetAllDaysAsync(int planId, string userId, string token);
        Task<StudyPlanDayDto?> GetDayAsync(int planId, int dayNumber, string userId, string token);
        Task<StudyPlanQuizDto?> GenerateQuizAsync(int planId, int dayNumber, string userId, string token);
        Task<StudyPlanDayDto?> SubmitQuizAsync(int planId, int dayNumber, SubmitPlanQuizDto dto, string token);
    }
}
