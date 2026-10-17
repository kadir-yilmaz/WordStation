using System.Collections.Generic;
using System.Threading.Tasks;
using WordStation.EL.Dtos;

namespace WordStation.BLL.Abstract
{
    public interface IStudyPlanService
    {
        Task<List<StudyPlanSummaryDto>> GetUserPlansAsync(string userId);
        Task<StudyPlanSummaryDto> CreatePlanAsync(CreateStudyPlanDto dto);
        Task<bool> DeletePlanAsync(int planId, string userId);

        Task<StudyPlanDayDto?> GetCurrentDayAsync(int planId, string userId);
        Task<StudyPlanDayDto?> GetDayAsync(int planId, int dayNumber, string userId);
        Task<List<StudyPlanDayDto>> GetAllDaysAsync(int planId, string userId);

        Task<StudyPlanQuizDto> GenerateQuizAsync(int planId, int dayNumber, string userId);
        Task<StudyPlanDayDto> SubmitQuizAsync(SubmitPlanQuizDto dto);
    }
}
