using System.Collections.Generic;
using System.Threading.Tasks;
using WordStation.EL.Dtos;

namespace WordStation.BLL.Abstract
{
    public interface IQuizHistoryService
    {
        Task<List<QuizHistoryDto>> GetHistoryAsync(string userId, bool? isDailyQuiz = null);
        Task<QuizHistoryDto> SaveHistoryAsync(CreateQuizHistoryDto dto);
        Task<bool> ClearHistoryAsync(string userId, bool? isDailyQuiz = null);
    }
}
